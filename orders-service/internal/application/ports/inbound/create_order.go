package inbound

import (
	"context"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type CreateOrderCommand struct {
	TenantID        uuid.UUID
	Source          domain.OrderSource
	FulfillmentType domain.FulfillmentType
}

type CreateOrderResult struct {
	Order *domain.Order
}

type CreateOrder interface {
	Execute(ctx context.Context, cmd CreateOrderCommand) (CreateOrderResult, error)
}
