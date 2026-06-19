//go:build integration

package postgres

import (
	"context"
	"database/sql"
	"os"
	"path/filepath"
	"testing"

	_ "github.com/jackc/pgx/v5/stdlib"
	"github.com/jackc/pgx/v5/pgxpool"
	"github.com/pressly/goose/v3"
)

var integrationPool *pgxpool.Pool

func TestMain(m *testing.M) {
	databaseURL := os.Getenv("DATABASE_URL")
	if databaseURL == "" {
		os.Exit(m.Run())
	}

	ctx := context.Background()
	pool, err := NewPool(ctx, databaseURL)
	if err != nil {
		panic(err)
	}
	integrationPool = pool

	if err := runMigrations(databaseURL); err != nil {
		pool.Close()
		panic(err)
	}

	code := m.Run()
	pool.Close()
	os.Exit(code)
}

func runMigrations(databaseURL string) error {
	db, err := sql.Open("pgx", databaseURL)
	if err != nil {
		return err
	}
	defer db.Close()

	if err := goose.SetDialect("postgres"); err != nil {
		return err
	}

	migrationsDir, err := filepath.Abs(filepath.Join("..", "..", "migrations"))
	if err != nil {
		return err
	}
	return goose.Up(db, migrationsDir)
}

func requireIntegrationDB(t *testing.T) *pgxpool.Pool {
	t.Helper()
	if os.Getenv("DATABASE_URL") == "" {
		t.Skip("DATABASE_URL not set")
	}
	if integrationPool == nil {
		t.Fatal("integration pool not initialized")
	}
	truncateIntegrationTables(t)
	return integrationPool
}

func truncateIntegrationTables(t *testing.T) {
	t.Helper()
	ctx := context.Background()
	_, err := integrationPool.Exec(ctx, `
		TRUNCATE TABLE
			idempotency_keys,
			order_transitions,
			order_lines,
			orders,
			tenant_sequences
	`)
	if err != nil {
		t.Fatalf("truncate tables: %v", err)
	}
}
