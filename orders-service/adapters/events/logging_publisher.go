package events

import (
	"context"
	"log/slog"

	outboundports "orders-service/internal/application/ports/outbound"
)

type LoggingPublisher struct {
	logger *slog.Logger
}

func NewLoggingPublisher(logger *slog.Logger) *LoggingPublisher {
	return &LoggingPublisher{logger: logger}
}

var _ outboundports.EventPublisher = (*LoggingPublisher)(nil)

func (p *LoggingPublisher) Publish(ctx context.Context, event outboundports.IntegrationEvent) error {
	p.logger.InfoContext(ctx, "integration event published",
		slog.String("eventType", event.EventType),
		slog.String("eventId", event.EventID),
		slog.String("tenantId", event.TenantID),
		slog.Any("data", event.Data),
	)
	return nil
}
