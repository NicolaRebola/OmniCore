package postgres

import (
	"context"

	"go.uber.org/fx"

	outboundports "orders-service/internal/application/ports/outbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"

	"github.com/jackc/pgx/v5/pgxpool"
)

var Module = fx.Module("postgres",
	fx.Provide(
		NewPoolFromConfig,
		fx.Annotate(
			NewOrderRepository,
			fx.As(new(postgresports.OrderRepository)),
		),
		fx.Annotate(
			NewTenantSequenceRepository,
			fx.As(new(postgresports.TenantSequenceRepository)),
		),
		fx.Annotate(
			NewIdempotencyRepository,
			fx.As(new(postgresports.IdempotencyRepository)),
		),
		fx.Annotate(
			NewTransactionManager,
			fx.As(new(outboundports.TransactionManager)),
		),
	),
)

type Config struct {
	DatabaseURL string
}

func NewPoolFromConfig(cfg Config, lc fx.Lifecycle) (*pgxpool.Pool, error) {
	pool, err := NewPool(context.Background(), cfg.DatabaseURL)
	if err != nil {
		return nil, err
	}

	lc.Append(fx.Hook{
		OnStop: func(ctx context.Context) error {
			pool.Close()
			return nil
		},
	})

	return pool, nil
}
