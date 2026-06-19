package usecases

import (
	"go.uber.org/fx"

	inboundports "orders-service/internal/application/ports/inbound"
)

var Module = fx.Module("usecases",
	fx.Provide(
		fx.Annotate(
			NewPlaceOrderHandler,
			fx.As(new(inboundports.PlaceOrder)),
		),
	),
)
