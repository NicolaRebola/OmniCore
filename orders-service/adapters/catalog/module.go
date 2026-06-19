package catalog

import (
	"go.uber.org/fx"

	outboundports "orders-service/internal/application/ports/outbound"
)

var Module = fx.Module("catalog",
	fx.Provide(
		fx.Annotate(
			NewClientFromConfig,
			fx.As(new(outboundports.CatalogReferenceValidator)),
		),
	),
)

type Config struct {
	BaseURL string
}

func NewClientFromConfig(cfg Config) *Client {
	return NewClient(cfg.BaseURL)
}
