package http

import (
	"log/slog"

	"github.com/go-chi/chi/v5"
	chimw "github.com/go-chi/chi/v5/middleware"
	"github.com/jackc/pgx/v5/pgxpool"

	httpmw "orders-service/adapters/http/middleware"
)

func NewRouter(logger *slog.Logger, pool *pgxpool.Pool) chi.Router {
	r := chi.NewRouter()

	r.Use(chimw.RequestID)
	r.Use(chimw.Recoverer)
	r.Use(httpmw.RequestLogger(logger))

	r.Get("/health", HealthHandler)
	r.Get("/ready", ReadyHandler(pool))

	r.Route("/api/v1", func(r chi.Router) {
		r.Use(httpmw.TenantPlaceholder())
	})

	return r
}
