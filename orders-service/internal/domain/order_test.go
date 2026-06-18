package domain

import (
	"strings"
	"testing"
	"time"

	"github.com/google/uuid"
)

var (
	testNow       = time.Date(2026, 6, 18, 12, 0, 0, 0, time.UTC)
	testTenantID  = uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")
	testOrderID   = uuid.MustParse("bbbbbbbb-0000-0000-0000-000000000001")
	testLineID    = uuid.MustParse("cccccccc-0000-0000-0000-000000000001")
	testVariantID = uuid.MustParse("dddddddd-0000-0000-0000-000000000001")
	testVariant2  = uuid.MustParse("eeeeeeee-0000-0000-0000-000000000001")
)

func mustMoney(t *testing.T, amount int64) Money {
	t.Helper()
	m, err := NewMoney(amount, DefaultCurrency)
	if err != nil {
		t.Fatal(err)
	}
	return m
}

func newDraftOrder(t *testing.T) *Order {
	t.Helper()
	order, err := NewOrder(testOrderID, testTenantID, SourcePOS, FulfillmentTakeaway, testNow)
	if err != nil {
		t.Fatal(err)
	}
	return order
}

func validSnapshot(variantID uuid.UUID, amount int64) VariantSnapshot {
	price, _ := NewMoney(amount, DefaultCurrency)
	return VariantSnapshot{
		VariantID:     variantID,
		CatalogItemID: uuid.New(),
		Name:          "Burger",
		UnitPrice:     price,
		IsActive:      true,
	}
}

func assertDomainCode(t *testing.T, err error, code string) {
	t.Helper()
	if err == nil {
		t.Fatalf("expected error %s, got nil", code)
	}
	if !HasDomainCode(err, code) {
		t.Fatalf("expected code %s, got %v", code, err)
	}
}

