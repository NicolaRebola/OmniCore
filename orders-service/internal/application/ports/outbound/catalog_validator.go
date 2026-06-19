package outbound

import (
	"context"

	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type CatalogReferenceValidator interface {
	ValidateAndResolve(
		ctx context.Context,
		tenantID uuid.UUID,
		variantIDs []uuid.UUID,
	) ([]domain.VariantSnapshot, error)
}
