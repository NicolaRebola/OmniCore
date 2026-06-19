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
	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	"orders-service/internal/domain"
)

var (
	testTenantID = uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")
	testOrderID  = uuid.MustParse("bbbbbbbb-0000-0000-0000-000000000001")
	testNow      = time.Date(2026, 6, 19, 12, 0, 0, 0, time.UTC)
)

type stubCreateAndPlace struct {
	result inboundports.CreateAndPlaceResult
	err    error
}

func (s stubCreateAndPlace) Execute(_ context.Context, _ inboundports.CreateAndPlaceCommand) (inboundports.CreateAndPlaceResult, error) {
	return s.result, s.err
}

type stubPlaceOrder struct {
	result inboundports.PlaceOrderResult
	err    error
}

func (s stubPlaceOrder) Execute(_ context.Context, _ inboundports.PlaceOrderCommand) (inboundports.PlaceOrderResult, error) {
	return s.result, s.err
}

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

type stubLifecycle struct {
	result inboundports.OrderMutationResult
	err    error
}

func (s stubLifecycle) Execute(_ context.Context, _ inboundports.LifecycleTransitionCommand) (inboundports.OrderMutationResult, error) {
	return s.result, s.err
}

type stubGetOrder struct {
	result inboundports.GetOrderResult
	err    error
}

func (s stubGetOrder) Execute(_ context.Context, _ inboundports.GetOrderQuery) (inboundports.GetOrderResult, error) {
	return s.result, s.err
}

type stubListOrders struct {
	result inboundports.ListOrdersResult
	err    error
}

func (s stubListOrders) Execute(_ context.Context, _ inboundports.ListOrdersQuery) (inboundports.ListOrdersResult, error) {
	return s.result, s.err
}

type recordingIdempotencyStore struct {
	getCalls  int
	saveCalls int
	replay    []byte
}

func (s *recordingIdempotencyStore) GetReplay(_ context.Context, _ uuid.UUID, _, _, _ string) ([]byte, bool, error) {
	s.getCalls++
	if s.replay != nil {
		return s.replay, true, nil
	}
	return nil, false, nil
}

func (s *recordingIdempotencyStore) SaveResponse(_ context.Context, _ uuid.UUID, _, _, _ string, _ []byte) error {
	s.saveCalls++
	return nil
}

type noopIdempotencyStore struct{}

func (noopIdempotencyStore) GetReplay(_ context.Context, _ uuid.UUID, _, _, _ string) ([]byte, bool, error) {
	return nil, false, nil
}

func (noopIdempotencyStore) SaveResponse(_ context.Context, _ uuid.UUID, _, _, _ string, _ []byte) error {
	return nil
}

