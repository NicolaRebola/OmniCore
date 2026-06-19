package usecases

import (
	"context"

	"github.com/google/uuid"

	inboundports "orders-service/internal/application/ports/inbound"
	outboundports "orders-service/internal/application/ports/outbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"
)

var _ inboundports.CreateAndPlace = (*CreateAndPlaceHandler)(nil)

type CreateAndPlaceHandler struct {
	orders    postgresports.OrderRepository
	sequences postgresports.TenantSequenceRepository
	catalog   outboundports.CatalogReferenceValidator
	events    outboundports.EventPublisher
	tx        outboundports.TransactionManager
	clock     outboundports.Clock
}

func NewCreateAndPlaceHandler(
	orders postgresports.OrderRepository,
	sequences postgresports.TenantSequenceRepository,
	catalog outboundports.CatalogReferenceValidator,
	events outboundports.EventPublisher,
	tx outboundports.TransactionManager,
	clock outboundports.Clock,
) *CreateAndPlaceHandler {
	return &CreateAndPlaceHandler{
		orders:    orders,
		sequences: sequences,
		catalog:   catalog,
		events:    events,
		tx:        tx,
		clock:     clock,
	}
}

func (h *CreateAndPlaceHandler) Execute(
	ctx context.Context,
	cmd inboundports.CreateAndPlaceCommand,
) (inboundports.CreateAndPlaceResult, error) {
	variantIDs := make([]uuid.UUID, len(cmd.Lines))
	for i, line := range cmd.Lines {
		variantIDs[i] = line.VariantID
	}

	snapshots, err := h.catalog.ValidateAndResolve(ctx, cmd.TenantID, variantIDs)
	if err != nil {
		return inboundports.CreateAndPlaceResult{}, err
	}

	now := h.clock.Now()
	orderID := uuid.New()

	var order *domain.Order

	err = h.tx.WithinTransaction(ctx, func(txCtx context.Context) error {
		var err error
		order, err = domain.NewOrder(orderID, cmd.TenantID, cmd.Source, cmd.FulfillmentType, now)
		if err != nil {
			return err
		}

		for _, line := range cmd.Lines {
			if err := order.AddLine(uuid.New(), line.VariantID, line.Quantity, now); err != nil {
				return err
			}
		}

		if err := order.SetCustomer(cmd.Customer, now); err != nil {
			return err
		}

		if cmd.Address != nil {
			if err := order.SetAddress(*cmd.Address, now); err != nil {
				return err
			}
		}

		if cmd.Comments != "" {
			if err := order.SetComments(cmd.Comments, now); err != nil {
				return err
			}
		}

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
		return inboundports.CreateAndPlaceResult{}, err
	}

	_ = h.events.Publish(ctx, buildOrderPlacedEvent(order, now))

	return inboundports.CreateAndPlaceResult{Order: order}, nil
}
