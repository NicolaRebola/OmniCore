package handlers_test

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"

	"orders-service/adapters/http/dto"
	"orders-service/adapters/http/handlers"
	"orders-service/adapters/http/idempotency"
	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	"orders-service/internal/domain"
)

type hashGateIdempotencyStore struct {
	storedHash string
	replay     []byte
	saveCalls  int
}

func (s *hashGateIdempotencyStore) GetReplay(
	_ context.Context,
	_ uuid.UUID,
	_, _, requestHash string,
) ([]byte, bool, error) {
	if s.storedHash == "" {
		return nil, false, nil
	}
	if s.storedHash != requestHash {
		return nil, false, application.ErrIdempotencyConflict
	}
	return s.replay, true, nil
}

func (s *hashGateIdempotencyStore) SaveResponse(
	_ context.Context,
	_ uuid.UUID,
	_, _, _ string,
	_ []byte,
) error {
	s.saveCalls++
	return nil
}

type trackingIdempotencyStore struct {
	operation string
}

func (s *trackingIdempotencyStore) GetReplay(_ context.Context, _ uuid.UUID, _, _, _ string) ([]byte, bool, error) {
	return nil, false, nil
}

func (s *trackingIdempotencyStore) SaveResponse(
	_ context.Context,
	_ uuid.UUID,
	_, operation, _ string,
	_ []byte,
) error {
	s.operation = operation
	return nil
}

type countingCreateOrder struct {
	calls int
}

func (c *countingCreateOrder) Execute(_ context.Context, _ inboundports.CreateOrderCommand) (inboundports.CreateOrderResult, error) {
	c.calls++
	order, err := domain.NewOrder(uuid.New(), testTenantID, domain.SourcePOS, domain.FulfillmentTakeaway, testNow)
	if err != nil {
		return inboundports.CreateOrderResult{}, err
	}
	return inboundports.CreateOrderResult{Order: order}, nil
}

type countingPlaceOrder struct {
	calls int
}

func (c *countingPlaceOrder) Execute(_ context.Context, _ inboundports.PlaceOrderCommand) (inboundports.PlaceOrderResult, error) {
	c.calls++
	order, _ := domain.NewOrder(testOrderID, testTenantID, domain.SourcePOS, domain.FulfillmentTakeaway, testNow)
	orderNumber := int64(1)
	order.OrderNumber = &orderNumber
	order.Status = domain.StatusPlaced
	return inboundports.PlaceOrderResult{Order: order}, nil
}

type countingCreateAndPlace struct {
	calls int
}

func (c *countingCreateAndPlace) Execute(_ context.Context, _ inboundports.CreateAndPlaceCommand) (inboundports.CreateAndPlaceResult, error) {
	c.calls++
	order, err := domain.NewOrder(uuid.New(), testTenantID, domain.SourcePOS, domain.FulfillmentTakeaway, testNow)
	if err != nil {
		return inboundports.CreateAndPlaceResult{}, err
	}
	orderNumber := int64(1)
	order.OrderNumber = &orderNumber
	order.Status = domain.StatusPlaced
	return inboundports.CreateAndPlaceResult{Order: order}, nil
}

