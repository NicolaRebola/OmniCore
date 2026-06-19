package inbound

import (
	"context"

	"github.com/google/uuid"

	"orders-service/internal/domain"
)

type CreateAndPlaceLine struct {
	VariantID uuid.UUID
	Quantity  int
}

type CreateAndPlaceCommand struct {
	TenantID        uuid.UUID
	Source          domain.OrderSource
	FulfillmentType domain.FulfillmentType
	Customer        domain.CustomerSnapshot
	Address         *domain.Address
	Comments        string
	Lines           []CreateAndPlaceLine
	Actor           domain.Actor
}

type CreateAndPlaceResult struct {
	Order *domain.Order
}

type CreateAndPlace interface {
	Execute(ctx context.Context, cmd CreateAndPlaceCommand) (CreateAndPlaceResult, error)
}
