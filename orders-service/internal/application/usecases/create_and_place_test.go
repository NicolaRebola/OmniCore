package usecases_test

import (
	"context"
	"testing"

	inboundports "orders-service/internal/application/ports/inbound"
	"orders-service/internal/application/usecases"
	"orders-service/internal/domain"
)

func TestCreateAndPlace_HappyPath(t *testing.T) {
	repo := &fakeOrderRepo{}
	seq := &fakeSequenceRepo{}
	catalog := &fakeCatalog{snapshots: []domain.VariantSnapshot{validSnapshot()}}
	events := &fakeEvents{}

	handler := usecases.NewCreateAndPlaceHandler(
		repo, seq, catalog, events, fakeTx{}, fakeClock{},
	)

	result, err := handler.Execute(context.Background(), inboundports.CreateAndPlaceCommand{
		TenantID:        testTenantID,
		Source:          domain.SourcePOS,
		FulfillmentType: domain.FulfillmentTakeaway,
		Customer:        domain.CustomerSnapshot{Name: "Walk-in"},
		Lines: []inboundports.CreateAndPlaceLine{
			{VariantID: testVariant, Quantity: 2},
		},
		Actor: domain.Actor{Type: domain.ActorStaff, ID: "user-1"},
	})
	if err != nil {
		t.Fatalf("execute: %v", err)
	}

	if result.Order.Status != domain.StatusPlaced {
		t.Fatalf("status = %s", result.Order.Status)
	}
	if result.Order.OrderNumber == nil || *result.Order.OrderNumber != 1 {
		t.Fatalf("orderNumber = %v", result.Order.OrderNumber)
	}
	if !repo.saveCalled {
		t.Fatal("expected order save")
	}
	if !events.published {
		t.Fatal("expected event publish")
	}
}

func TestCreateAndPlace_CatalogDown_DoesNotSave(t *testing.T) {
	repo := &fakeOrderRepo{}
	handler := usecases.NewCreateAndPlaceHandler(
		repo, &fakeSequenceRepo{},
		&fakeCatalog{err: domain.ErrInvalidVariant},
		&fakeEvents{}, fakeTx{}, fakeClock{},
	)

	_, err := handler.Execute(context.Background(), inboundports.CreateAndPlaceCommand{
		TenantID:        testTenantID,
		Source:          domain.SourcePOS,
		FulfillmentType: domain.FulfillmentTakeaway,
		Customer:        domain.CustomerSnapshot{Name: "Walk-in"},
		Lines: []inboundports.CreateAndPlaceLine{
			{VariantID: testVariant, Quantity: 1},
		},
		Actor: domain.Actor{Type: domain.ActorStaff},
	})
	if err == nil {
		t.Fatal("expected error")
	}
	if repo.saveCalled {
		t.Fatal("order must not be saved when catalog fails")
	}
}
