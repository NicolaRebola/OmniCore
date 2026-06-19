package domain

import "time"

type Actor struct {
	Type   ActorType
	ID     string
	Reason string
}

type OrderTransition struct {
	FromStatus OrderStatus
	ToStatus   OrderStatus
	OccurredAt time.Time
	ActorType  ActorType
	ActorID    string
	Reason     string
}

func NewOrderTransition(from, to OrderStatus, actor Actor, occurredAt time.Time) OrderTransition {
	return OrderTransition{
		FromStatus: from,
		ToStatus:   to,
		OccurredAt: occurredAt,
		ActorType:  actor.Type,
		ActorID:    actor.ID,
		Reason:     actor.Reason,
	}
}
