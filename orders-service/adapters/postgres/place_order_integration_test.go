//go:build integration

package postgres

import (
	"context"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/google/uuid"

	catalogadapter "orders-service/adapters/catalog"
	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	outboundports "orders-service/internal/application/ports/outbound"
	"orders-service/internal/application/usecases"
	"orders-service/internal/domain"
)

type fixedClock struct{}

func (fixedClock) Now() time.Time { return integrationNow }

type spyEventPublisher struct {
	events []outboundports.IntegrationEvent
}

func (s *spyEventPublisher) Publish(_ context.Context, event outboundports.IntegrationEvent) error {
	s.events = append(s.events, event)
	return nil
}

func TestPlaceOrder_Integration_HappyPath(t *testing.T) {
	pool := requireIntegrationDB(t)
	ctx := context.Background()

	tenantID := uuid.New()
	orderID := uuid.New()
	lineID := uuid.New()
	variantID := uuid.New()
	catalogItemID := uuid.New()

	orderRepo := NewOrderRepository(pool)
	order, err := domain.NewOrder(orderID, tenantID, domain.SourcePOS, domain.FulfillmentTakeaway, integrationNow)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(lineID, variantID, 2, integrationNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(domain.CustomerSnapshot{Name: "Ana"}, integrationNow); err != nil {
		t.Fatal(err)
	}
	if err := orderRepo.Save(ctx, order); err != nil {
		t.Fatalf("save draft: %v", err)
	}

	catalogServer := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if r.Header.Get("X-Tenant-Id") != tenantID.String() {
			t.Errorf("tenant header = %q", r.Header.Get("X-Tenant-Id"))
		}
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{
			"categories": [{
				"items": [{
					"variantId": "` + variantID.String() + `",
					"itemId": "` + catalogItemID.String() + `",
					"variantName": "Burger",
					"status": "active",
					"price": { "amount": 15.0, "currency": "ARS" }
				}]
			}]
		}`))
	}))
	defer catalogServer.Close()

	events := &spyEventPublisher{}
	handler := usecases.NewPlaceOrderHandler(
		orderRepo,
		NewTenantSequenceRepository(pool),
		catalogadapter.NewClient(catalogServer.URL),
		events,
		NewTransactionManager(pool),
		fixedClock{},
	)

	result, err := handler.Execute(ctx, inboundports.PlaceOrderCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		Actor:    domain.Actor{Type: domain.ActorStaff, ID: "staff-1"},
	})
	if err != nil {
		t.Fatalf("place: %v", err)
	}

	if result.Order.Status != domain.StatusPlaced {
		t.Fatalf("status = %s", result.Order.Status)
	}
	if result.Order.OrderNumber == nil || *result.Order.OrderNumber != 1 {
		t.Fatalf("orderNumber = %v", result.Order.OrderNumber)
	}

	loaded, err := orderRepo.GetByID(ctx, tenantID, orderID)
	if err != nil {
		t.Fatalf("get: %v", err)
	}
	if loaded.Status != domain.StatusPlaced {
		t.Fatalf("db status = %s", loaded.Status)
	}
	if loaded.OrderNumber == nil || *loaded.OrderNumber != 1 {
		t.Fatalf("db orderNumber = %v", loaded.OrderNumber)
	}

	line := loaded.Lines()[0]
	if line.Name == nil || *line.Name != "Burger" {
		t.Fatal("snapshot name not persisted")
	}
	if line.CatalogItemID == nil || *line.CatalogItemID != catalogItemID {
		t.Fatalf("catalogItemID = %v", line.CatalogItemID)
	}
	if line.UnitPrice == nil || line.UnitPrice.Amount != 1500 {
		t.Fatalf("unit price = %v", line.UnitPrice)
	}
	if line.LineTotal == nil || line.LineTotal.Amount != 3000 {
		t.Fatalf("line total = %v", line.LineTotal)
	}
	if loaded.Totals == nil || loaded.Totals.Subtotal.Amount != 3000 {
		t.Fatalf("totals = %+v", loaded.Totals)
	}
	if len(loaded.Transitions()) != 1 || loaded.Transitions()[0].ToStatus != domain.StatusPlaced {
		t.Fatalf("transitions = %+v", loaded.Transitions())
	}

	if len(events.events) != 1 {
		t.Fatalf("events = %d", len(events.events))
	}
	if events.events[0].EventType != "order.placed" {
		t.Fatalf("event type = %s", events.events[0].EventType)
	}
}

func TestPlaceOrder_Integration_CatalogDown_DoesNotPersist(t *testing.T) {
	pool := requireIntegrationDB(t)
	ctx := context.Background()

	tenantID := uuid.New()
	orderID := uuid.New()
	lineID := uuid.New()
	variantID := uuid.New()

	orderRepo := NewOrderRepository(pool)
	order, err := domain.NewOrder(orderID, tenantID, domain.SourcePOS, domain.FulfillmentTakeaway, integrationNow)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(lineID, variantID, 1, integrationNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(domain.CustomerSnapshot{Name: "Ana"}, integrationNow); err != nil {
		t.Fatal(err)
	}
	if err := orderRepo.Save(ctx, order); err != nil {
		t.Fatalf("save draft: %v", err)
	}

	catalogServer := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
		http.Error(w, "catalog unavailable", http.StatusServiceUnavailable)
	}))
	defer catalogServer.Close()

	events := &spyEventPublisher{}
	handler := usecases.NewPlaceOrderHandler(
		orderRepo,
		NewTenantSequenceRepository(pool),
		catalogadapter.NewClient(catalogServer.URL),
		events,
		NewTransactionManager(pool),
		fixedClock{},
	)

	_, err = handler.Execute(ctx, inboundports.PlaceOrderCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		Actor:    domain.Actor{Type: domain.ActorStaff},
	})
	if !application.HasAppCode(err, "ORD-APP-004") {
		t.Fatalf("expected ORD-APP-004, got %v", err)
	}

	loaded, err := orderRepo.GetByID(ctx, tenantID, orderID)
	if err != nil {
		t.Fatalf("get: %v", err)
	}
	if loaded.Status != domain.StatusDraft {
		t.Fatalf("expected Draft, got %s", loaded.Status)
	}
	if loaded.OrderNumber != nil {
		t.Fatalf("orderNumber should be nil, got %v", loaded.OrderNumber)
	}
	if loaded.Lines()[0].Name != nil {
		t.Fatal("line snapshot should not be frozen")
	}
	if len(events.events) != 0 {
		t.Fatalf("expected no events published, got %d", len(events.events))
	}
}
