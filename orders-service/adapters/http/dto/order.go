package dto

import (
	"strconv"
	"time"

	"github.com/google/uuid"

	httperrors "orders-service/adapters/http/errors"
	"orders-service/internal/domain"
)

type MoneyResponse struct {
	Amount   int64  `json:"amount"`
	Currency string `json:"currency"`
}

type CustomerResponse struct {
	Name  string `json:"name"`
	Email string `json:"email,omitempty"`
	Phone string `json:"phone,omitempty"`
}

type AddressResponse struct {
	Street     string `json:"street"`
	Number     string `json:"number"`
	Apartment  string `json:"apartment,omitempty"`
	City       string `json:"city"`
	References string `json:"references,omitempty"`
}

type OrderLineResponse struct {
	ID            uuid.UUID      `json:"id"`
	VariantID     uuid.UUID      `json:"variantId"`
	CatalogItemID *uuid.UUID     `json:"catalogItemId,omitempty"`
	Name          *string        `json:"name,omitempty"`
	UnitPrice     *MoneyResponse `json:"unitPrice,omitempty"`
	Quantity      int            `json:"quantity"`
	LineTotal     *MoneyResponse `json:"lineTotal,omitempty"`
}

type TransitionResponse struct {
	FromStatus string    `json:"fromStatus"`
	ToStatus   string    `json:"toStatus"`
	OccurredAt time.Time `json:"occurredAt"`
	ActorType  string    `json:"actorType"`
	ActorID    string    `json:"actorId,omitempty"`
	Reason     string    `json:"reason,omitempty"`
}

type TotalsResponse struct {
	Subtotal MoneyResponse `json:"subtotal"`
	Total    MoneyResponse `json:"total"`
}

type OrderResponse struct {
	ID              uuid.UUID            `json:"id"`
	TenantID        uuid.UUID            `json:"tenantId"`
	OrderNumber     *int64               `json:"orderNumber"`
	Status          string               `json:"status"`
	Source          string               `json:"source"`
	FulfillmentType string               `json:"fulfillmentType"`
	Customer        *CustomerResponse    `json:"customer,omitempty"`
	Address         *AddressResponse     `json:"address,omitempty"`
	Comments        string               `json:"comments"`
	Totals          *TotalsResponse      `json:"totals,omitempty"`
	Lines           []OrderLineResponse  `json:"lines"`
	Transitions     []TransitionResponse `json:"transitions"`
	CreatedAt       time.Time            `json:"createdAt"`
	UpdatedAt       time.Time            `json:"updatedAt"`
}

type CreateOrderRequest struct {
	Source          string `json:"source"`
	FulfillmentType string `json:"fulfillmentType"`
}

type CreateAndPlaceLineRequest struct {
	VariantID uuid.UUID `json:"variantId"`
	Quantity  int       `json:"quantity"`
}

type CreateAndPlaceRequest struct {
	Source          string                      `json:"source"`
	FulfillmentType string                      `json:"fulfillmentType"`
	Customer        CustomerRequest             `json:"customer"`
	Address         *AddressRequest             `json:"address,omitempty"`
	Comments        string                      `json:"comments,omitempty"`
	Lines           []CreateAndPlaceLineRequest `json:"lines"`
	ActorType       string                      `json:"actorType"`
	ActorID         string                      `json:"actorId,omitempty"`
}

type AddLineRequest struct {
	VariantID uuid.UUID `json:"variantId"`
	Quantity  int       `json:"quantity"`
}

type UpdateLineQuantityRequest struct {
	Quantity int `json:"quantity"`
}

type CustomerRequest struct {
	Name  string `json:"name"`
	Email string `json:"email,omitempty"`
	Phone string `json:"phone,omitempty"`
}

type AddressRequest struct {
	Street     string `json:"street"`
	Number     string `json:"number"`
	Apartment  string `json:"apartment,omitempty"`
	City       string `json:"city"`
	References string `json:"references,omitempty"`
}

type SetFulfillmentRequest struct {
	FulfillmentType string `json:"fulfillmentType"`
}

type SetCommentsRequest struct {
	Comments string `json:"comments"`
}

type ActorRequest struct {
	ActorType string `json:"actorType"`
	ActorID   string `json:"actorId,omitempty"`
	Reason    string `json:"reason,omitempty"`
}

type ListOrdersResponse struct {
	Items    []OrderResponse `json:"items"`
	Page     int             `json:"page"`
	PageSize int             `json:"pageSize"`
	Total    int             `json:"total"`
}

const (
	defaultPage     = 1
	defaultPageSize = 20
	maxPageSize     = 100
)

type PaginationParams struct {
	Page     int
	PageSize int
}

func ParsePagination(pageStr, pageSizeStr string) (PaginationParams, error) {
	page := defaultPage
	pageSize := defaultPageSize

	if pageStr != "" {
		p, err := parsePositiveInt(pageStr)
		if err != nil || p < 1 {
			return PaginationParams{}, httperrors.ErrInvalidPagination
		}
		page = p
	}

	if pageSizeStr != "" {
		ps, err := parsePositiveInt(pageSizeStr)
		if err != nil || ps < 1 || ps > maxPageSize {
			return PaginationParams{}, httperrors.ErrInvalidPagination
		}
		pageSize = ps
	}

	return PaginationParams{Page: page, PageSize: pageSize}, nil
}

