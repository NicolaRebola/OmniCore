package domain

import "github.com/google/uuid"

type OrderLine struct {
	ID            uuid.UUID
	VariantID     uuid.UUID
	CatalogItemID *uuid.UUID
	Name          *string
	UnitPrice     *Money
	Quantity      int
	LineTotal     *Money
}

func NewDraftOrderLine(id, variantID uuid.UUID, quantity int) OrderLine {
	return OrderLine{
		ID:        id,
		VariantID: variantID,
		Quantity:  quantity,
	}
}
