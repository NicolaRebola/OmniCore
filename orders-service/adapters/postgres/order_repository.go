package postgres

import (
	"context"
	"errors"
	"fmt"

	outboundports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"

	"github.com/google/uuid"
	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
)

var ErrOrderNotFound = errors.New("order not found")

type OrderRepository struct {
	pool *pgxpool.Pool
}

func NewOrderRepository(pool *pgxpool.Pool) *OrderRepository {
	return &OrderRepository{pool: pool}
}

var _ outboundports.OrderRepository = (*OrderRepository)(nil)

func (r *OrderRepository) GetByID(ctx context.Context, tenantID, orderID uuid.UUID) (*domain.Order, error) {
	row, err := r.loadOrderRow(ctx, tenantID, orderID)
	if err != nil {
		return nil, err
	}
	if row == nil {
		return nil, nil
	}

	lineRows, err := r.loadLineRows(ctx, tenantID, orderID)
	if err != nil {
		return nil, err
	}

	transitionRows, err := r.loadTransitionRows(ctx, tenantID, orderID)
	if err != nil {
		return nil, err
	}

	return orderRowToDomain(*row, lineRows, transitionRows)
}

func (r *OrderRepository) Save(ctx context.Context, order *domain.Order) error {
	params, err := orderToPersistParams(order)
	if err != nil {
		return err
	}

	tx, err := r.pool.Begin(ctx)
	if err != nil {
		return fmt.Errorf("begin tx: %w", err)
	}
	defer tx.Rollback(ctx)

	const upsertOrderSQL = `
		INSERT INTO orders (
			id, tenant_id, order_number, source, fulfillment_type, status,
			customer_json, address_json, comments, totals_json,
			created_at, updated_at
		) VALUES (
			$1, $2, $3, $4, $5, $6,
			$7, $8, $9, $10,
			$11, $12
		)
		ON CONFLICT (id) DO UPDATE SET
			order_number = EXCLUDED.order_number,
			source = EXCLUDED.source,
			fulfillment_type = EXCLUDED.fulfillment_type,
			status = EXCLUDED.status,
			customer_json = EXCLUDED.customer_json,
			address_json = EXCLUDED.address_json,
			comments = EXCLUDED.comments,
			totals_json = EXCLUDED.totals_json,
			updated_at = EXCLUDED.updated_at
	`

	_, err = tx.Exec(ctx, upsertOrderSQL,
		params.ID,
		params.TenantID,
		params.OrderNumber,
		params.Source,
		params.FulfillmentType,
		params.Status,
		params.CustomerJSON,
		params.AddressJSON,
		params.Comments,
		params.TotalsJSON,
		params.CreatedAt,
		params.UpdatedAt,
	)
	if err != nil {
		return fmt.Errorf("upsert order: %w", err)
	}

	if _, err := tx.Exec(ctx,
		`DELETE FROM order_lines WHERE order_id = $1 AND tenant_id = $2`,
		order.ID, order.TenantID,
	); err != nil {
		return fmt.Errorf("delete order lines: %w", err)
	}

	const insertLineSQL = `
		INSERT INTO order_lines (
			id, order_id, tenant_id, variant_id, catalog_item_id,
			name, unit_price_json, quantity, line_total_json
		) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
	`

	for _, line := range order.Lines() {
		unitPriceJSON, err := optionalMoneyToJSON(line.UnitPrice)
		if err != nil {
			return fmt.Errorf("line %s unit price: %w", line.ID, err)
		}
		lineTotalJSON, err := optionalMoneyToJSON(line.LineTotal)
		if err != nil {
			return fmt.Errorf("line %s line total: %w", line.ID, err)
		}

		_, err = tx.Exec(ctx, insertLineSQL,
			line.ID,
			order.ID,
			order.TenantID,
			line.VariantID,
			line.CatalogItemID,
			line.Name,
			unitPriceJSON,
			line.Quantity,
			lineTotalJSON,
		)
		if err != nil {
			return fmt.Errorf("insert order line %s: %w", line.ID, err)
		}
	}

	// MVP: reemplazo completo del timeline (el dominio no tiene ID de transition).
	if _, err := tx.Exec(ctx,
		`DELETE FROM order_transitions WHERE order_id = $1 AND tenant_id = $2`,
		order.ID, order.TenantID,
	); err != nil {
		return fmt.Errorf("delete order transitions: %w", err)
	}

	const insertTransitionSQL = `
		INSERT INTO order_transitions (
			order_id, tenant_id, from_status, to_status,
			actor_type, actor_id, reason, occurred_at
		) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
	`

	for _, tr := range order.Transitions() {
		_, err := tx.Exec(ctx, insertTransitionSQL,
			order.ID,
			order.TenantID,
			string(tr.FromStatus),
			string(tr.ToStatus),
			string(tr.ActorType),
			tr.ActorID,
			tr.Reason,
			tr.OccurredAt,
		)
		if err != nil {
			return fmt.Errorf("insert order transition: %w", err)
		}
	}

	if err := tx.Commit(ctx); err != nil {
		return fmt.Errorf("commit tx: %w", err)
	}
	return nil
}

