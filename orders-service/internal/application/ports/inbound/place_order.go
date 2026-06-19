package inbound

import (
	"context"
	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type PlaceOrderCommand struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
	Actor    domain.Actor
}

type PlaceOrderResult struct {
	Order *domain.Order
}

type PlaceOrder interface {
	Execute(ctx context.Context, cmd PlaceOrderCommand) (PlaceOrderResult, error)
}
