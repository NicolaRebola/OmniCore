package usecases_test

import (
	"context"
	"testing"
	"time"

	"github.com/google/uuid"

	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	outboundports "orders-service/internal/application/ports/outbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/application/usecases"
	"orders-service/internal/domain"
)

var (
	testNow      = time.Date(2026, 6, 19, 12, 0, 0, 0, time.UTC)
	testTenantID = uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")
	testOrderID  = uuid.MustParse("bbbbbbbb-0000-0000-0000-000000000001")
	testLineID   = uuid.MustParse("cccccccc-0000-0000-0000-000000000001")
	testVariant  = uuid.MustParse("dddddddd-0000-0000-0000-000000000001")
)

type fakeClock struct{}

func (fakeClock) Now() time.Time { return testNow }

type fakeOrderRepo struct {
	order      *domain.Order
	saveCalled bool
}

func (r *fakeOrderRepo) GetByID(_ context.Context, _, _ uuid.UUID) (*domain.Order, error) {
	return r.order, nil
}

func (r *fakeOrderRepo) Save(_ context.Context, _ *domain.Order) error {
	r.saveCalled = true
	return nil
}

func (r *fakeOrderRepo) List(_ context.Context, _ postgresports.ListOrdersFilter) (postgresports.ListOrdersPage, error) {
	return postgresports.ListOrdersPage{}, nil
}

type fakeSequenceRepo struct {
	next int64
}

func (r *fakeSequenceRepo) NextOrderNumber(_ context.Context, _ uuid.UUID) (int64, error) {
	r.next++
	return r.next, nil
}

type fakeCatalog struct {
	snapshots []domain.VariantSnapshot
	err       error
}

func (f *fakeCatalog) ValidateAndResolve(
	_ context.Context,
	_ uuid.UUID,
	_ []uuid.UUID,
) ([]domain.VariantSnapshot, error) {
	return f.snapshots, f.err
}

type fakeEvents struct {
	published bool
}

func (f *fakeEvents) Publish(_ context.Context, _ outboundports.IntegrationEvent) error {
	f.published = true
	return nil
}

type fakeTx struct{}

func (fakeTx) WithinTransaction(ctx context.Context, fn func(context.Context) error) error {
	return fn(ctx)
}

func draftOrderForPlace(t *testing.T) *domain.Order {
	t.Helper()
	order, err := domain.NewOrder(testOrderID, testTenantID, domain.SourcePOS, domain.FulfillmentTakeaway, testNow)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(testLineID, testVariant, 2, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(domain.CustomerSnapshot{Name: "Ana"}, testNow); err != nil {
		t.Fatal(err)
	}
	return order
}

func validSnapshot() domain.VariantSnapshot {
	price, _ := domain.NewMoney(1500, domain.DefaultCurrency)
	return domain.VariantSnapshot{
		VariantID:     testVariant,
		CatalogItemID: uuid.New(),
		Name:          "Burger",
		UnitPrice:     price,
		IsActive:      true,
	}
}

func newHandler(
	orders postgresports.OrderRepository,
	catalog outboundports.CatalogReferenceValidator,
	events outboundports.EventPublisher,
) inboundports.PlaceOrder {
	return usecases.NewPlaceOrderHandler(
		orders,
		&fakeSequenceRepo{},
		catalog,
		events,
		fakeTx{},
		fakeClock{},
	)
}

func TestPlaceOrder_HappyPath(t *testing.T) {
	orders := &fakeOrderRepo{order: draftOrderForPlace(t)}
	events := &fakeEvents{}

	result, err := newHandler(orders, &fakeCatalog{snapshots: []domain.VariantSnapshot{validSnapshot()}}, events).
		Execute(context.Background(), inboundports.PlaceOrderCommand{
			TenantID: testTenantID,
			OrderID:  testOrderID,
			Actor:    domain.Actor{Type: domain.ActorStaff},
		})
	if err != nil {
		t.Fatal(err)
	}
	if result.Order.Status != domain.StatusPlaced {
		t.Fatalf("status = %s", result.Order.Status)
	}
	if !orders.saveCalled {
		t.Fatal("expected Save to be called")
	}
	if !events.published {
		t.Fatal("expected event to be published")
	}
}

func TestPlaceOrder_CatalogDown_DoesNotSave(t *testing.T) {
	orders := &fakeOrderRepo{order: draftOrderForPlace(t)}

	_, err := newHandler(orders, &fakeCatalog{err: application.ErrCatalogUnavailable}, &fakeEvents{}).
		Execute(context.Background(), inboundports.PlaceOrderCommand{
			TenantID: testTenantID,
			OrderID:  testOrderID,
			Actor:    domain.Actor{Type: domain.ActorStaff},
		})
	if !application.HasAppCode(err, "ORD-APP-004") {
		t.Fatalf("expected ORD-APP-004, got %v", err)
	}
	if orders.saveCalled {
		t.Fatal("Save must not be called when catalog fails")
	}
}

func TestPlaceOrder_OrderNotFound(t *testing.T) {
	orders := &fakeOrderRepo{order: nil}

	_, err := newHandler(orders, &fakeCatalog{}, &fakeEvents{}).
		Execute(context.Background(), inboundports.PlaceOrderCommand{
			TenantID: testTenantID,
			OrderID:  testOrderID,
			Actor:    domain.Actor{Type: domain.ActorStaff},
		})
	if !application.HasAppCode(err, "ORD-APP-001") {
		t.Fatalf("expected ORD-APP-001, got %v", err)
	}
}

func TestPlaceOrder_DomainError_DoesNotSave(t *testing.T) {
	order, err := domain.NewOrder(testOrderID, testTenantID, domain.SourcePOS, domain.FulfillmentTakeaway, testNow)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(testLineID, testVariant, 1, testNow); err != nil {
		t.Fatal(err)
	}

	orders := &fakeOrderRepo{order: order}

	_, err = newHandler(orders, &fakeCatalog{snapshots: []domain.VariantSnapshot{validSnapshot()}}, &fakeEvents{}).
		Execute(context.Background(), inboundports.PlaceOrderCommand{
			TenantID: testTenantID,
			OrderID:  testOrderID,
			Actor:    domain.Actor{Type: domain.ActorStaff},
		})
	if !domain.HasDomainCode(err, "ORD-DOM-005") {
		t.Fatalf("expected ORD-DOM-005, got %v", err)
	}
	if orders.saveCalled {
		t.Fatal("Save must not be called on domain error")
	}
}
