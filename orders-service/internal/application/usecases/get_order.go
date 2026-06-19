package usecases

import (
	"context"

	inboundports "orders-service/internal/application/ports/inbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
)

var _ inboundports.GetOrder = (*GetOrderHandler)(nil)

type GetOrderHandler struct {
	orderLoader orderLoader
}

func NewGetOrderHandler(orders postgresports.OrderRepository) *GetOrderHandler {
	return &GetOrderHandler{orderLoader: orderLoader{orders: orders}}
}

func (h *GetOrderHandler) Execute(
	ctx context.Context,
	query inboundports.GetOrderQuery,
) (inboundports.GetOrderResult, error) {
	order, err := h.orderLoader.loadForMutation(ctx, query.TenantID, query.OrderID)
	if err != nil {
		return inboundports.GetOrderResult{}, err
	}
	return inboundports.GetOrderResult{Order: order}, nil
}