func TestPlace_MissingIdempotencyKey_ReturnsORDAPP005(t *testing.T) {
	h := newTestHandlers(stubCreateOrder{}, stubPlaceOrder{})

	body, _ := json.Marshal(dto.ActorRequest{ActorType: "staff"})
	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/place", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	rec := httptest.NewRecorder()

	h.Place(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-005")
}

func TestPlace_Replay_ReturnsStoredResponse(t *testing.T) {
	stored, _ := json.Marshal(dto.OrderResponse{ID: testOrderID, Status: string(domain.StatusPlaced)})
	store := &recordingIdempotencyStore{replay: stored}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	body, _ := json.Marshal(dto.ActorRequest{ActorType: "staff"})
	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/place", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	req.Header.Set("Idempotency-Key", "place-key-1")
	rec := httptest.NewRecorder()

	h.Place(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	if store.getCalls != 1 {
		t.Fatalf("getCalls = %d", store.getCalls)
	}
}

func TestCreateAndPlace_MissingIdempotencyKey_ReturnsORDAPP005(t *testing.T) {
	h := newTestHandlers(stubCreateOrder{}, stubCreateAndPlace{})

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/create-and-place", bytes.NewReader([]byte(`{}`)))
	req = withTenant(req)
	rec := httptest.NewRecorder()

	h.CreateAndPlace(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-005")
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

func TestAccept_FromDraft_ReturnsORDDOM001(t *testing.T) {
	h := newLifecycleTestHandlers(stubLifecycle{err: domain.ErrInvalidTransition})

	body, _ := json.Marshal(dto.ActorRequest{ActorType: "staff"})
	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/accept", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	rec := httptest.NewRecorder()

	h.Accept(rec, req)

	if rec.Code != http.StatusUnprocessableEntity {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	assertProblemCode(t, rec, "ORD-DOM-001")
}

func TestCancel_FromPlaced_WithoutReason_ReturnsORDDOM007(t *testing.T) {
	h := newLifecycleTestHandlers(stubLifecycle{err: domain.ErrCancelReasonRequired})

	body, _ := json.Marshal(dto.ActorRequest{ActorType: "staff"})
	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/cancel", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	rec := httptest.NewRecorder()

	h.Cancel(rec, req)

	if rec.Code != http.StatusUnprocessableEntity {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	assertProblemCode(t, rec, "ORD-DOM-007")
}

func TestGetByID_NotFound_ReturnsORDAPP001(t *testing.T) {
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{err: application.ErrOrderNotFound}, stubListOrders{},
		noopIdempotencyStore{},
	)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/orders/"+testOrderID.String(), nil)
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	rec := httptest.NewRecorder()

	h.GetByID(rec, req)

	if rec.Code != http.StatusNotFound {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	assertProblemCode(t, rec, "ORD-APP-001")
}

func TestGetByID_ReturnsOrder(t *testing.T) {
	orderNum := int64(42)
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{result: inboundports.GetOrderResult{Order: &domain.Order{
			ID: testOrderID, TenantID: testTenantID, Status: domain.StatusPlaced,
			OrderNumber: &orderNum, CreatedAt: testNow, UpdatedAt: testNow,
		}}},
		stubListOrders{},
		noopIdempotencyStore{},
	)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/orders/"+testOrderID.String(), nil)
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	rec := httptest.NewRecorder()

	h.GetByID(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}

	var resp dto.OrderResponse
	if err := json.Unmarshal(rec.Body.Bytes(), &resp); err != nil {
		t.Fatal(err)
	}
	if resp.ID != testOrderID || resp.Status != string(domain.StatusPlaced) {
		t.Fatalf("resp = %+v", resp)
	}
	if resp.OrderNumber == nil || *resp.OrderNumber != 42 {
		t.Fatalf("orderNumber = %v", resp.OrderNumber)
	}
}

func TestList_DefaultPagination(t *testing.T) {
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{},
		stubListOrders{result: inboundports.ListOrdersResult{
			Orders: []*domain.Order{{ID: testOrderID, TenantID: testTenantID, Status: domain.StatusDraft, CreatedAt: testNow, UpdatedAt: testNow}},
			Total:  1,
		}},
		noopIdempotencyStore{},
	)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/orders", nil)
	req = withTenant(req)
	rec := httptest.NewRecorder()

	h.List(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}

	var resp dto.ListOrdersResponse
	if err := json.Unmarshal(rec.Body.Bytes(), &resp); err != nil {
		t.Fatal(err)
	}
	if resp.Page != 1 || resp.PageSize != 20 {
		t.Fatalf("pagination = %+v", resp)
	}
	if resp.Total != 1 || len(resp.Items) != 1 {
		t.Fatalf("items = %+v total=%d", resp.Items, resp.Total)
	}
}

func TestList_InvalidPageSize_ReturnsORDAPI009(t *testing.T) {
	h := newTestHandlers(stubCreateOrder{})

	req := httptest.NewRequest(http.MethodGet, "/api/v1/orders?page=1&pageSize=101", nil)
	req = withTenant(req)
	rec := httptest.NewRecorder()

	h.List(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-API-009")
}

func TestList_InvalidStatus_ReturnsORDAPI010(t *testing.T) {
	h := newTestHandlers(stubCreateOrder{})

	req := httptest.NewRequest(http.MethodGet, "/api/v1/orders?status=unknown", nil)
	req = withTenant(req)
	rec := httptest.NewRecorder()

	h.List(rec, req)

	if rec.Code != http.StatusBadRequest {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-API-010")
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

func newLifecycleTestHandlers(lifecycle stubLifecycle) *handlers.OrderHandlers {
	return handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{}, stubSetCustomer{},
		stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, lifecycle, lifecycle, lifecycle, lifecycle,
		stubGetOrder{}, stubListOrders{},
		noopIdempotencyStore{},
	)
}

func newTestHandlers(create inboundports.CreateOrder, extras ...any) *handlers.OrderHandlers {
	add := stubAddLine{}
	place := stubPlaceOrder{}
	createAndPlace := stubCreateAndPlace{}
	for _, extra := range extras {
		switch v := extra.(type) {
		case stubAddLine:
			add = v
		case stubPlaceOrder:
			place = v
		case stubCreateAndPlace:
			createAndPlace = v
		}
	}
	return handlers.NewOrderHandlers(
		create, createAndPlace, add, stubUpdateLineQty{}, stubRemoveLine{}, stubSetCustomer{},
		stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		place, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		noopIdempotencyStore{},
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
