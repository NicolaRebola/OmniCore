package inbound

import (
	"context"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type GetOrderQuery struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
}

type GetOrderResult struct {
	Order *domain.Order
}

type GetOrder interface {
	Execute(ctx context.Context, query GetOrderQuery) (GetOrderResult, error)
}

type ListOrdersQuery struct {
	TenantID uuid.UUID
	Status   *domain.OrderStatus
	Page     int
	PageSize int
}

type ListOrdersResult struct {
	Orders []*domain.Order
	Total  int
}

type ListOrders interface {
	Execute(ctx context.Context, query ListOrdersQuery) (ListOrdersResult, error)
}
