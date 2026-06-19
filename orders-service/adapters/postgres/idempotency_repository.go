package postgres

import (
	"context"
	"errors"
	"fmt"

	outboundports "orders-service/internal/application/ports/outbound/repositories/postgres"

	"github.com/google/uuid"
	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgconn"
	"github.com/jackc/pgx/v5/pgxpool"
)

var ErrIdempotencyKeyExists = errors.New("idempotency key already exists")

type IdempotencyRepository struct {
	pool *pgxpool.Pool
}

func NewIdempotencyRepository(pool *pgxpool.Pool) *IdempotencyRepository {
	return &IdempotencyRepository{pool: pool}
}

var _ outboundports.IdempotencyRepository = (*IdempotencyRepository)(nil)

func (r *IdempotencyRepository) Find(
	ctx context.Context,
	tenantID uuid.UUID,
	key, operation string,
) (*outboundports.IdempotencyRecord, error) {
	const sql = `
		SELECT tenant_id, idempotency_key, operation, request_hash, response_body
		FROM idempotency_keys
		WHERE tenant_id = $1 AND idempotency_key = $2 AND operation = $3
	`

	var record outboundports.IdempotencyRecord
	err := r.pool.QueryRow(ctx, sql, tenantID, key, operation).Scan(
		&record.TenantID,
		&record.IdempotencyKey,
		&record.Operation,
		&record.RequestHash,
		&record.ResponseBody,
	)
	if errors.Is(err, pgx.ErrNoRows) {
		return nil, nil
	}
	if err != nil {
		return nil, fmt.Errorf("find idempotency key: %w", err)
	}
	return &record, nil
}

func (r *IdempotencyRepository) Save(ctx context.Context, record outboundports.IdempotencyRecord) error {
	const sql = `
		INSERT INTO idempotency_keys (
			tenant_id, idempotency_key, operation, request_hash, response_body
		) VALUES ($1, $2, $3, $4, $5)
	`

	_, err := r.pool.Exec(ctx, sql,
		record.TenantID,
		record.IdempotencyKey,
		record.Operation,
		record.RequestHash,
		record.ResponseBody,
	)
	if err == nil {
		return nil
	}

	var pgErr *pgconn.PgError
	if errors.As(err, &pgErr) && pgErr.Code == "23505" {
		return ErrIdempotencyKeyExists
	}
	return fmt.Errorf("save idempotency key: %w", err)
}
