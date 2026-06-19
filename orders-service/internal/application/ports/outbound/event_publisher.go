package outbound

import "context"

type IntegrationEvent struct {
	SpecVersion   string
	EventID       string
	EventType     string
	Source        string
	OccurredAt    string
	TenantID      string
	CorrelationID string
	Data          any
}

type EventPublisher interface {
	Publish(ctx context.Context, event IntegrationEvent) error
}
