package usecases

import (
	"context"

	"github.com/google/uuid"

	inboundports "orders-service/internal/application/ports/inbound"
	outboundports "orders-service/internal/application/ports/outbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"
)

var _ inboundports.CreateOrder = (*CreateOrderHandler)(nil)

type CreateOrderHandler struct {
	orders postgresports.OrderRepository
	clock  outboundports.Clock
}

func NewCreateOrderHandler(
	orders postgresports.OrderRepository,
	clock outboundports.Clock,
) *CreateOrderHandler {
	return &CreateOrderHandler{
		orders: orders,
		clock:  clock,
	}
}

func (h *CreateOrderHandler) Execute(
	ctx context.Context,
	cmd inboundports.CreateOrderCommand,
) (inboundports.CreateOrderResult, error) {
	now := h.clock.Now()
	orderID := uuid.New()

	order, err := domain.NewOrder(orderID, cmd.TenantID, cmd.Source, cmd.FulfillmentType, now)
	if err != nil {
		return inboundports.CreateOrderResult{}, err
	}

	if err := h.orders.Save(ctx, order); err != nil {
		return inboundports.CreateOrderResult{}, err
	}

	return inboundports.CreateOrderResult{Order: order}, nil
}