func TestCreate_Replay_ReturnsStoredResponseWithoutExecuting(t *testing.T) {
	body := []byte(`{"source":"POS","fulfillmentType":"TAKEAWAY"}`)
	stored, _ := json.Marshal(dto.OrderResponse{ID: testOrderID, Status: string(domain.StatusDraft)})
	store := &hashGateIdempotencyStore{
		storedHash: idempotency.HashRequestBody(body),
		replay:     stored,
	}
	create := &countingCreateOrder{}
	h := handlers.NewOrderHandlers(
		create, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders", bytes.NewReader(body))
	req = withTenant(req)
	req.Header.Set("Idempotency-Key", "create-key-1")
	rec := httptest.NewRecorder()

	h.Create(rec, req)

	if rec.Code != http.StatusCreated {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	if create.calls != 0 {
		t.Fatalf("create calls = %d", create.calls)
	}
}

func TestCreate_IdempotencyConflict_ReturnsORDAPP006(t *testing.T) {
	body := []byte(`{"source":"POS","fulfillmentType":"TAKEAWAY"}`)
	store := &hashGateIdempotencyStore{storedHash: "other-hash"}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders", bytes.NewReader(body))
	req = withTenant(req)
	req.Header.Set("Idempotency-Key", "create-key-1")
	rec := httptest.NewRecorder()

	h.Create(rec, req)

	if rec.Code != http.StatusConflict {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-006")
}

func TestPlace_IdempotencyConflict_ReturnsORDAPP006(t *testing.T) {
	body, _ := json.Marshal(dto.ActorRequest{ActorType: "staff"})
	store := &hashGateIdempotencyStore{storedHash: "other-hash"}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/place", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	req.Header.Set("Idempotency-Key", "place-key-1")
	rec := httptest.NewRecorder()

	h.Place(rec, req)

	if rec.Code != http.StatusConflict {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-006")
}

func TestPlace_Replay_DoesNotExecutePlace(t *testing.T) {
	body, _ := json.Marshal(dto.ActorRequest{ActorType: "staff"})
	stored, _ := json.Marshal(dto.OrderResponse{ID: testOrderID, Status: string(domain.StatusPlaced)})
	store := &hashGateIdempotencyStore{
		storedHash: idempotency.HashScopedRequest(testOrderID.String(), body),
		replay:     stored,
	}
	place := &countingPlaceOrder{}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		place, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/place", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	req.Header.Set("Idempotency-Key", "place-key-1")
	rec := httptest.NewRecorder()

	h.Place(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	if place.calls != 0 {
		t.Fatalf("place calls = %d", place.calls)
	}
}

func TestPlace_Success_SavesPlaceOrderOperation(t *testing.T) {
	body, _ := json.Marshal(dto.ActorRequest{ActorType: "staff", ActorID: "staff-1"})
	store := &trackingIdempotencyStore{}
	place := &countingPlaceOrder{}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		place, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/"+testOrderID.String()+"/place", bytes.NewReader(body))
	req = withTenant(req)
	req = chiRoute(req, "id", testOrderID.String())
	req.Header.Set("Idempotency-Key", "place-key-1")
	rec := httptest.NewRecorder()

	h.Place(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	if store.operation != idempotency.PlaceOrderOperation {
		t.Fatalf("operation = %q", store.operation)
	}
}

func TestCreateAndPlace_Replay_ReturnsStoredResponseWithoutExecuting(t *testing.T) {
	body := []byte(`{"source":"POS","fulfillmentType":"TAKEAWAY","customer":{"name":"Walk-in"},"lines":[{"variantId":"dddddddd-0000-0000-0000-000000000001","quantity":1}],"actorType":"staff"}`)
	stored, _ := json.Marshal(dto.OrderResponse{ID: testOrderID, Status: string(domain.StatusPlaced)})
	store := &hashGateIdempotencyStore{
		storedHash: idempotency.HashRequestBody(body),
		replay:     stored,
	}
	createAndPlace := &countingCreateAndPlace{}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, createAndPlace, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/create-and-place", bytes.NewReader(body))
	req = withTenant(req)
	req.Header.Set("Idempotency-Key", "cap-key-1")
	rec := httptest.NewRecorder()

	h.CreateAndPlace(rec, req)

	if rec.Code != http.StatusCreated {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	if createAndPlace.calls != 0 {
		t.Fatalf("createAndPlace calls = %d", createAndPlace.calls)
	}
}

func TestCreateAndPlace_IdempotencyConflict_ReturnsORDAPP006(t *testing.T) {
	body := []byte(`{"source":"POS","fulfillmentType":"TAKEAWAY","customer":{"name":"Walk-in"},"lines":[{"variantId":"dddddddd-0000-0000-0000-000000000001","quantity":1}],"actorType":"staff"}`)
	store := &hashGateIdempotencyStore{storedHash: "other-hash"}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, stubCreateAndPlace{}, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/create-and-place", bytes.NewReader(body))
	req = withTenant(req)
	req.Header.Set("Idempotency-Key", "cap-key-1")
	rec := httptest.NewRecorder()

	h.CreateAndPlace(rec, req)

	if rec.Code != http.StatusConflict {
		t.Fatalf("status = %d", rec.Code)
	}
	assertProblemCode(t, rec, "ORD-APP-006")
}

func TestCreateAndPlace_Success_SavesCreateAndPlaceOperation(t *testing.T) {
	variantID := uuid.MustParse("dddddddd-0000-0000-0000-000000000001")
	body, _ := json.Marshal(dto.CreateAndPlaceRequest{
		Source:          "POS",
		FulfillmentType: "TAKEAWAY",
		Customer:        dto.CustomerRequest{Name: "Walk-in"},
		Lines:           []dto.CreateAndPlaceLineRequest{{VariantID: variantID, Quantity: 1}},
		ActorType:       "staff",
	})
	store := &trackingIdempotencyStore{}
	createAndPlace := &countingCreateAndPlace{}
	h := handlers.NewOrderHandlers(
		stubCreateOrder{}, createAndPlace, stubAddLine{}, stubUpdateLineQty{}, stubRemoveLine{},
		stubSetCustomer{}, stubSetAddress{}, stubSetFulfillment{}, stubSetComments{},
		stubPlaceOrder{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{}, stubLifecycle{},
		stubGetOrder{}, stubListOrders{},
		store,
	)

	req := httptest.NewRequest(http.MethodPost, "/api/v1/orders/create-and-place", bytes.NewReader(body))
	req = withTenant(req)
	req.Header.Set("Idempotency-Key", "cap-key-1")
	rec := httptest.NewRecorder()

	h.CreateAndPlace(rec, req)

	if rec.Code != http.StatusCreated {
		t.Fatalf("status = %d body=%s", rec.Code, rec.Body.String())
	}
	if store.operation != idempotency.CreateAndPlaceOperation {
		t.Fatalf("operation = %q", store.operation)
	}
}