func ListOrdersFromDomain(orders []*domain.Order, page, pageSize, total int) ListOrdersResponse {
	items := make([]OrderResponse, 0, len(orders))
	for _, order := range orders {
		items = append(items, OrderFromDomain(order))
	}
	return ListOrdersResponse{
		Items:    items,
		Page:     page,
		PageSize: pageSize,
		Total:    total,
	}
}

func OrderFromDomain(order *domain.Order) OrderResponse {
	resp := OrderResponse{
		ID:              order.ID,
		TenantID:        order.TenantID,
		OrderNumber:     order.OrderNumber,
		Status:          string(order.Status),
		Source:          string(order.Source),
		FulfillmentType: string(order.FulfillmentType),
		Comments:        order.Comments,
		Lines:           make([]OrderLineResponse, 0, len(order.Lines())),
		Transitions:     make([]TransitionResponse, 0, len(order.Transitions())),
		CreatedAt:       order.CreatedAt,
		UpdatedAt:       order.UpdatedAt,
	}

	if order.Customer != nil {
		resp.Customer = &CustomerResponse{
			Name:  order.Customer.Name,
			Email: order.Customer.Email,
			Phone: order.Customer.Phone,
		}
	}

	if order.Address != nil {
		resp.Address = &AddressResponse{
			Street:     order.Address.Street,
			Number:     order.Address.Number,
			Apartment:  order.Address.Apartment,
			City:       order.Address.City,
			References: order.Address.References,
		}
	}

	if order.Totals != nil {
		resp.Totals = &TotalsResponse{
			Subtotal: moneyFromDomain(order.Totals.Subtotal),
			Total:    moneyFromDomain(order.Totals.Total),
		}
	}

	for _, line := range order.Lines() {
		resp.Lines = append(resp.Lines, orderLineFromDomain(line))
	}

	for _, tr := range order.Transitions() {
		resp.Transitions = append(resp.Transitions, TransitionResponse{
			FromStatus: string(tr.FromStatus),
			ToStatus:   string(tr.ToStatus),
			OccurredAt: tr.OccurredAt,
			ActorType:  string(tr.ActorType),
			ActorID:    tr.ActorID,
			Reason:     tr.Reason,
		})
	}

	return resp
}

func orderLineFromDomain(line domain.OrderLine) OrderLineResponse {
	resp := OrderLineResponse{
		ID:            line.ID,
		VariantID:     line.VariantID,
		CatalogItemID: line.CatalogItemID,
		Name:          line.Name,
		Quantity:      line.Quantity,
	}

	if line.UnitPrice != nil {
		m := moneyFromDomain(*line.UnitPrice)
		resp.UnitPrice = &m
	}
	if line.LineTotal != nil {
		m := moneyFromDomain(*line.LineTotal)
		resp.LineTotal = &m
	}

	return resp
}

func moneyFromDomain(m domain.Money) MoneyResponse {
	return MoneyResponse{Amount: m.Amount, Currency: m.Currency}
}

func ParseOrderSource(value string) (domain.OrderSource, error) {
	switch domain.OrderSource(value) {
	case domain.SourcePOS, domain.SourceQRMenu, domain.SourceWeb, domain.SourceBackoffice:
		return domain.OrderSource(value), nil
	default:
		return "", httperrors.ErrInvalidSource
	}
}

func ParseFulfillmentType(value string) (domain.FulfillmentType, error) {
	switch domain.FulfillmentType(value) {
	case domain.FulfillmentDineIn, domain.FulfillmentTakeaway, domain.FulfillmentDelivery:
		return domain.FulfillmentType(value), nil
	default:
		return "", httperrors.ErrInvalidFulfillmentType
	}
}

func ParseActorType(value string) (domain.ActorType, error) {
	switch domain.ActorType(value) {
	case domain.ActorBuyer, domain.ActorStaff, domain.ActorAdmin:
		return domain.ActorType(value), nil
	default:
		return "", httperrors.ErrInvalidActorType
	}
}

func ParseOrderStatus(value string) (domain.OrderStatus, error) {
	switch domain.OrderStatus(value) {
	case domain.StatusDraft, domain.StatusPlaced, domain.StatusAccepted,
		domain.StatusInProgress, domain.StatusCompleted, domain.StatusCancelled:
		return domain.OrderStatus(value), nil
	default:
		return "", httperrors.ErrInvalidStatus
	}
}

func parsePositiveInt(value string) (int, error) {
	n, err := strconv.Atoi(value)
	if err != nil {
		return 0, err
	}
	return n, nil
}

func ActorFromRequest(req ActorRequest) (domain.Actor, error) {
	actorType, err := ParseActorType(req.ActorType)
	if err != nil {
		return domain.Actor{}, err
	}

	return domain.Actor{
		Type:   actorType,
		ID:     req.ActorID,
		Reason: req.Reason,
	}, nil
}
