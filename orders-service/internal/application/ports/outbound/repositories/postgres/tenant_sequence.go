package outbound

import (
	"context"

	"github.com/google/uuid"
)

type TenantSequenceRepository interface {
	NextOrderNumber(ctx context.Context, tenantID uuid.UUID) (int64, error)
}
