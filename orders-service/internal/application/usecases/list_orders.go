package usecases

import (
	"context"

	inboundports "orders-service/internal/application/ports/inbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
)

var _ inboundports.ListOrders = (*ListOrdersHandler)(nil)

type ListOrdersHandler struct {
	orders postgresports.OrderRepository
}

func NewListOrdersHandler(orders postgresports.OrderRepository) *ListOrdersHandler {
	return &ListOrdersHandler{orders: orders}
}

func (h *ListOrdersHandler) Execute(
	ctx context.Context,
	query inboundports.ListOrdersQuery,
) (inboundports.ListOrdersResult, error) {
	page, err := h.orders.List(ctx, postgresports.ListOrdersFilter{
		TenantID: query.TenantID,
		Status:   query.Status,
		Page:     query.Page,
		PageSize: query.PageSize,
	})
	if err != nil {
		return inboundports.ListOrdersResult{}, err
	}

	return inboundports.ListOrdersResult{
		Orders: page.Orders,
		Total:  page.Total,
	}, nil
}
