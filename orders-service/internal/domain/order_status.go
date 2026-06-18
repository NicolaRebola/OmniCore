package domain

import (
	"time"
)

var allowedTransitions = map[OrderStatus][]OrderStatus{
	StatusDraft:      {StatusPlaced, StatusCancelled},
	StatusPlaced:     {StatusAccepted, StatusCancelled},
	StatusAccepted:   {StatusInProgress, StatusCancelled},
	StatusInProgress: {StatusCompleted, StatusCancelled},
}

func canTransition(from, to OrderStatus) bool {
	for _, allowed := range allowedTransitions[from] {
		if allowed == to {
			return true
		}
	}
	return false
}

func (o *Order) applyTransition(to OrderStatus, actor Actor, now time.Time) error {
	if !canTransition(o.Status, to) {
		return ErrInvalidTransition
	}

	if to == StatusCancelled && o.Status != StatusDraft && actor.Reason == "" {
		return ErrCancelReasonRequired
	}

	transition := NewOrderTransition(o.Status, to, actor, now)
	o.transitions = append(o.transitions, transition)
	o.Status = to
	o.touch(now)
	return nil
}

type PlaceInput struct {
	OrderNumber int64
	Variants    []VariantSnapshot
	Actor       Actor
	OccurredAt  time.Time
}

func (o *Order) Place(in PlaceInput) error {
	// 1. Solo desde Draft
	if o.Status != StatusDraft {
		return ErrInvalidTransition
	}

	// 2. Líneas
	if len(o.lines) == 0 {
		return ErrPlaceWithoutLines
	}
	for _, line := range o.lines {
		if line.Quantity <= 0 {
			return ErrInvalidQuantity
		}
	}

	// 3. Customer
	if o.Customer == nil || !o.Customer.HasName() {
		return ErrPlaceWithoutCustomer
	}
	// 4. Delivery + address
	if o.FulfillmentType == FulfillmentDelivery {
		if o.Address == nil || !o.Address.IsCompleteForDelivery() {
			return ErrDeliveryWithoutAddr
		}
	}

	// 5. Snapshots + congelar líneas
	byVariant := indexSnapshotsByVariant(in.Variants)
	var subtotal Money

	for i, line := range o.lines {
		snap, ok := byVariant[line.VariantID]
		if !ok || !snap.IsValidForPlace() {
			return ErrInvalidVariant
		}

		lineTotal := snap.UnitPrice.Multiply(line.Quantity)
		name := snap.Name
		catalogItemID := snap.CatalogItemID
		unitPrice := snap.UnitPrice

		o.lines[i].Name = &name
		o.lines[i].CatalogItemID = &catalogItemID
		o.lines[i].UnitPrice = &unitPrice
		o.lines[i].LineTotal = &lineTotal

		if subtotal.IsZero() {
			subtotal = lineTotal
		} else {
			var err error
			subtotal, err = subtotal.Add(lineTotal)
			if err != nil {
				return err
			}
		}
	}

	// 6. Totals
	totals := NewTotals(subtotal)
	o.Totals = &totals

	// 7. Order number
	o.OrderNumber = &in.OrderNumber

	// 8. Transición Draft → Placed
	return o.applyTransition(StatusPlaced, in.Actor, in.OccurredAt)
}

func (o *Order) Accept(actor Actor, at time.Time) error {
	return o.applyTransition(StatusAccepted, actor, at)
}

func (o *Order) Start(actor Actor, at time.Time) error {
	return o.applyTransition(StatusInProgress, actor, at)
}

func (o *Order) Complete(actor Actor, at time.Time) error {
	return o.applyTransition(StatusCompleted, actor, at)
}

func (o *Order) Cancel(actor Actor, at time.Time) error {
	return o.applyTransition(StatusCancelled, actor, at)
}
