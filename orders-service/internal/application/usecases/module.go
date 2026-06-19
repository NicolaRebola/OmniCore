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
		fx.Annotate(
			NewCreateOrderHandler,
			fx.As(new(inboundports.CreateOrder)),
		),
		fx.Annotate(
			NewAddLineHandler,
			fx.As(new(inboundports.AddLine)),
		),
		fx.Annotate(
			NewUpdateLineQuantityHandler,
			fx.As(new(inboundports.UpdateLineQuantity)),
		),
		fx.Annotate(
			NewRemoveLineHandler,
			fx.As(new(inboundports.RemoveLine)),
		),
		fx.Annotate(
			NewSetCustomerHandler,
			fx.As(new(inboundports.SetCustomer)),
		),
		fx.Annotate(
			NewSetAddressHandler,
			fx.As(new(inboundports.SetAddress)),
		),
		fx.Annotate(
			NewSetFulfillmentTypeHandler,
			fx.As(new(inboundports.SetFulfillmentType)),
		),
		fx.Annotate(
			NewSetCommentsHandler,
			fx.As(new(inboundports.SetComments)),
		),
	),
)
