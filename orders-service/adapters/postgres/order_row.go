package postgres

import (
	"time"

	"github.com/google/uuid"
)

type orderRow struct {
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

type orderLineRow struct {
	ID            uuid.UUID
	OrderID       uuid.UUID
	TenantID      uuid.UUID
	VariantID     uuid.UUID
	CatalogItemID *uuid.UUID
	Name          *string
	UnitPriceJSON []byte
	Quantity      int
	LineTotalJSON []byte
}

type orderTransitionRow struct {
	ID         uuid.UUID
	OrderID    uuid.UUID
	TenantID   uuid.UUID
	FromStatus string
	ToStatus   string
	ActorType  string
	ActorID    string
	Reason     string
	OccurredAt time.Time
}
