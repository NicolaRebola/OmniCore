package outbound

import (
	"context"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type OrderRepository interface {
	Save(ctx context.Context, order *domain.Order) error
	GetByID(ctx context.Context, tenantID, orderID uuid.UUID) (*domain.Order, error)
}
