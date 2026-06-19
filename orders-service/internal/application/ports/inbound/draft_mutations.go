package inbound

import (
	"context"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type OrderMutationResult struct {
	Order *domain.Order
}

type AddLineCommand struct {
	TenantID  uuid.UUID
	OrderID   uuid.UUID
	VariantID uuid.UUID
	Quantity  int
}

type AddLine interface {
	Execute(ctx context.Context, cmd AddLineCommand) (OrderMutationResult, error)
}

type UpdateLineQuantityCommand struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
	LineID   uuid.UUID
	Quantity int
}

type UpdateLineQuantity interface {
	Execute(ctx context.Context, cmd UpdateLineQuantityCommand) (OrderMutationResult, error)
}

type RemoveLineCommand struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
	LineID   uuid.UUID
}

type RemoveLine interface {
	Execute(ctx context.Context, cmd RemoveLineCommand) (OrderMutationResult, error)
}

type SetCustomerCommand struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
	Customer domain.CustomerSnapshot
}

type SetCustomer interface {
	Execute(ctx context.Context, cmd SetCustomerCommand) (OrderMutationResult, error)
}

type SetAddressCommand struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
	Address  domain.Address
}

type SetAddress interface {
	Execute(ctx context.Context, cmd SetAddressCommand) (OrderMutationResult, error)
}

type SetFulfillmentTypeCommand struct {
	TenantID        uuid.UUID
	OrderID         uuid.UUID
	FulfillmentType domain.FulfillmentType
}

type SetFulfillmentType interface {
	Execute(ctx context.Context, cmd SetFulfillmentTypeCommand) (OrderMutationResult, error)
}

type SetCommentsCommand struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
	Comments string
}

type SetComments interface {
	Execute(ctx context.Context, cmd SetCommentsCommand) (OrderMutationResult, error)
}
