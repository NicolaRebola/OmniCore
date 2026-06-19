package http_test

import (
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/go-chi/chi/v5"

	httpadapter "orders-service/adapters/http"
)

func TestSwaggerDisabled(t *testing.T) {
	r := chi.NewRouter()
	httpadapter.MountSwaggerIfEnabled(r, httpadapter.SwaggerConfig{Enabled: false})

	for _, path := range []string{"/swagger", "/swagger/v1/swagger.json", "/swagger/v1/openapi.yaml"} {
		rec := httptest.NewRecorder()
		r.ServeHTTP(rec, httptest.NewRequest(http.MethodGet, path, nil))
		if rec.Code != http.StatusNotFound {
			t.Fatalf("GET %s: status = %d, want 404", path, rec.Code)
		}
	}
}

func TestSwaggerEnabled(t *testing.T) {
	r := chi.NewRouter()
	httpadapter.MountSwaggerIfEnabled(r, httpadapter.SwaggerConfig{Enabled: true})

	t.Run("UI", func(t *testing.T) {
		rec := httptest.NewRecorder()
		r.ServeHTTP(rec, httptest.NewRequest(http.MethodGet, "/swagger", nil))
		if rec.Code != http.StatusOK {
			t.Fatalf("status = %d", rec.Code)
		}
		if ct := rec.Header().Get("Content-Type"); ct != "text/html; charset=utf-8" {
			t.Fatalf("content-type = %q", ct)
		}
	})

	t.Run("openapi yaml", func(t *testing.T) {
		rec := httptest.NewRecorder()
		r.ServeHTTP(rec, httptest.NewRequest(http.MethodGet, "/swagger/v1/openapi.yaml", nil))
		if rec.Code != http.StatusOK {
			t.Fatalf("status = %d", rec.Code)
		}
		if ct := rec.Header().Get("Content-Type"); ct != "application/yaml" {
			t.Fatalf("content-type = %q", ct)
		}
		if len(rec.Body.Bytes()) == 0 {
			t.Fatal("empty openapi spec")
		}
	})

	t.Run("swagger json", func(t *testing.T) {
		rec := httptest.NewRecorder()
		r.ServeHTTP(rec, httptest.NewRequest(http.MethodGet, "/swagger/v1/swagger.json", nil))
		if rec.Code != http.StatusOK {
			t.Fatalf("status = %d", rec.Code)
		}
		if ct := rec.Header().Get("Content-Type"); ct != "application/json" {
			t.Fatalf("content-type = %q", ct)
		}
	})
}
