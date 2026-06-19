package inbound

import (
	"context"

	"github.com/google/uuid"

	"orders-service/internal/domain"
)

type LifecycleTransitionCommand struct {
	TenantID uuid.UUID
	OrderID  uuid.UUID
	Actor    domain.Actor
}

type AcceptOrder interface {
	Execute(ctx context.Context, cmd LifecycleTransitionCommand) (OrderMutationResult, error)
}

type StartOrder interface {
	Execute(ctx context.Context, cmd LifecycleTransitionCommand) (OrderMutationResult, error)
}

type CompleteOrder interface {
	Execute(ctx context.Context, cmd LifecycleTransitionCommand) (OrderMutationResult, error)
}

type CancelOrder interface {
	Execute(ctx context.Context, cmd LifecycleTransitionCommand) (OrderMutationResult, error)
}
