package usecases_test

import (
	"context"
	"testing"

	"github.com/google/uuid"

	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	"orders-service/internal/application/usecases"
	"orders-service/internal/domain"
)

func TestCreateOrderHandler_CreatesDraft(t *testing.T) {
	repo := &fakeOrderRepo{}
	handler := usecases.NewCreateOrderHandler(repo, fakeClock{})

	result, err := handler.Execute(context.Background(), inboundports.CreateOrderCommand{
		TenantID:        testTenantID,
		Source:          domain.SourcePOS,
		FulfillmentType: domain.FulfillmentTakeaway,
	})
	if err != nil {
		t.Fatalf("Execute() error = %v", err)
	}
	if !repo.saveCalled {
		t.Fatal("expected Save to be called")
	}
	if result.Order.Status != domain.StatusDraft {
		t.Fatalf("status = %s", result.Order.Status)
	}
	if result.Order.TenantID != testTenantID {
		t.Fatalf("tenant = %s", result.Order.TenantID)
	}
}

func TestAddLineHandler_WhenPlaced_ReturnsORDDOM002(t *testing.T) {
	order := placedOrder(t)
	repo := &fakeOrderRepo{order: order}
	handler := usecases.NewAddLineHandler(repo, fakeClock{})

	_, err := handler.Execute(context.Background(), inboundports.AddLineCommand{
		TenantID:  testTenantID,
		OrderID:   testOrderID,
		VariantID: testVariant,
		Quantity:  1,
	})
	if !domain.HasDomainCode(err, domain.ErrMutationNotInDraft.Code) {
		t.Fatalf("error = %v", err)
	}
}

func TestUpdateLineQuantityHandler_LineNotFound(t *testing.T) {
	order := draftOrder(t)
	repo := &fakeOrderRepo{order: order}
	handler := usecases.NewUpdateLineQuantityHandler(repo, fakeClock{})

	_, err := handler.Execute(context.Background(), inboundports.UpdateLineQuantityCommand{
		TenantID: testTenantID,
		OrderID:  testOrderID,
		LineID:   uuid.New(),
		Quantity: 2,
	})
	if !application.HasAppCode(err, application.ErrLineNotFound.Code) {
		t.Fatalf("error = %v", err)
	}
}

func TestSetCommentsHandler_UpdatesDraft(t *testing.T) {
	order := draftOrder(t)
	repo := &fakeOrderRepo{order: order}
	handler := usecases.NewSetCommentsHandler(repo, fakeClock{})

	result, err := handler.Execute(context.Background(), inboundports.SetCommentsCommand{
		TenantID: testTenantID,
		OrderID:  testOrderID,
		Comments: "sin cebolla",
	})
	if err != nil {
		t.Fatalf("Execute() error = %v", err)
	}
	if result.Order.Comments != "sin cebolla" {
		t.Fatalf("comments = %q", result.Order.Comments)
	}
	if !repo.saveCalled {
		t.Fatal("expected Save to be called")
	}
}

func draftOrder(t *testing.T) *domain.Order {
	t.Helper()

	order, err := domain.NewOrder(testOrderID, testTenantID, domain.SourceWeb, domain.FulfillmentTakeaway, testNow)
	if err != nil {
		t.Fatalf("NewOrder() error = %v", err)
	}
	return order
}

func placedOrder(t *testing.T) *domain.Order {
	t.Helper()

	order := draftOrder(t)
	if err := order.AddLine(testLineID, testVariant, 1, testNow); err != nil {
		t.Fatalf("AddLine() error = %v", err)
	}
	if err := order.SetCustomer(domain.CustomerSnapshot{Name: "Ana"}, testNow); err != nil {
		t.Fatalf("SetCustomer() error = %v", err)
	}

	snap := domain.VariantSnapshot{
		VariantID:     testVariant,
		CatalogItemID: uuid.New(),
		Name:          "Latte",
		UnitPrice:     mustMoney(t, 50000),
		IsActive:      true,
	}
	if err := order.Place(domain.PlaceInput{
		OrderNumber: 1,
		Variants:    []domain.VariantSnapshot{snap},
		Actor:       domain.Actor{Type: domain.ActorStaff},
		OccurredAt:  testNow,
	}); err != nil {
		t.Fatalf("Place() error = %v", err)
	}
	return order
}

func mustMoney(t *testing.T, amount int64) domain.Money {
	t.Helper()
	m, err := domain.NewMoney(amount, domain.DefaultCurrency)
	if err != nil {
		t.Fatalf("NewMoney() error = %v", err)
	}
	return m
}
