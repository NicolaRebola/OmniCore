//go:build integration

package postgres

import (
	"context"
	"encoding/json"
	"errors"
	"testing"
	"time"

	outboundports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"

	"github.com/google/uuid"
)

var (
	integrationNow = time.Date(2026, 6, 19, 10, 0, 0, 0, time.UTC)
)

func TestOrderRepository_SaveAndGet_DraftRoundTrip(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewOrderRepository(pool)

	tenantID := uuid.New()
	orderID := uuid.New()
	lineID := uuid.New()
	variantID := uuid.New()

	order, err := domain.NewOrder(orderID, tenantID, domain.SourcePOS, domain.FulfillmentTakeaway, integrationNow)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(lineID, variantID, 2, integrationNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(domain.CustomerSnapshot{Name: "Ana", Email: "ana@example.com"}, integrationNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetComments("sin cebolla", integrationNow); err != nil {
		t.Fatal(err)
	}

	ctx := context.Background()
	if err := repo.Save(ctx, order); err != nil {
		t.Fatalf("save: %v", err)
	}

	loaded, err := repo.GetByID(ctx, tenantID, orderID)
	if err != nil {
		t.Fatalf("get: %v", err)
	}
	if loaded == nil {
		t.Fatal("expected order")
	}
	if loaded.Status != domain.StatusDraft {
		t.Fatalf("status = %s", loaded.Status)
	}
	if loaded.OrderNumber != nil {
		t.Fatal("expected nil order number in draft")
	}
	if loaded.Customer == nil || loaded.Customer.Name != "Ana" {
		t.Fatal("customer not restored")
	}
	if loaded.Comments != "sin cebolla" {
		t.Fatalf("comments = %q", loaded.Comments)
	}
	if len(loaded.Lines()) != 1 {
		t.Fatalf("lines = %d", len(loaded.Lines()))
	}
	if loaded.Lines()[0].Quantity != 2 {
		t.Fatalf("quantity = %d", loaded.Lines()[0].Quantity)
	}
}

func TestOrderRepository_SaveAndGet_PlacedOrder(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewOrderRepository(pool)

	tenantID := uuid.New()
	orderID := uuid.New()
	lineID := uuid.New()
	variantID := uuid.New()

	order, err := domain.NewOrder(orderID, tenantID, domain.SourceWeb, domain.FulfillmentTakeaway, integrationNow)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(lineID, variantID, 1, integrationNow); err != nil {
		t.Fatal(err)
	}
	if err := order.SetCustomer(domain.CustomerSnapshot{Name: "Luis"}, integrationNow); err != nil {
		t.Fatal(err)
	}

	price, err := domain.NewMoney(1500, domain.DefaultCurrency)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.Place(domain.PlaceInput{
		OrderNumber: 7,
		Variants: []domain.VariantSnapshot{{
			VariantID:     variantID,
			CatalogItemID: uuid.New(),
			Name:          "Burger",
			UnitPrice:     price,
			IsActive:      true,
		}},
		Actor:      domain.Actor{Type: domain.ActorStaff, ID: "staff-1"},
		OccurredAt: integrationNow,
	}); err != nil {
		t.Fatal(err)
	}

	ctx := context.Background()
	if err := repo.Save(ctx, order); err != nil {
		t.Fatalf("save: %v", err)
	}

	loaded, err := repo.GetByID(ctx, tenantID, orderID)
	if err != nil {
		t.Fatalf("get: %v", err)
	}
	if loaded == nil {
		t.Fatal("expected order")
	}
	if loaded.Status != domain.StatusPlaced {
		t.Fatalf("status = %s", loaded.Status)
	}
	if loaded.OrderNumber == nil || *loaded.OrderNumber != 7 {
		t.Fatalf("order number = %v", loaded.OrderNumber)
	}
	if loaded.Totals == nil || loaded.Totals.Total.Amount != 1500 {
		t.Fatalf("totals = %+v", loaded.Totals)
	}
	if len(loaded.Lines()) != 1 || loaded.Lines()[0].Name == nil {
		t.Fatal("placed line snapshot not restored")
	}
	if len(loaded.Transitions()) != 1 || loaded.Transitions()[0].ToStatus != domain.StatusPlaced {
		t.Fatalf("transitions = %+v", loaded.Transitions())
	}
}

func TestOrderRepository_GetByID_NotFound(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewOrderRepository(pool)

	ctx := context.Background()
	loaded, err := repo.GetByID(ctx, uuid.New(), uuid.New())
	if err != nil {
		t.Fatalf("get: %v", err)
	}
	if loaded != nil {
		t.Fatal("expected nil order")
	}
}

func TestOrderRepository_GetByID_WrongTenantReturnsNotFound(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewOrderRepository(pool)

	tenantID := uuid.New()
	otherTenantID := uuid.New()
	orderID := uuid.New()

	order, err := domain.NewOrder(orderID, tenantID, domain.SourcePOS, domain.FulfillmentTakeaway, integrationNow)
	if err != nil {
		t.Fatal(err)
	}

	ctx := context.Background()
	if err := repo.Save(ctx, order); err != nil {
		t.Fatalf("save: %v", err)
	}

	loaded, err := repo.GetByID(ctx, otherTenantID, orderID)
	if err != nil {
		t.Fatalf("get: %v", err)
	}
	if loaded != nil {
		t.Fatal("expected nil for cross-tenant access")
	}
}

func TestOrderRepository_Save_UpdatesDraftLines(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewOrderRepository(pool)

	tenantID := uuid.New()
	orderID := uuid.New()
	lineID := uuid.New()
	variantID := uuid.New()

	order, err := domain.NewOrder(orderID, tenantID, domain.SourcePOS, domain.FulfillmentTakeaway, integrationNow)
	if err != nil {
		t.Fatal(err)
	}
	if err := order.AddLine(lineID, variantID, 1, integrationNow); err != nil {
		t.Fatal(err)
	}

	ctx := context.Background()
	if err := repo.Save(ctx, order); err != nil {
		t.Fatalf("save: %v", err)
	}

	if err := order.UpdateLineQuantity(lineID, 3, integrationNow.Add(time.Minute)); err != nil {
		t.Fatal(err)
	}
	if err := repo.Save(ctx, order); err != nil {
		t.Fatalf("save update: %v", err)
	}

	loaded, err := repo.GetByID(ctx, tenantID, orderID)
	if err != nil {
		t.Fatalf("get: %v", err)
	}
	if loaded.Lines()[0].Quantity != 3 {
		t.Fatalf("quantity = %d", loaded.Lines()[0].Quantity)
	}
}

func TestIdempotencyRepository_SaveAndFind(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewIdempotencyRepository(pool)

	tenantID := uuid.New()
	record := outboundports.IdempotencyRecord{
		TenantID:       tenantID,
		IdempotencyKey: "key-1",
		Operation:      "Create",
		RequestHash:    "hash-abc",
		ResponseBody:   []byte(`{"id":"order-1"}`),
	}

	ctx := context.Background()
	if err := repo.Save(ctx, record); err != nil {
		t.Fatalf("save: %v", err)
	}

	found, err := repo.Find(ctx, tenantID, "key-1", "Create")
	if err != nil {
		t.Fatalf("find: %v", err)
	}
	if found == nil {
		t.Fatal("expected record")
	}
	if found.RequestHash != "hash-abc" {
		t.Fatalf("hash = %q", found.RequestHash)
	}
	assertJSONEqual(t, record.ResponseBody, found.ResponseBody)
}

func assertJSONEqual(t *testing.T, want, got []byte) {
	t.Helper()

	var wantVal, gotVal any
	if err := json.Unmarshal(want, &wantVal); err != nil {
		t.Fatalf("unmarshal want: %v", err)
	}
	if err := json.Unmarshal(got, &gotVal); err != nil {
		t.Fatalf("unmarshal got: %v", err)
	}

	wantNorm, err := json.Marshal(wantVal)
	if err != nil {
		t.Fatalf("marshal want: %v", err)
	}
	gotNorm, err := json.Marshal(gotVal)
	if err != nil {
		t.Fatalf("marshal got: %v", err)
	}
	if string(wantNorm) != string(gotNorm) {
		t.Fatalf("body = %s, want %s", gotNorm, wantNorm)
	}
}

func TestIdempotencyRepository_Find_NotFound(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewIdempotencyRepository(pool)

	ctx := context.Background()
	found, err := repo.Find(ctx, uuid.New(), "missing", "Create")
	if err != nil {
		t.Fatalf("find: %v", err)
	}
	if found != nil {
		t.Fatal("expected nil")
	}
}

func TestIdempotencyRepository_Save_DuplicateReturnsError(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewIdempotencyRepository(pool)

	tenantID := uuid.New()
	record := outboundports.IdempotencyRecord{
		TenantID:       tenantID,
		IdempotencyKey: "dup-key",
		Operation:      "Place",
		RequestHash:    "hash-1",
		ResponseBody:   []byte(`{"ok":true}`),
	}

	ctx := context.Background()
	if err := repo.Save(ctx, record); err != nil {
		t.Fatalf("first save: %v", err)
	}

	err := repo.Save(ctx, record)
	if !errors.Is(err, ErrIdempotencyKeyExists) {
		t.Fatalf("expected ErrIdempotencyKeyExists, got %v", err)
	}
}

func TestTenantSequenceRepository_NextOrderNumber_Sequential(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewTenantSequenceRepository(pool)

	tenantID := uuid.New()
	ctx := context.Background()

	first, err := repo.NextOrderNumber(ctx, tenantID)
	if err != nil {
		t.Fatalf("first: %v", err)
	}
	second, err := repo.NextOrderNumber(ctx, tenantID)
	if err != nil {
		t.Fatalf("second: %v", err)
	}

	if first != 1 || second != 2 {
		t.Fatalf("sequence = %d, %d", first, second)
	}
}

func TestTenantSequenceRepository_NextOrderNumber_IsolatedPerTenant(t *testing.T) {
	pool := requireIntegrationDB(t)
	repo := NewTenantSequenceRepository(pool)

	ctx := context.Background()
	tenantA := uuid.New()
	tenantB := uuid.New()

	numA, err := repo.NextOrderNumber(ctx, tenantA)
	if err != nil {
		t.Fatalf("tenant A: %v", err)
	}
	numB, err := repo.NextOrderNumber(ctx, tenantB)
	if err != nil {
		t.Fatalf("tenant B: %v", err)
	}

	if numA != 1 || numB != 1 {
		t.Fatalf("expected independent sequences, got %d and %d", numA, numB)
	}
}
