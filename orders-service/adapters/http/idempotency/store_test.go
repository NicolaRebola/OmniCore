package idempotency_test

import (
	"context"
	"testing"

	"github.com/google/uuid"

	"orders-service/adapters/http/idempotency"
	"orders-service/adapters/postgres"
	"orders-service/internal/application"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
)

type memoryIdempotencyRepo struct {
	records map[string]postgresports.IdempotencyRecord
}

func newMemoryRepo() *memoryIdempotencyRepo {
	return &memoryIdempotencyRepo{records: make(map[string]postgresports.IdempotencyRecord)}
}

func (r *memoryIdempotencyRepo) key(tenantID uuid.UUID, idempotencyKey, operation string) string {
	return tenantID.String() + "|" + idempotencyKey + "|" + operation
}

func (r *memoryIdempotencyRepo) Find(
	_ context.Context,
	tenantID uuid.UUID,
	key, operation string,
) (*postgresports.IdempotencyRecord, error) {
	record, ok := r.records[r.key(tenantID, key, operation)]
	if !ok {
		return nil, nil
	}
	copy := record
	return &copy, nil
}

func (r *memoryIdempotencyRepo) Save(_ context.Context, record postgresports.IdempotencyRecord) error {
	k := r.key(record.TenantID, record.IdempotencyKey, record.Operation)
	if _, exists := r.records[k]; exists {
		return postgres.ErrIdempotencyKeyExists
	}
	r.records[k] = record
	return nil
}

func TestGetReplay_NotFound(t *testing.T) {
	store := idempotency.NewStore(newMemoryRepo())
	tenantID := uuid.New()

	replay, found, err := store.GetReplay(
		context.Background(), tenantID, "key-1", idempotency.CreateOrderOperation, "hash-a",
	)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if found {
		t.Fatal("expected no replay")
	}
	if replay != nil {
		t.Fatalf("replay = %q", replay)
	}
}

func TestGetReplay_SameHashReturnsStoredResponse(t *testing.T) {
	repo := newMemoryRepo()
	store := idempotency.NewStore(repo)
	tenantID := uuid.New()
	const (
		key      = "key-1"
		op       = idempotency.PlaceOrderOperation
		hash     = "hash-a"
		response = `{"status":"Placed"}`
	)

	if err := store.SaveResponse(context.Background(), tenantID, key, op, hash, []byte(response)); err != nil {
		t.Fatalf("save: %v", err)
	}

	replay, found, err := store.GetReplay(context.Background(), tenantID, key, op, hash)
	if err != nil {
		t.Fatalf("get replay: %v", err)
	}
	if !found {
		t.Fatal("expected replay")
	}
	if string(replay) != response {
		t.Fatalf("replay = %q", replay)
	}
}

func TestGetReplay_DifferentHashReturnsORDAPP006(t *testing.T) {
	repo := newMemoryRepo()
	store := idempotency.NewStore(repo)
	tenantID := uuid.New()

	if err := store.SaveResponse(
		context.Background(), tenantID, "key-1", idempotency.CreateAndPlaceOperation, "hash-a", []byte(`{}`),
	); err != nil {
		t.Fatalf("save: %v", err)
	}

	_, found, err := store.GetReplay(context.Background(), tenantID, "key-1", idempotency.CreateAndPlaceOperation, "hash-b")
	if !application.HasAppCode(err, "ORD-APP-006") {
		t.Fatalf("expected ORD-APP-006, got found=%v err=%v", found, err)
	}
}

func TestSaveResponse_DuplicateSameHashIsNoOp(t *testing.T) {
	repo := newMemoryRepo()
	store := idempotency.NewStore(repo)
	tenantID := uuid.New()
	const response = `{"id":"order-1"}`

	if err := store.SaveResponse(
		context.Background(), tenantID, "key-1", idempotency.CreateOrderOperation, "hash-a", []byte(response),
	); err != nil {
		t.Fatalf("first save: %v", err)
	}
	if err := store.SaveResponse(
		context.Background(), tenantID, "key-1", idempotency.CreateOrderOperation, "hash-a", []byte(response),
	); err != nil {
		t.Fatalf("second save: %v", err)
	}
}

func TestSaveResponse_DuplicateDifferentHashReturnsORDAPP006(t *testing.T) {
	repo := newMemoryRepo()
	store := idempotency.NewStore(repo)
	tenantID := uuid.New()

	if err := store.SaveResponse(
		context.Background(), tenantID, "key-1", idempotency.PlaceOrderOperation, "hash-a", []byte(`{"status":"Placed"}`),
	); err != nil {
		t.Fatalf("first save: %v", err)
	}

	err := store.SaveResponse(
		context.Background(), tenantID, "key-1", idempotency.PlaceOrderOperation, "hash-b", []byte(`{"status":"Placed"}`),
	)
	if !application.HasAppCode(err, "ORD-APP-006") {
		t.Fatalf("expected ORD-APP-006, got %v", err)
	}
}

func TestHashScopedRequest_ScopeChangesHash(t *testing.T) {
	body := []byte(`{"actorType":"staff"}`)
	orderA := uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")
	orderB := uuid.MustParse("bbbbbbbb-0000-0000-0000-000000000001")

	hashA := idempotency.HashScopedRequest(orderA.String(), body)
	hashB := idempotency.HashScopedRequest(orderB.String(), body)
	if hashA == hashB {
		t.Fatal("expected different hashes for different order ids")
	}

	same := idempotency.HashScopedRequest(orderA.String(), body)
	if hashA != same {
		t.Fatal("expected deterministic hash for same scope and body")
	}
}

func TestHashRequestBody_IsDeterministic(t *testing.T) {
	body := []byte(`{"source":"POS","fulfillmentType":"TAKEAWAY"}`)
	a := idempotency.HashRequestBody(body)
	b := idempotency.HashRequestBody(body)
	if a != b || a == "" {
		t.Fatalf("hashes = %q %q", a, b)
	}
}
