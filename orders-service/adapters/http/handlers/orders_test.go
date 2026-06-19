package handlers_test

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/google/uuid"

	"orders-service/adapters/http/dto"
	"orders-service/adapters/http/handlers"
	httpmw "orders-service/adapters/http/middleware"
	inboundports "orders-service/internal/application/ports/inbound"
	"orders-service/internal/domain"
)

var (
	testTenantID = uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")
	testOrderID  = uuid.MustParse("bbbbbbbb-0000-0000-0000-000000000001")
	testNow      = time.Date(2026, 6, 19, 12, 0, 0, 0, time.UTC)
)

type stubCreateOrder struct {
	result inboundports.CreateOrderResult
	err    error
}

func (s stubCreateOrder) Execute(_ context.Context, _ inboundports.CreateOrderCommand) (inboundports.CreateOrderResult, error) {
	return s.result, s.err
}

type stubAddLine struct {
	err error
}

func (s stubAddLine) Execute(_ context.Context, _ inboundports.AddLineCommand) (inboundports.OrderMutationResult, error) {
	return inboundports.OrderMutationResult{}, s.err
}

type stubUpdateLineQty struct{ err error }

func (s stubUpdateLineQty) Execute(_ context.Context, _ inboundports.UpdateLineQuantityCommand) (inboundports.OrderMutationResult, error) {
	return inboundports.OrderMutationResult{}, s.err
}

type stubRemoveLine struct{ err error }

func (s stubRemoveLine) Execute(_ context.Context, _ inboundports.RemoveLineCommand) (inboundports.OrderMutationResult, error) {
	return inboundports.OrderMutationResult{}, s.err
}

type stubSetCustomer struct{ err error }

func (s stubSetCustomer) Execute(_ context.Context, _ inboundports.SetCustomerCommand) (inboundports.OrderMutationResult, error) {
	return inboundports.OrderMutationResult{}, s.err
}

type stubSetAddress struct{ err error }

func (s stubSetAddress) Execute(_ context.Context, _ inboundports.SetAddressCommand) (inboundports.OrderMutationResult, error) {
	return inboundports.OrderMutationResult{}, s.err
}

type stubSetFulfillment struct{ err error }

func (s stubSetFulfillment) Execute(_ context.Context, _ inboundports.SetFulfillmentTypeCommand) (inboundports.OrderMutationResult, error) {
	return inboundports.OrderMutationResult{}, s.err
}

type stubSetComments struct{ err error }

func (s stubSetComments) Execute(_ context.Context, _ inboundports.SetCommentsCommand) (inboundports.OrderMutationResult, error) {
	return inboundports.OrderMutationResult{}, s.err
}

type noopIdempotencyStore struct{}

func (noopIdempotencyStore) GetReplay(_ context.Context, _ uuid.UUID, _, _, _ string) ([]byte, bool, error) {
	return nil, false, nil
}

func (noopIdempotencyStore) SaveResponse(_ context.Context, _ uuid.UUID, _, _, _ string, _ []byte) error {
	return nil
}

func TestCreate_MissingTenant_ReturnsORDAPP002(t *testing.T) {
	h := newTestHandlers(stubCreateOrder{})

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders", bytes.NewReader([]byte(`{"source":"POS","fulfillmentType":"TAKEAWAY"}`)))
	req.Header.Set("Idempotency-Key", "key-1")
	rec := httptest.NewRecorder()

	h.Create(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-002")
}

func TestCreate_MissingIdempotencyKey_ReturnsORDAPP005(t *testing.T) {
	h := newTestHandlers(stubCreateOrder{})

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders", bytes.NewReader([]byte(`{"source":"POS","fulfillmentType":"TAKEAWAY"}`)))
	req = withTenant(req)
	rec := httptest.NewRecorder()

	h.Create(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-005")
}

func TestAddLine_WhenPlaced_ReturnsORDDOM002(t *testing.T) {
	h := newTestHandlers(stubCreateOrder{}, stubAddLine{err: domain.ErrMutationNotInDraft})

	body, _ := json.Marshal(dto.AddLineRequest{VariantID: uuid.New(), Quantity: 1})
	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/lines", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	rec := httptest.NewRecorder()

	h.AddLine(rec, req)

	if rec.Code != http.StatusUnprocessableEntity {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	assertProblemCode(t, rec, "ORD-DOM-002")
}

func TestTenantRequired_MissingHeader(t *testing.T) {
	req := httptest.NewRequest(http.MethodGet, "/api/v1/orders", nil)
	rec := httptest.NewRecorder()

	handler := httpmw.TenantRequired()(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
	}))
	handler.ServeHTTP(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-002")
}

func withTenant(req *http.Request) *http.Request {
	return req.WithContext(httpmw.ContextWithTenantID(req.Context(), testTenantID))
}

func chiRoute(req *http.Request, key, value string) *http.Request {
	rctx := chi.NewRouteContext()
	rctx.URLParams.Add(key, value)
	return req.WithContext(context.WithValue(req.Context(), chi.RouteCtxKey, rctx))
}

func newTestHandlers(create inboundports.CreateOrder, addLine ...stubAddLine) *handlers.OrderHandlers {
	add := stubAddLine{}
	if len(addLine) > 0 {
		add = addLine[0]
	}
	return handlers.NewOrderHandlers(
		create, add, stubUpdateLineQty{}, stubRemoveLine{}, stubSetCustomer{},
		stubSetAddress{}, stubSetFulfillment{}, stubSetComments{}, noopIdempotencyStore{},
	)
}

func assertProblemCode(t *testing.T, rec *httptest.ResponseRecorder, code string) {
	t.Helper()

	var body map[string]any
	if err := json.Unmarshal(rec.Body.Bytes(), &body); err != nil {
		t.Fatalf("unmarshal problem: %v", err)
	}
	if body["errorCode"] != code {
		t.Fatalf("errorCode = %v", body["errorCode"])
	}
	if ct := rec.Header().Get("Content-Type"); ct != "application/problem+json" {
		t.Fatalf("content-type = %q", ct)
	}
}
