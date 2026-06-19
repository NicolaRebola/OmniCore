package http

import (
	"go.uber.org/fx"

	"orders-service/adapters/http/handlers"
	"orders-service/adapters/http/idempotency"
)

var Module = fx.Module("http",
	fx.Provide(
		fx.Annotate(
			idempotency.NewStore,
			fx.As(new(idempotency.ReplayStore)),
		),
		handlers.NewOrderHandlers,
		NewRouter,
	),
)
