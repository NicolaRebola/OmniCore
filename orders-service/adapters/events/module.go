package events

import (
	"go.uber.org/fx"

	outboundports "orders-service/internal/application/ports/outbound"
)

var Module = fx.Module("events",
	fx.Provide(
		fx.Annotate(
			NewLoggingPublisher,
			fx.As(new(outboundports.EventPublisher)),
		),
	),
)
