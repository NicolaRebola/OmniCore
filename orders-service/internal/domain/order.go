// Package domain contains the Order aggregate, value objects, state machine,
// and domain errors. It must not import adapters, HTTP, or database drivers.
package domain

import (
	"fmt"
	"time"

	"github.com/google/uuid"
)

const MaxCommentsLength = 500

type Order struct {
	ID              uuid.UUID
	TenantID        uuid.UUID
	OrderNumber     *int64
	Status          OrderStatus
	Source          OrderSource
	FulfillmentType FulfillmentType
	Customer        *CustomerSnapshot
	Address         *Address
	Comments        string
	Totals          *Totals

	lines       []OrderLine
	transitions []OrderTransition

	CreatedAt time.Time
	UpdatedAt time.Time
}

func NewOrder(
	id, tenantID uuid.UUID,
	source OrderSource,
	fulfillment FulfillmentType,
	now time.Time,
) (*Order, error) {
	if tenantID == uuid.Nil {
		return nil, nil
	}

	return &Order{
		ID:              id,
		TenantID:        tenantID,
		Status:          StatusDraft,
		Source:          source,
		FulfillmentType: fulfillment,
		CreatedAt:       now,
		UpdatedAt:       now,
	}, nil
}

func (o *Order) ensureDraft() error {
	if o.Status != StatusDraft {
		return ErrMutationNotInDraft
	}
	return nil
}

func (o *Order) touch(now time.Time) {
	o.UpdatedAt = now
}

func (o *Order) AddLine(lineID, variantID uuid.UUID, quantity int, now time.Time) error {
	if err := o.ensureDraft(); err != nil {
		return err
	}
	if quantity <= 0 {
		return ErrInvalidQuantity
	}
	if variantID == uuid.Nil {
		return ErrInvalidVariant // o error más específico
	}

	o.lines = append(o.lines, NewDraftOrderLine(lineID, variantID, quantity))
	o.touch(now)
	return nil
}

func (o *Order) UpdateLineQuantity(lineID uuid.UUID, quantity int, now time.Time) error {
	if err := o.ensureDraft(); err != nil {
		return err
	}
	if quantity <= 0 {
		return ErrInvalidQuantity
	}

	idx := o.findLineIndex(lineID)
	if idx < 0 {
		return nil /* line not found — puedes usar fmt.Errorf o nuevo ORD-DOM */
	}

	o.lines[idx].Quantity = quantity
	o.touch(now)
	return nil
}

func (o *Order) removeLineAt(index int) {
	o.lines = append(o.lines[:index], o.lines[index+1:]...)
}

func (o *Order) SetCustomer(c CustomerSnapshot, now time.Time) error {
	if err := o.ensureDraft(); err != nil {
		return err
	}
	copy := c
	o.Customer = &copy
	o.touch(now)
	return nil
}

func (o *Order) SetComments(comments string, now time.Time) error {
	if err := o.ensureDraft(); err != nil {
		return err
	}
	if len(comments) > MaxCommentsLength {
		return fmt.Errorf("comments exceed max length")
	}
	o.Comments = comments
	o.touch(now)
	return nil
}

func (o *Order) findLineIndex(lineID uuid.UUID) int {
	for i, line := range o.lines {
		if line.ID == lineID {
			return i
		}
	}
	return -1
}

func indexSnapshotsByVariant(snapshots []VariantSnapshot) map[uuid.UUID]VariantSnapshot {
	m := make(map[uuid.UUID]VariantSnapshot, len(snapshots))
	for _, s := range snapshots {
		m[s.VariantID] = s
	}
	return m
}

func (o *Order) RemoveLine(lineID uuid.UUID, now time.Time) error {
	if err := o.ensureDraft(); err != nil {
		return err
	}
	idx := o.findLineIndex(lineID)
	if idx < 0 {
		return fmt.Errorf("order line not found") // o DomainError
	}
	o.removeLineAt(idx)
	o.touch(now)
	return nil
}

func (o *Order) SetAddress(a Address, now time.Time) error {
	if err := o.ensureDraft(); err != nil {
		return err
	}
	copy := a
	o.Address = &copy
	o.touch(now)
	return nil
}

func (o *Order) SetFulfillmentType(ft FulfillmentType, now time.Time) error {
	if err := o.ensureDraft(); err != nil {
		return err
	}
	o.FulfillmentType = ft
	o.touch(now)
	return nil
}
