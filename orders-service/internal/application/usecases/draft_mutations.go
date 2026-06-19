package usecases

import (
	"context"
	"time"

	"github.com/google/uuid"

	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	outboundports "orders-service/internal/application/ports/outbound"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"
)

type draftMutationService struct {
	orderLoader
	clock outboundports.Clock
}

func newDraftMutationService(
	orders postgresports.OrderRepository,
	clock outboundports.Clock,
) draftMutationService {
	return draftMutationService{
		orderLoader: orderLoader{orders: orders},
		clock:       clock,
	}
}

type orderMutator func(order *domain.Order, now time.Time) error

func (s draftMutationService) mutate(
	ctx context.Context,
	tenantID, orderID uuid.UUID,
	mutate orderMutator,
) (inboundports.OrderMutationResult, error) {
	order, err := s.loadForMutation(ctx, tenantID, orderID)
	if err != nil {
		return inboundports.OrderMutationResult{}, err
	}

	now := s.clock.Now()
	if err := mutate(order, now); err != nil {
		return inboundports.OrderMutationResult{}, err
	}

	if err := s.save(ctx, order); err != nil {
		return inboundports.OrderMutationResult{}, err
	}

	return inboundports.OrderMutationResult{Order: order}, nil
}

type AddLineHandler struct {
	svc draftMutationService
}

func NewAddLineHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *AddLineHandler {
	return &AddLineHandler{svc: newDraftMutationService(orders, clock)}
}

var _ inboundports.AddLine = (*AddLineHandler)(nil)

func (h *AddLineHandler) Execute(
	ctx context.Context,
	cmd inboundports.AddLineCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.mutate(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, now time.Time) error {
		return order.AddLine(uuid.New(), cmd.VariantID, cmd.Quantity, now)
	})
}

type UpdateLineQuantityHandler struct {
	svc draftMutationService
}

func NewUpdateLineQuantityHandler(
	orders postgresports.OrderRepository,
	clock outboundports.Clock,
) *UpdateLineQuantityHandler {
	return &UpdateLineQuantityHandler{svc: newDraftMutationService(orders, clock)}
}

var _ inboundports.UpdateLineQuantity = (*UpdateLineQuantityHandler)(nil)

func (h *UpdateLineQuantityHandler) Execute(
	ctx context.Context,
	cmd inboundports.UpdateLineQuantityCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.mutate(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, now time.Time) error {
		if !orderHasLine(order, cmd.LineID) {
			return application.ErrLineNotFound
		}
		return order.UpdateLineQuantity(cmd.LineID, cmd.Quantity, now)
	})
}

type RemoveLineHandler struct {
	svc draftMutationService
}

func NewRemoveLineHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *RemoveLineHandler {
	return &RemoveLineHandler{svc: newDraftMutationService(orders, clock)}
}

var _ inboundports.RemoveLine = (*RemoveLineHandler)(nil)

func (h *RemoveLineHandler) Execute(
	ctx context.Context,
	cmd inboundports.RemoveLineCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.mutate(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, now time.Time) error {
		if !orderHasLine(order, cmd.LineID) {
			return application.ErrLineNotFound
		}
		return order.RemoveLine(cmd.LineID, now)
	})
}

type SetCustomerHandler struct {
	svc draftMutationService
}

func NewSetCustomerHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *SetCustomerHandler {
	return &SetCustomerHandler{svc: newDraftMutationService(orders, clock)}
}

var _ inboundports.SetCustomer = (*SetCustomerHandler)(nil)

func (h *SetCustomerHandler) Execute(
	ctx context.Context,
	cmd inboundports.SetCustomerCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.mutate(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, now time.Time) error {
		return order.SetCustomer(cmd.Customer, now)
	})
}

type SetAddressHandler struct {
	svc draftMutationService
}

func NewSetAddressHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *SetAddressHandler {
	return &SetAddressHandler{svc: newDraftMutationService(orders, clock)}
}

var _ inboundports.SetAddress = (*SetAddressHandler)(nil)

func (h *SetAddressHandler) Execute(
	ctx context.Context,
	cmd inboundports.SetAddressCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.mutate(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, now time.Time) error {
		return order.SetAddress(cmd.Address, now)
	})
}

type SetFulfillmentTypeHandler struct {
	svc draftMutationService
}

func NewSetFulfillmentTypeHandler(
	orders postgresports.OrderRepository,
	clock outboundports.Clock,
) *SetFulfillmentTypeHandler {
	return &SetFulfillmentTypeHandler{svc: newDraftMutationService(orders, clock)}
}

var _ inboundports.SetFulfillmentType = (*SetFulfillmentTypeHandler)(nil)

func (h *SetFulfillmentTypeHandler) Execute(
	ctx context.Context,
	cmd inboundports.SetFulfillmentTypeCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.mutate(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, now time.Time) error {
		return order.SetFulfillmentType(cmd.FulfillmentType, now)
	})
}

type SetCommentsHandler struct {
	svc draftMutationService
}

func NewSetCommentsHandler(orders postgresports.OrderRepository, clock outboundports.Clock) *SetCommentsHandler {
	return &SetCommentsHandler{svc: newDraftMutationService(orders, clock)}
}

var _ inboundports.SetComments = (*SetCommentsHandler)(nil)

func (h *SetCommentsHandler) Execute(
	ctx context.Context,
	cmd inboundports.SetCommentsCommand,
) (inboundports.OrderMutationResult, error) {
	return h.svc.mutate(ctx, cmd.TenantID, cmd.OrderID, func(order *domain.Order, now time.Time) error {
		return order.SetComments(cmd.Comments, now)
	})
}
