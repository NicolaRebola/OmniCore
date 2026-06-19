package http

import (
	"log/slog"

	"github.com/go-chi/chi/v5"
	chimw "github.com/go-chi/chi/v5/middleware"
	"github.com/jackc/pgx/v5/pgxpool"

	"orders-service/adapters/http/handlers"
	httpmw "orders-service/adapters/http/middleware"
)

func NewRouter(logger *slog.Logger, pool *pgxpool.Pool, orderHandlers *handlers.OrderHandlers) chi.Router {
	r := chi.NewRouter()

	r.Use(chimw.RequestID)
	r.Use(chimw.Recoverer)
	r.Use(httpmw.RequestLogger(logger))

	r.Get("/health", HealthHandler)
	r.Get("/ready", ReadyHandler(pool))

	r.Route("/api/v1", func(r chi.Router) {
		r.Use(httpmw.TenantRequired())

		r.Route("/orders", func(r chi.Router) {
			r.Post("/", orderHandlers.Create)
			r.Post("/create-and-place", orderHandlers.CreateAndPlace)
			r.Post("/{id}/place", orderHandlers.Place)
			r.Post("/{id}/lines", orderHandlers.AddLine)
			r.Patch("/{id}/lines/{lineId}", orderHandlers.UpdateLineQuantity)
			r.Delete("/{id}/lines/{lineId}", orderHandlers.RemoveLine)
			r.Put("/{id}/customer", orderHandlers.SetCustomer)
			r.Put("/{id}/address", orderHandlers.SetAddress)
			r.Put("/{id}/fulfillment", orderHandlers.SetFulfillment)
			r.Put("/{id}/comments", orderHandlers.SetComments)
			r.Post("/{id}/accept", orderHandlers.Accept)
			r.Post("/{id}/start", orderHandlers.Start)
			r.Post("/{id}/complete", orderHandlers.Complete)
			r.Post("/{id}/cancel", orderHandlers.Cancel)
		})
	})

	return r
}
