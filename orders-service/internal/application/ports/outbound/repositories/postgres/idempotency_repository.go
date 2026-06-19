package outbound

import (
	"context"

	"github.com/google/uuid"
)

type IdempotencyRecord struct {
	TenantID       uuid.UUID
	IdempotencyKey string
	Operation      string
	RequestHash    string
	ResponseBody   []byte
}

type IdempotencyRepository interface {
	Find(ctx context.Context, tenantID uuid.UUID, key, operation string) (*IdempotencyRecord, error)
	Save(ctx context.Context, record IdempotencyRecord) error
}
