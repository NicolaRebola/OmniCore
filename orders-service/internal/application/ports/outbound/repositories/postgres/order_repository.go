package outbound

import (
	"context"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type ListOrdersFilter struct {
	TenantID uuid.UUID
	Status   *domain.OrderStatus
	Page     int
	PageSize int
}

type ListOrdersPage struct {
	Orders []*domain.Order
	Total  int
}

type OrderRepository interface {
	Save(ctx context.Context, order *domain.Order) error
	GetByID(ctx context.Context, tenantID, orderID uuid.UUID) (*domain.Order, error)
	List(ctx context.Context, filter ListOrdersFilter) (ListOrdersPage, error)
}
