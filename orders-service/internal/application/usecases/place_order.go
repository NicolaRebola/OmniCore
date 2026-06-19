package usecases

import (
	"context"
	"time"

	"github.com/google/uuid"

	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	outboundports "orders-service/internal/application/ports/outbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"
)

const orderPlacedEventType = "order.placed"

var _ inboundports.PlaceOrder = (*PlaceOrderHandler)(nil)

type PlaceOrderHandler struct {
	orders    postgresports.OrderRepository
	sequences postgresports.TenantSequenceRepository
	catalog   outboundports.CatalogReferenceValidator
	events    outboundports.EventPublisher
	tx        outboundports.TransactionManager
	clock     outboundports.Clock
}

func NewPlaceOrderHandler(
	orders postgresports.OrderRepository,
	sequences postgresports.TenantSequenceRepository,
	catalog outboundports.CatalogReferenceValidator,
	events outboundports.EventPublisher,
	tx outboundports.TransactionManager,
	clock outboundports.Clock,
) *PlaceOrderHandler {
	return &PlaceOrderHandler{
		orders:    orders,
		sequences: sequences,
		catalog:   catalog,
		events:    events,
		tx:        tx,
		clock:     clock,
	}
}

func (h *PlaceOrderHandler) Execute(
	ctx context.Context,
	cmd inboundports.PlaceOrderCommand,
) (inboundports.PlaceOrderResult, error) {
	order, err := h.orders.GetByID(ctx, cmd.TenantID, cmd.OrderID)
	if err != nil {
		return inboundports.PlaceOrderResult{}, err
	}
	if order == nil {
		return inboundports.PlaceOrderResult{}, application.ErrOrderNotFound
	}

	variantIDs := extractVariantIDs(order.Lines())

	snapshots, err := h.catalog.ValidateAndResolve(ctx, cmd.TenantID, variantIDs)
	if err != nil {
		return inboundports.PlaceOrderResult{}, err
	}

	now := h.clock.Now()

	err = h.tx.WithinTransaction(ctx, func(txCtx context.Context) error {
		orderNumber, err := h.sequences.NextOrderNumber(txCtx, cmd.TenantID)
		if err != nil {
			return err
		}

		if err := order.Place(domain.PlaceInput{
			OrderNumber: orderNumber,
			Variants:    snapshots,
			Actor:       cmd.Actor,
			OccurredAt:  now,
		}); err != nil {
			return err
		}

		return h.orders.Save(txCtx, order)
	})
	if err != nil {
		return inboundports.PlaceOrderResult{}, err
	}

	_ = h.events.Publish(ctx, buildOrderPlacedEvent(order, now))

	return inboundports.PlaceOrderResult{Order: order}, nil
}

func extractVariantIDs(lines []domain.OrderLine) []uuid.UUID {
	ids := make([]uuid.UUID, len(lines))
	for i, line := range lines {
		ids[i] = line.VariantID
	}
	return ids
}

func buildOrderPlacedEvent(order *domain.Order, occurredAt time.Time) outboundports.IntegrationEvent {
	var orderNumber int64
	if order.OrderNumber != nil {
		orderNumber = *order.OrderNumber
	}

	return outboundports.IntegrationEvent{
		SpecVersion: "1.0",
		EventID:     uuid.New().String(),
		EventType:   orderPlacedEventType,
		Source:      "orders-service",
		OccurredAt:  occurredAt.UTC().Format(time.RFC3339),
		TenantID:    order.TenantID.String(),
		Data: map[string]any{
			"orderId":     order.ID.String(),
			"orderNumber": orderNumber,
			"tenantId":    order.TenantID.String(),
			"status":      string(order.Status),
			"occurredAt":  occurredAt.UTC().Format(time.RFC3339),
		},
	}
}
