package idempotency

import (
	"context"

	"github.com/google/uuid"
)

type ReplayStore interface {
	GetReplay(ctx context.Context, tenantID uuid.UUID, key, operation, requestHash string) ([]byte, bool, error)
	SaveResponse(ctx context.Context, tenantID uuid.UUID, key, operation, requestHash string, responseBody []byte) error
}

var _ ReplayStore = (*Store)(nil)
