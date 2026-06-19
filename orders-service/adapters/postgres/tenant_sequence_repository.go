package postgres

import (
	"context"
	"fmt"

	outboundports "orders-service/internal/application/ports/outbound/repositories/postgres"

	"github.com/google/uuid"
	"github.com/jackc/pgx/v5/pgxpool"
)

type TenantSequenceRepository struct {
	pool *pgxpool.Pool
}

func NewTenantSequenceRepository(pool *pgxpool.Pool) *TenantSequenceRepository {
	return &TenantSequenceRepository{pool: pool}
}

var _ outboundports.TenantSequenceRepository = (*TenantSequenceRepository)(nil)

func (r *TenantSequenceRepository) NextOrderNumber(ctx context.Context, tenantID uuid.UUID) (int64, error) {
	const sql = `
		INSERT INTO tenant_sequences (tenant_id, last_number)
		VALUES ($1, 1)
		ON CONFLICT (tenant_id) DO UPDATE
		SET last_number = tenant_sequences.last_number + 1
		RETURNING last_number
	`

	var next int64
	if err := r.pool.QueryRow(ctx, sql, tenantID).Scan(&next); err != nil {
		return 0, fmt.Errorf("next order number: %w", err)
	}
	return next, nil
}
