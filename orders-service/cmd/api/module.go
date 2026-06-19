package main

import (
	"go.uber.org/fx"

	catalogadapter "orders-service/adapters/catalog"
	eventsadapter "orders-service/adapters/events"
	httpadapter "orders-service/adapters/http"
	postgresadapter "orders-service/adapters/postgres"
	outboundports "orders-service/internal/application/ports/outbound"
	"orders-service/internal/application/usecases"
)

var AppModule = fx.Options(
	fx.Provide(
		LoadConfig,
		NewLogger,
		ProvidePostgresConfig,
		ProvideCatalogConfig,
		ProvideSystemClock,
	),
	postgresadapter.Module,
	catalogadapter.Module,
	eventsadapter.Module,
	usecases.Module,
	httpadapter.Module,
)

func ProvidePostgresConfig(cfg Config) postgresadapter.Config {
	return postgresadapter.Config{DatabaseURL: cfg.DatabaseURL}
}

func ProvideCatalogConfig(cfg Config) catalogadapter.Config {
	return catalogadapter.Config{BaseURL: cfg.CatalogBaseURL}
}

func ProvideSystemClock() outboundports.Clock {
	return outboundports.SystemClock{}
}