func draftWithLineAndCustomer(t *testing.T) *Order {
	t.Helper()
	order := newDraftOrder(t)
	if err := order.AddLine(testLineID, testVariantID, 2, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(CustomerSnapshot{Name: "Ana"}, testNow); err != nil {
		t.Fatal(err)
	}
	return order
}

func mustPlace(t *testing.T, order *Order) {
	t.Helper()
	err := order.Place(PlaceInput{
		OrderNumber: 42,
		Variants:    []VariantSnapshot{validSnapshot(testVariantID, 1500)},
		Actor:       Actor{Type: ActorStaff},
		OccurredAt:  testNow,
	})
	if err != nil {
		t.Fatal(err)
	}
}

// TC-001: Create Draft order.
func TestNewOrder_CreatesDraft(t *testing.T) {
	order := newDraftOrder(t)

	if order.Status != StatusDraft {
		t.Fatalf("status = %s", order.Status)
	}
	if order.OrderNumber != nil {
		t.Fatal("order number should be nil in draft")
	}
	if len(order.lines) != 0 {
		t.Fatal("expected no lines")
	}
}

func TestNewOrder_RequiresTenant(t *testing.T) {
	_, err := NewOrder(testOrderID, uuid.Nil, SourcePOS, FulfillmentTakeaway, testNow)
	if err == nil {
		t.Fatal("expected error for empty tenant")
	}
}

// TC-002: AddLine + Place freezes prices.
func TestPlace_FreezesLineSnapshot(t *testing.T) {
	order := draftWithLineAndCustomer(t)

	err := order.Place(PlaceInput{
		OrderNumber: 42,
		Variants:    []VariantSnapshot{validSnapshot(testVariantID, 1500)},
		Actor:       Actor{Type: ActorStaff},
		OccurredAt:  testNow,
	})
	if err != nil {
		t.Fatal(err)
	}

	if order.Status != StatusPlaced {
		t.Fatalf("status = %s", order.Status)
	}
	if order.OrderNumber == nil || *order.OrderNumber != 42 {
		t.Fatal("expected order number 42")
	}

	line := order.lines[0]
	if line.Name == nil || *line.Name != "Burger" {
		t.Fatal("expected frozen name")
	}
	if line.UnitPrice == nil || line.UnitPrice.Amount != 1500 {
		t.Fatalf("unit price = %v", line.UnitPrice)
	}
	if line.LineTotal == nil || line.LineTotal.Amount != 3000 {
		t.Fatalf("line total = %v", line.LineTotal)
	}
	if order.Totals == nil || order.Totals.Subtotal.Amount != 3000 {
		t.Fatalf("subtotal = %v", order.Totals)
	}
}

// TC-003: Place fails without customer.name.
func TestPlace_WithoutCustomerName_ReturnsORDDOM005(t *testing.T) {
	order := newDraftOrder(t)
	if err := order.AddLine(testLineID, testVariantID, 1, testNow); err != nil {
		t.Fatal(err)
	}

	err := order.Place(PlaceInput{
		OrderNumber: 1,
		Variants:    []VariantSnapshot{validSnapshot(testVariantID, 1000)},
		Actor:       Actor{Type: ActorBuyer},
		OccurredAt:  testNow,
	})
	assertDomainCode(t, err, "ORD-DOM-005")
}

func TestPlace_WithWhitespaceCustomerName_ReturnsORDDOM005(t *testing.T) {
	order := newDraftOrder(t)
	if err := order.AddLine(testLineID, testVariantID, 1, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(CustomerSnapshot{Name: "   "}, testNow); err != nil {
		t.Fatal(err)
	}

	err := order.Place(PlaceInput{
		OrderNumber: 1,
		Variants:    []VariantSnapshot{validSnapshot(testVariantID, 1000)},
		Actor:       Actor{Type: ActorBuyer},
		OccurredAt:  testNow,
	})
	assertDomainCode(t, err, "ORD-DOM-005")
}

// TC-004: Placed → Accept → Start → Complete.
func TestLifecycle_HappyPath(t *testing.T) {
	order := draftWithLineAndCustomer(t)
	staff := Actor{Type: ActorStaff, ID: "s1"}
	mustPlace(t, order)

	steps := []struct {
		name   string
		action func(Actor, time.Time) error
		want   OrderStatus
	}{
		{"Accept", order.Accept, StatusAccepted},
		{"Start", order.Start, StatusInProgress},
		{"Complete", order.Complete, StatusCompleted},
	}

	for _, step := range steps {
		t.Run(step.name, func(t *testing.T) {
			if err := step.action(staff, testNow); err != nil {
				t.Fatal(err)
			}
			if order.Status != step.want {
				t.Fatalf("status = %s, want %s", order.Status, step.want)
			}
		})
	}

	if len(order.transitions) != 4 {
		t.Fatalf("transitions = %d, want 4", len(order.transitions))
	}
}

// TC-005: Placed → Cancelled with reason.
func TestCancel_FromPlaced_RequiresReason(t *testing.T) {
	order := draftWithLineAndCustomer(t)
	mustPlace(t, order)

	err := order.Cancel(Actor{Type: ActorBuyer}, testNow)
	assertDomainCode(t, err, "ORD-DOM-007")

	err = order.Cancel(Actor{Type: ActorBuyer, Reason: "changed mind"}, testNow)
	if err != nil {
		t.Fatal(err)
	}
	if order.Status != StatusCancelled {
		t.Fatalf("status = %s", order.Status)
	}
}

// TC-006: AddLine when Placed → ORD-DOM-002.
func TestAddLine_WhenPlaced_ReturnsORDDOM002(t *testing.T) {
	order := draftWithLineAndCustomer(t)
	mustPlace(t, order)

	err := order.AddLine(uuid.New(), uuid.New(), 1, testNow)
	assertDomainCode(t, err, "ORD-DOM-002")
}

func TestPlace_WithoutLines_ReturnsORDDOM004(t *testing.T) {
	order := newDraftOrder(t)
	if err := order.SetCustomer(CustomerSnapshot{Name: "Ana"}, testNow); err != nil {
		t.Fatal(err)
	}

	err := order.Place(PlaceInput{
		OrderNumber: 1,
		Variants:    []VariantSnapshot{validSnapshot(testVariantID, 1000)},
		Actor:       Actor{Type: ActorStaff},
		OccurredAt:  testNow,
	})
	assertDomainCode(t, err, "ORD-DOM-004")
}

func TestPlace_DeliveryWithoutAddress_ReturnsORDDOM006(t *testing.T) {
	order := newDraftOrder(t)
	if err := order.SetFulfillmentType(FulfillmentDelivery, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(testLineID, testVariantID, 1, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(CustomerSnapshot{Name: "Ana"}, testNow); err != nil {
		t.Fatal(err)
	}

	err := order.Place(PlaceInput{
		OrderNumber: 1,
		Variants:    []VariantSnapshot{validSnapshot(testVariantID, 1000)},
		Actor:       Actor{Type: ActorBuyer},
		OccurredAt:  testNow,
	})
	assertDomainCode(t, err, "ORD-DOM-006")
}

func TestPlace_DeliveryWithAddress_Succeeds(t *testing.T) {
	order := newDraftOrder(t)
	if err := order.SetFulfillmentType(FulfillmentDelivery, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetAddress(Address{Street: "Main", Number: "123", City: "BA"}, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(testLineID, testVariantID, 1, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(CustomerSnapshot{Name: "Ana"}, testNow); err != nil {
		t.Fatal(err)
	}

	if err := order.Place(PlaceInput{
		OrderNumber: 7,
		Variants:    []VariantSnapshot{validSnapshot(testVariantID, 1000)},
		Actor:       Actor{Type: ActorBuyer},
		OccurredAt:  testNow,
	}); err != nil {
		t.Fatal(err)
	}
}

func TestPlace_InactiveVariant_ReturnsORDDOM008(t *testing.T) {
	order := draftWithLineAndCustomer(t)
	snap := validSnapshot(testVariantID, 1000)
	snap.IsActive = false

	err := order.Place(PlaceInput{
		OrderNumber: 1,
		Variants:    []VariantSnapshot{snap},
		Actor:       Actor{Type: ActorStaff},
		OccurredAt:  testNow,
	})
	assertDomainCode(t, err, "ORD-DOM-008")
}

func TestAddLine_InvalidQuantity_ReturnsORDDOM003(t *testing.T) {
	order := newDraftOrder(t)
	err := order.AddLine(testLineID, testVariantID, 0, testNow)
	assertDomainCode(t, err, "ORD-DOM-003")
}

func TestAddLine_DuplicateVariantAllowed(t *testing.T) {
	order := newDraftOrder(t)
	line2 := uuid.MustParse("11111111-0000-0000-0000-000000000001")

	if err := order.AddLine(testLineID, testVariantID, 1, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(line2, testVariantID, 2, testNow); err != nil {
		t.Fatal(err)
	}
	if len(order.lines) != 2 {
		t.Fatalf("lines = %d", len(order.lines))
	}
}

func TestCancel_FromDraft_WithoutReason(t *testing.T) {
	order := newDraftOrder(t)
	if err := order.Cancel(Actor{Type: ActorBuyer}, testNow); err != nil {
		t.Fatal(err)
	}
	if order.Status != StatusCancelled {
		t.Fatalf("status = %s", order.Status)
	}
}

func TestDraftMutations_WhenPlaced_ReturnORDDOM002(t *testing.T) {
	order := draftWithLineAndCustomer(t)
	mustPlace(t, order)

	tests := []struct {
		name string
		run  func() error
	}{
		{
			name: "UpdateLineQuantity",
			run: func() error {
				return order.UpdateLineQuantity(testLineID, 3, testNow)
			},
		},
		{
			name: "RemoveLine",
			run: func() error {
				return order.RemoveLine(testLineID, testNow)
			},
		},
		{
			name: "SetCustomer",
			run: func() error {
				return order.SetCustomer(CustomerSnapshot{Name: "Other"}, testNow)
			},
		},
		{
			name: "SetAddress",
			run: func() error {
				return order.SetAddress(Address{Street: "A", Number: "1", City: "C"}, testNow)
			},
		},
		{
			name: "SetFulfillmentType",
			run: func() error {
				return order.SetFulfillmentType(FulfillmentDelivery, testNow)
			},
		},
		{
			name: "SetComments",
			run: func() error {
				return order.SetComments("note", testNow)
			},
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			assertDomainCode(t, tt.run(), "ORD-DOM-002")
		})
	}
}

func TestUpdateLineQuantity_NotFound(t *testing.T) {
	order := newDraftOrder(t)
	err := order.UpdateLineQuantity(uuid.New(), 2, testNow)
	if err == nil || !strings.Contains(err.Error(), "not found") {
		t.Fatalf("expected line not found error, got %v", err)
	}
}

func TestRemoveLine_NotFound(t *testing.T) {
	order := newDraftOrder(t)
	err := order.RemoveLine(uuid.New(), testNow)
	if err == nil || !strings.Contains(err.Error(), "not found") {
		t.Fatalf("expected line not found error, got %v", err)
	}
}

func TestSetComments_ExceedsMaxLength(t *testing.T) {
	order := newDraftOrder(t)
	long := strings.Repeat("a", MaxCommentsLength+1)
	if err := order.SetComments(long, testNow); err == nil {
		t.Fatal("expected error")
	}
}

func TestInvalidTransitions(t *testing.T) {
	tests := []struct {
		name       string
		setup      func(t *testing.T) *Order
		transition func(*Order) error
	}{
		{
			name:  "draft to complete",
			setup: func(t *testing.T) *Order { return newDraftOrder(t) },
			transition: func(o *Order) error {
				return o.Complete(Actor{Type: ActorStaff}, testNow)
			},
		},
		{
			name: "placed to start",
			setup: func(t *testing.T) *Order {
				o := draftWithLineAndCustomer(t)
				mustPlace(t, o)
				return o
			},
			transition: func(o *Order) error {
				return o.Start(Actor{Type: ActorStaff}, testNow)
			},
		},
		{
			name: "completed to cancel",
			setup: func(t *testing.T) *Order {
				o := draftWithLineAndCustomer(t)
				mustPlace(t, o)
				_ = o.Accept(Actor{Type: ActorStaff}, testNow)
				_ = o.Start(Actor{Type: ActorStaff}, testNow)
				_ = o.Complete(Actor{Type: ActorStaff}, testNow)
				return o
			},
			transition: func(o *Order) error {
				return o.Cancel(Actor{Type: ActorStaff, Reason: "too late"}, testNow)
			},
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			order := tt.setup(t)
			assertDomainCode(t, tt.transition(order), "ORD-DOM-001")
		})
	}
}

func TestPlace_TwoLines_SumsTotals(t *testing.T) {
	order := newDraftOrder(t)
	line2ID := uuid.MustParse("11111111-0000-0000-0000-000000000001")

	if err := order.AddLine(testLineID, testVariantID, 1, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(line2ID, testVariant2, 2, testNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(CustomerSnapshot{Name: "Ana"}, testNow); err != nil {
		t.Fatal(err)
	}

	err := order.Place(PlaceInput{
		OrderNumber: 10,
		Variants: []VariantSnapshot{
			validSnapshot(testVariantID, 1000),
			validSnapshot(testVariant2, 500),
		},
		Actor:      Actor{Type: ActorStaff},
		OccurredAt: testNow,
	})
	if err != nil {
		t.Fatal(err)
	}

	if order.Totals.Subtotal.Amount != 2000 {
		t.Fatalf("subtotal = %d, want 2000", order.Totals.Subtotal.Amount)
	}
}

func TestHasDomainCode(t *testing.T) {
	if !HasDomainCode(ErrInvalidQuantity, "ORD-DOM-003") {
		t.Fatal("expected HasDomainCode to match")
	}
	if HasDomainCode(ErrInvalidQuantity, "ORD-DOM-001") {
		t.Fatal("expected HasDomainCode to reject wrong code")
	}
}
