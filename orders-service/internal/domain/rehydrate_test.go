package domain

import (
	"testing"
	"time"

	"github.com/google/uuid"
)

func TestRehydrateOrder_RestoresAggregate(t *testing.T) {
	orderNumber := int64(42)
	catalogItemID := uuid.MustParse("11111111-0000-0000-0000-000000000001")
	name := "Burger"
	unitPrice := mustMoney(t, 1500)
	lineTotal := mustMoney(t, 3000)
	customer := &CustomerSnapshot{Name: "Ana", Email: "ana@example.com", Phone: "+5411"}
	address := &Address{Street: "Corrientes", Number: "1234", City: "CABA"}
	subtotal := mustMoney(t, 3000)
	totals := &Totals{Subtotal: subtotal, Total: subtotal}
	lines := []OrderLine{{
		ID:            testLineID,
		VariantID:     testVariantID,
		CatalogItemID: &catalogItemID,
		Name:          &name,
		UnitPrice:     &unitPrice,
		Quantity:      2,
		LineTotal:     &lineTotal,
	}}
	transitions := []OrderTransition{{
		FromStatus: StatusDraft,
		ToStatus:   StatusPlaced,
		OccurredAt: testNow,
		ActorType:  ActorStaff,
		ActorID:    "staff-1",
		Reason:     "",
	}}
	updatedAt := testNow.Add(time.Hour)

	order := RehydrateOrder(
		testOrderID,
		testTenantID,
		&orderNumber,
		StatusPlaced,
		SourcePOS,
		FulfillmentDelivery,
		customer,
		address,
		"sin cebolla",
		totals,
		lines,
		transitions,
		testNow,
		updatedAt,
	)

	if order.ID != testOrderID {
		t.Fatalf("id = %s", order.ID)
	}
	if order.TenantID != testTenantID {
		t.Fatalf("tenant id = %s", order.TenantID)
	}
	if order.OrderNumber == nil || *order.OrderNumber != 42 {
		t.Fatalf("order number = %v", order.OrderNumber)
	}
	if order.Status != StatusPlaced {
		t.Fatalf("status = %s", order.Status)
	}
	if order.Source != SourcePOS {
		t.Fatalf("source = %s", order.Source)
	}
	if order.FulfillmentType != FulfillmentDelivery {
		t.Fatalf("fulfillment = %s", order.FulfillmentType)
	}
	if order.Customer == nil || order.Customer.Name != "Ana" {
		t.Fatal("customer not restored")
	}
	if order.Address == nil || order.Address.City != "CABA" {
		t.Fatal("address not restored")
	}
	if order.Comments != "sin cebolla" {
		t.Fatalf("comments = %q", order.Comments)
	}
	if order.Totals == nil || order.Totals.Total.Amount != 3000 {
		t.Fatal("totals not restored")
	}
	if order.CreatedAt != testNow {
		t.Fatalf("created at = %v", order.CreatedAt)
	}
	if order.UpdatedAt != updatedAt {
		t.Fatalf("updated at = %v", order.UpdatedAt)
	}

	gotLines := order.Lines()
	if len(gotLines) != 1 {
		t.Fatalf("lines len = %d", len(gotLines))
	}
	if gotLines[0].ID != testLineID || gotLines[0].Quantity != 2 {
		t.Fatalf("line = %+v", gotLines[0])
	}

	gotTransitions := order.Transitions()
	if len(gotTransitions) != 1 {
		t.Fatalf("transitions len = %d", len(gotTransitions))
	}
	if gotTransitions[0].ToStatus != StatusPlaced || gotTransitions[0].ActorType != ActorStaff {
		t.Fatalf("transition = %+v", gotTransitions[0])
	}
}

func TestRehydrateOrder_AllowsNilOptionalFields(t *testing.T) {
	order := RehydrateOrder(
		testOrderID,
		testTenantID,
		nil,
		StatusDraft,
		SourceWeb,
		FulfillmentTakeaway,
		nil,
		nil,
		"",
		nil,
		nil,
		nil,
		testNow,
		testNow,
	)

	if order.OrderNumber != nil {
		t.Fatal("expected nil order number")
	}
	if order.Customer != nil || order.Address != nil || order.Totals != nil {
		t.Fatal("expected nil optional fields")
	}
	if len(order.Lines()) != 0 || len(order.Transitions()) != 0 {
		t.Fatal("expected empty children")
	}
}

func TestOrder_LinesReturnsCopy(t *testing.T) {
	order := RehydrateOrder(
		testOrderID,
		testTenantID,
		nil,
		StatusDraft,
		SourcePOS,
		FulfillmentTakeaway,
		nil,
		nil,
		"",
		nil,
		[]OrderLine{{ID: testLineID, VariantID: testVariantID, Quantity: 1}},
		nil,
		testNow,
		testNow,
	)

	lines := order.Lines()
	lines[0].Quantity = 99

	if order.Lines()[0].Quantity != 1 {
		t.Fatal("mutating returned lines slice affected internal state")
	}
}

func TestOrder_TransitionsReturnsCopy(t *testing.T) {
	order := RehydrateOrder(
		testOrderID,
		testTenantID,
		nil,
		StatusPlaced,
		SourcePOS,
		FulfillmentTakeaway,
		nil,
		nil,
		"",
		nil,
		nil,
		[]OrderTransition{{
			FromStatus: StatusDraft,
			ToStatus:   StatusPlaced,
			OccurredAt: testNow,
			ActorType:  ActorStaff,
		}},
		testNow,
		testNow,
	)

	transitions := order.Transitions()
	transitions[0].Reason = "mutated"

	if order.Transitions()[0].Reason != "" {
		t.Fatal("mutating returned transitions slice affected internal state")
	}
}

func TestRehydrateOrder_DefensiveCopyOfChildren(t *testing.T) {
	lines := []OrderLine{{ID: testLineID, VariantID: testVariantID, Quantity: 1}}
	transitions := []OrderTransition{{
		FromStatus: StatusDraft,
		ToStatus:   StatusPlaced,
		OccurredAt: testNow,
		ActorType:  ActorStaff,
	}}

	order := RehydrateOrder(
		testOrderID,
		testTenantID,
		nil,
		StatusPlaced,
		SourcePOS,
		FulfillmentTakeaway,
		nil,
		nil,
		"",
		nil,
		lines,
		transitions,
		testNow,
		testNow,
	)

	lines[0].Quantity = 99
	transitions[0].Reason = "mutated"

	if order.Lines()[0].Quantity != 1 {
		t.Fatal("mutating input lines slice affected rehydrated order")
	}
	if order.Transitions()[0].Reason != "" {
		t.Fatal("mutating input transitions slice affected rehydrated order")
	}
}
