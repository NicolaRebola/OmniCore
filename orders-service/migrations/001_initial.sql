-- +goose Up
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE orders (
    id                UUID PRIMARY KEY,
    tenant_id         UUID NOT NULL,
    order_number      BIGINT,
    source            TEXT NOT NULL,
    fulfillment_type  TEXT NOT NULL,
    status            TEXT NOT NULL,
    customer_json     JSONB,
    address_json      JSONB,
    comments          TEXT NOT NULL DEFAULT '',
    totals_json       JSONB,
    external_reference TEXT,
    created_at        TIMESTAMPTZ NOT NULL,
    updated_at        TIMESTAMPTZ NOT NULL
);

CREATE TABLE order_lines (
    id              UUID PRIMARY KEY,
    order_id        UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    tenant_id       UUID NOT NULL,
    variant_id      UUID NOT NULL,
    catalog_item_id UUID,
    name            TEXT,
    unit_price_json JSONB,
    quantity        INT NOT NULL CHECK (quantity > 0),
    line_total_json JSONB
);

CREATE TABLE order_transitions (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id    UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    tenant_id   UUID NOT NULL,
    from_status TEXT NOT NULL,
    to_status   TEXT NOT NULL,
    actor_type  TEXT NOT NULL,
    actor_id    TEXT NOT NULL DEFAULT '',
    reason      TEXT NOT NULL DEFAULT '',
    occurred_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE tenant_sequences (
    tenant_id   UUID PRIMARY KEY,
    last_number BIGINT NOT NULL DEFAULT 0
);

CREATE TABLE idempotency_keys (
    tenant_id       UUID NOT NULL,
    idempotency_key TEXT NOT NULL,
    operation       TEXT NOT NULL,
    request_hash    TEXT NOT NULL,
    response_body   JSONB NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (tenant_id, idempotency_key, operation)
);

-- Índices (criterios de aceptación)
CREATE INDEX idx_orders_tenant_created_at ON orders (tenant_id, created_at DESC);
CREATE INDEX idx_orders_tenant_status ON orders (tenant_id, status);
CREATE UNIQUE INDEX idx_orders_tenant_order_number
    ON orders (tenant_id, order_number)
    WHERE order_number IS NOT NULL;

CREATE INDEX idx_order_lines_order_id ON order_lines (order_id);
CREATE INDEX idx_order_lines_tenant_id ON order_lines (tenant_id);
CREATE INDEX idx_order_transitions_order_id ON order_transitions (order_id);

-- +goose Down
DROP TABLE IF EXISTS idempotency_keys;
DROP TABLE IF EXISTS tenant_sequences;
DROP TABLE IF EXISTS order_transitions;
DROP TABLE IF EXISTS order_lines;
DROP TABLE IF EXISTS orders;