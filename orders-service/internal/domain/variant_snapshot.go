package domain

import "github.com/google/uuid"

type VariantSnapshot struct {
	VariantID     uuid.UUID
	CatalogItemID uuid.UUID
	Name          string
	UnitPrice     Money
	IsActive      bool
}

func (v VariantSnapshot) IsValidForPlace() bool {
	return v.IsActive && v.Name != "" && !v.UnitPrice.IsZero()
}
