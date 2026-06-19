package catalog

import (
	"context"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"

	"orders-service/internal/application"
	"orders-service/internal/domain"
)

func TestValidateAndResolve_CatalogDown_ReturnsORDAPP004(t *testing.T) {
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
		http.Error(w, "down", http.StatusServiceUnavailable)
	}))
	defer srv.Close()

	client := NewClient(srv.URL)
	tenantID := uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")
	variantID := uuid.MustParse("dddddddd-0000-0000-0000-000000000001")

	_, err := client.ValidateAndResolve(context.Background(), tenantID, []uuid.UUID{variantID})
	if !application.HasAppCode(err, "ORD-APP-004") {
		t.Fatalf("expected ORD-APP-004, got %v", err)
	}
}

func TestValidateAndResolve_MapsProjectionToSnapshots(t *testing.T) {
	variantID := uuid.MustParse("dddddddd-0000-0000-0000-000000000001")
	itemID := uuid.MustParse("11111111-0000-0000-0000-000000000001")
	tenantID := uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")

	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if r.Header.Get("X-Tenant-Id") != tenantID.String() {
			t.Fatalf("unexpected tenant header: %s", r.Header.Get("X-Tenant-Id"))
		}
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{
			"categories": [{
				"items": [{
					"variantId": "` + variantID.String() + `",
					"itemId": "` + itemID.String() + `",
					"variantName": "Burger",
					"status": "active",
					"price": { "amount": 12.5, "currency": "ARS" }
				}]
			}]
		}`))
	}))
	defer srv.Close()

	client := NewClient(srv.URL)
	snapshots, err := client.ValidateAndResolve(context.Background(), tenantID, []uuid.UUID{variantID})
	if err != nil {
		t.Fatal(err)
	}
	if len(snapshots) != 1 {
		t.Fatalf("expected 1 snapshot, got %d", len(snapshots))
	}
	if snapshots[0].VariantID != variantID {
		t.Fatalf("variant id = %v", snapshots[0].VariantID)
	}
	if snapshots[0].CatalogItemID != itemID {
		t.Fatalf("item id = %v", snapshots[0].CatalogItemID)
	}
	if snapshots[0].UnitPrice.Amount != 1250 {
		t.Fatalf("unit price amount = %d", snapshots[0].UnitPrice.Amount)
	}
}

func TestValidateAndResolve_MissingVariant_ReturnsORDDOM008(t *testing.T) {
	tenantID := uuid.MustParse("aaaaaaaa-0000-0000-0000-000000000001")
	variantID := uuid.MustParse("dddddddd-0000-0000-0000-000000000001")

	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{"categories": []}`))
	}))
	defer srv.Close()

	client := NewClient(srv.URL)
	_, err := client.ValidateAndResolve(context.Background(), tenantID, []uuid.UUID{variantID})
	if !domain.HasDomainCode(err, "ORD-DOM-008") {
		t.Fatalf("expected ORD-DOM-008, got %v", err)
	}
}
