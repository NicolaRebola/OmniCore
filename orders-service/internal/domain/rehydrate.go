package domain

import (
	"time"

	"github.com/google/uuid"
)

func RehydrateOrder(
	id, tenantID uuid.UUID,
	orderNumber *int64,
	status OrderStatus,
	source OrderSource,
	fulfillment FulfillmentType,
	customer *CustomerSnapshot,
	address *Address,
	comments string,
	totals *Totals,
	lines []OrderLine,
	transitions []OrderTransition,
	createdAt, updatedAt time.Time,
) *Order {
	return &Order{
		ID:              id,
		TenantID:        tenantID,
		OrderNumber:     orderNumber,
		Status:          status,
		Source:          source,
		FulfillmentType: fulfillment,
		Customer:        customer,
		Address:         address,
		Comments:        comments,
		Totals:          totals,
		lines:           append([]OrderLine(nil), lines...),
		transitions:     append([]OrderTransition(nil), transitions...),
		CreatedAt:       createdAt,
		UpdatedAt:       updatedAt,
	}
}

func (o *Order) Lines() []OrderLine {
	return append([]OrderLine(nil), o.lines...)
}

func (o *Order) Transitions() []OrderTransition {
	return append([]OrderTransition(nil), o.transitions...)
}
