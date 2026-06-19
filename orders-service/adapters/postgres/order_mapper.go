package postgres

import (
	"fmt"
	"time"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

func orderRowToDomain(row orderRow, lineRows []orderLineRow, transitionRows []orderTransitionRow) (*domain.Order, error) {
	customer, err := customerFromJSON(row.CustomerJSON)
	if err != nil {
		return nil, fmt.Errorf("customer_json: %w", err)
	}
	address, err := addressFromJSON(row.AddressJSON)
	if err != nil {
		return nil, fmt.Errorf("address_json: %w", err)
	}
	totals, err := totalsFromJSON(row.TotalsJSON)
	if err != nil {
		return nil, fmt.Errorf("totals_json: %w", err)
	}

	lines := make([]domain.OrderLine, 0, len(lineRows))
	for _, lr := range lineRows {
		line, err := orderLineRowToDomain(lr)
		if err != nil {
			return nil, err
		}
		lines = append(lines, line)
	}

	transitions := make([]domain.OrderTransition, 0, len(transitionRows))
	for _, tr := range transitionRows {
		transitions = append(transitions, orderTransitionRowToDomain(tr))
	}

	return domain.RehydrateOrder(
		row.ID,
		row.TenantID,
		row.OrderNumber,
		domain.OrderStatus(row.Status),
		domain.OrderSource(row.Source),
		domain.FulfillmentType(row.FulfillmentType),
		customer,
		address,
		row.Comments,
		totals,
		lines,
		transitions,
		row.CreatedAt,
		row.UpdatedAt,
	), nil
}

func orderLineRowToDomain(row orderLineRow) (domain.OrderLine, error) {
	unitPrice, err := optionalMoneyFromJSON(row.UnitPriceJSON)
	if err != nil {
		return domain.OrderLine{}, fmt.Errorf("unit_price_json: %w", err)
	}
	lineTotal, err := optionalMoneyFromJSON(row.LineTotalJSON)
	if err != nil {
		return domain.OrderLine{}, fmt.Errorf("line_total_json: %w", err)
	}

	return domain.OrderLine{
		ID:            row.ID,
		VariantID:     row.VariantID,
		CatalogItemID: row.CatalogItemID,
		Name:          row.Name,
		UnitPrice:     unitPrice,
		Quantity:      row.Quantity,
		LineTotal:     lineTotal,
	}, nil
}

func orderTransitionRowToDomain(row orderTransitionRow) domain.OrderTransition {
	return domain.OrderTransition{
		FromStatus: domain.OrderStatus(row.FromStatus),
		ToStatus:   domain.OrderStatus(row.ToStatus),
		OccurredAt: row.OccurredAt,
		ActorType:  domain.ActorType(row.ActorType),
		ActorID:    row.ActorID,
		Reason:     row.Reason,
	}
}

type orderPersistParams struct {
	ID              uuid.UUID
	TenantID        uuid.UUID
	OrderNumber     *int64
	Source          string
	FulfillmentType string
	Status          string
	CustomerJSON    []byte
	AddressJSON     []byte
	Comments        string
	TotalsJSON      []byte
	CreatedAt       time.Time
	UpdatedAt       time.Time
}

func orderToPersistParams(order *domain.Order) (orderPersistParams, error) {
	customerJSON, err := customerToJSON(order.Customer)
	if err != nil {
		return orderPersistParams{}, fmt.Errorf("customer: %w", err)
	}
	addressJSON, err := addressToJSON(order.Address)
	if err != nil {
		return orderPersistParams{}, fmt.Errorf("address: %w", err)
	}
	totalsJSON, err := totalsToJSON(order.Totals)
	if err != nil {
		return orderPersistParams{}, fmt.Errorf("totals: %w", err)
	}

	return orderPersistParams{
		ID:              order.ID,
		TenantID:        order.TenantID,
		OrderNumber:     order.OrderNumber,
		Source:          string(order.Source),
		FulfillmentType: string(order.FulfillmentType),
		Status:          string(order.Status),
		CustomerJSON:    customerJSON,
		AddressJSON:     addressJSON,
		Comments:        order.Comments,
		TotalsJSON:      totalsJSON,
		CreatedAt:       order.CreatedAt,
		UpdatedAt:       order.UpdatedAt,
	}, nil
}
