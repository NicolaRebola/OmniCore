package usecases

import (
	"context"
	"time"

	"github.com/google/uuid"

	inboundports "orders-service/internal/application/ports/inbound"
	outboundports "orders-service/internal/application/ports/outbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"
)

type lifecycleTransitionService struct {
	orderLoader
	clock outboundports.Clock
}

func newLifecycleTransitionService(
	orders postgresports.OrderRepository,
	clock outboundports.Clock,
) lifecycleTransitionService {
	return lifecycleTransitionService{
		orderLoader: orderLoader{orders: orders},
		clock:       clock,
	}
}

func (s lifecycleTransitionService) transition(
	ctx context.Context,
	tenantID, orderID uuid.UUID,
	apply func(order *domain.Order, actor domain.Actor, now time.Time) error,
	actor domain.Actor,
) (inboundports.OrderMutationResult, error) {
	order, err := s.loadForMutation(ctx, tenantID, orderID)
	if err != nil {
		return inboundports.OrderMutationResult{}, err
	}

	now := s.clock.Now()
	if err := apply(order, actor, now); err != nil {
		return inboundports.OrderMutationResult{}, err
	}

	if err := s.save(ctx, order); err != nil {
		return inboundports.OrderMutationResult{}, err
	}

	return inboundports.OrderMutationResult{Order: order}, nil
}

type AcceptOrderHandler struct {
	svc lifecycleTransitionService
}

func NewAcceptOrderHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *AcceptOrderHandler {
	return &AcceptOrderHandler{svc: newLifecycleTransitionService(orders, clock)}
}

var _ inboundports.AcceptOrder = (*AcceptOrderHandler)(nil)

func (h *AcceptOrderHandler) Execute(
	ctx context.Context,
	cmd inboundports.LifecycleTransitionCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.transition(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, actor domain.Actor, now time.Time) error {
		return order.Accept(actor, now)
	}, cmd.Actor)
}

type StartOrderHandler struct {
	svc lifecycleTransitionService
}

func NewStartOrderHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *StartOrderHandler {
	return &StartOrderHandler{svc: newLifecycleTransitionService(orders, clock)}
}

var _ inboundports.StartOrder = (*StartOrderHandler)(nil)

func (h *StartOrderHandler) Execute(
	ctx context.Context,
	cmd inboundports.LifecycleTransitionCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.transition(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, actor domain.Actor, now time.Time) error {
		return order.Start(actor, now)
	}, cmd.Actor)
}

type CompleteOrderHandler struct {
	svc lifecycleTransitionService
}

func NewCompleteOrderHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *CompleteOrderHandler {
	return &CompleteOrderHandler{svc: newLifecycleTransitionService(orders, clock)}
}

var _ inboundports.CompleteOrder = (*CompleteOrderHandler)(nil)

func (h *CompleteOrderHandler) Execute(
	ctx context.Context,
	cmd inboundports.LifecycleTransitionCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.transition(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, actor domain.Actor, now time.Time) error {
		return order.Complete(actor, now)
	}, cmd.Actor)
}

type CancelOrderHandler struct {
	svc lifecycleTransitionService
}

func NewCancelOrderHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *CancelOrderHandler {
	return &CancelOrderHandler{svc: newLifecycleTransitionService(orders, clock)}
}

var _ inboundports.CancelOrder = (*CancelOrderHandler)(nil)

func (h *CancelOrderHandler) Execute(
	ctx context.Context,
	cmd inboundports.LifecycleTransitionCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.transition(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, actor domain.Actor, now time.Time) error {
		return order.Cancel(actor, now)
	}, cmd.Actor)
}