func (r *OrderRepository) loadOrderRow(ctx context.Context, tenantID, orderID uuid.UUID) (*orderRow, error) {
	const sql = `
		SELECT
			id, tenant_id, order_number, source, fulfillment_type, status,
			customer_json, address_json, comments, totals_json,
			created_at, updated_at
		FROM orders
		WHERE tenant_id = $1 AND id = $2
	`

	var row orderRow
	err := r.pool.QueryRow(ctx, sql, tenantID, orderID).Scan(
		&row.ID,
		&row.TenantID,
		&row.OrderNumber,
		&row.Source,
		&row.FulfillmentType,
		&row.Status,
		&row.CustomerJSON,
		&row.AddressJSON,
		&row.Comments,
		&row.TotalsJSON,
		&row.CreatedAt,
		&row.UpdatedAt,
	)
	if errors.Is(err, pgx.ErrNoRows) {
		return nil, nil
	}
	if err != nil {
		return nil, fmt.Errorf("select order: %w", err)
	}
	return &row, nil
}

func (r *OrderRepository) loadLineRows(ctx context.Context, tenantID, orderID uuid.UUID) ([]orderLineRow, error) {
	const sql = `
		SELECT
			id, order_id, tenant_id, variant_id, catalog_item_id,
			name, unit_price_json, quantity, line_total_json
		FROM order_lines
		WHERE tenant_id = $1 AND order_id = $2
		ORDER BY id
	`

	rows, err := r.pool.Query(ctx, sql, tenantID, orderID)
	if err != nil {
		return nil, fmt.Errorf("select order lines: %w", err)
	}
	defer rows.Close()

	var result []orderLineRow
	for rows.Next() {
		var row orderLineRow
		if err := rows.Scan(
			&row.ID,
			&row.OrderID,
			&row.TenantID,
			&row.VariantID,
			&row.CatalogItemID,
			&row.Name,
			&row.UnitPriceJSON,
			&row.Quantity,
			&row.LineTotalJSON,
		); err != nil {
			return nil, fmt.Errorf("scan order line: %w", err)
		}
		result = append(result, row)
	}
	if err := rows.Err(); err != nil {
		return nil, fmt.Errorf("iterate order lines: %w", err)
	}
	return result, nil
}

func (r *OrderRepository) loadTransitionRows(ctx context.Context, tenantID, orderID uuid.UUID) ([]orderTransitionRow, error) {
	const sql = `
		SELECT
			id, order_id, tenant_id, from_status, to_status,
			actor_type, actor_id, reason, occurred_at
		FROM order_transitions
		WHERE tenant_id = $1 AND order_id = $2
		ORDER BY occurred_at, id
	`

	rows, err := r.pool.Query(ctx, sql, tenantID, orderID)
	if err != nil {
		return nil, fmt.Errorf("select order transitions: %w", err)
	}
	defer rows.Close()

	var result []orderTransitionRow
	for rows.Next() {
		var row orderTransitionRow
		if err := rows.Scan(
			&row.ID,
			&row.OrderID,
			&row.TenantID,
			&row.FromStatus,
			&row.ToStatus,
			&row.ActorType,
			&row.ActorID,
			&row.Reason,
			&row.OccurredAt,
		); err != nil {
			return nil, fmt.Errorf("scan order transition: %w", err)
		}
		result = append(result, row)
	}
	if err := rows.Err(); err != nil {
		return nil, fmt.Errorf("iterate order transitions: %w", err)
	}
	return result, nil
}
