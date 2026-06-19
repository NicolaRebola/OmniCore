package catalog

import (
	"math"
	"strings"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type catalogProjectionDTO struct {
	Categories []catalogProjectionCategoryDTO `json:"categories"`
}

type catalogProjectionCategoryDTO struct {
	Items []catalogProjectionItemDTO `json:"items"`
}

type catalogProjectionItemDTO struct {
	VariantID     uuid.UUID  `json:"variantId"`
	ItemID        uuid.UUID  `json:"itemId"`
	VariantName   string     `json:"variantName"`
	Status        string     `json:"status"`
	Price         *priceDTO  `json:"price"`
}

type priceDTO struct {
	Amount   float64 `json:"amount"`
	Currency string  `json:"currency"`
}

func (p catalogProjectionDTO) indexByVariantID() map[uuid.UUID]catalogProjectionItemDTO {
	index := make(map[uuid.UUID]catalogProjectionItemDTO)
	for _, category := range p.Categories {
		for _, item := range category.Items {
			index[item.VariantID] = item
		}
	}
	return index
}

func matchSnapshots(
	variantIDs []uuid.UUID,
	byVariant map[uuid.UUID]catalogProjectionItemDTO,
) ([]domain.VariantSnapshot, error) {
	snapshots := make([]domain.VariantSnapshot, 0, len(variantIDs))
	for _, variantID := range variantIDs {
		item, ok := byVariant[variantID]
		if !ok {
			return nil, domain.ErrInvalidVariant
		}

		snapshot, err := item.toVariantSnapshot()
		if err != nil {
			return nil, err
		}
		if !snapshot.IsValidForPlace() {
			return nil, domain.ErrInvalidVariant
		}
		snapshots = append(snapshots, snapshot)
	}
	return snapshots, nil
}

func (item catalogProjectionItemDTO) toVariantSnapshot() (domain.VariantSnapshot, error) {
	if item.Price == nil {
		return domain.VariantSnapshot{}, domain.ErrInvalidVariant
	}

	amountMinor := int64(math.Round(item.Price.Amount * 100))
	price, err := domain.NewMoney(amountMinor, item.Price.Currency)
	if err != nil {
		return domain.VariantSnapshot{}, domain.ErrInvalidVariant
	}

	return domain.VariantSnapshot{
		VariantID:     item.VariantID,
		CatalogItemID: item.ItemID,
		Name:          item.VariantName,
		UnitPrice:     price,
		IsActive:      strings.EqualFold(item.Status, "active"),
	}, nil
}
