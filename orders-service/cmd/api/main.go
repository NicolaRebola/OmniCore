package main

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	httpadapter "orders-service/adapters/http"
	"orders-service/adapters/postgres"

	pgxpool "github.com/jackc/pgx/v5/pgxpool"
)

func main() {
	cfg := LoadConfig()
	logger := slog.New(slog.NewJSONHandler(os.Stdout, &slog.HandlerOptions{
		Level: parseLogLevel(cfg.LogLevel),
	}))

	pool := connectDB(cfg, logger)
	defer pool.Close()

	srv := newServer(cfg, logger, pool)

	go startServer(srv, logger)

	waitForShutdownSignal()

	logger.Info("server shutting down")

	shutdownCtx, cancel := context.WithTimeout(
		context.Background(),
		time.Duration(cfg.ShutdownTimeout)*time.Second,
	)
	defer cancel()

	if err := srv.Shutdown(shutdownCtx); err != nil {
		logger.Error("shutdown failed", slog.Any("error", err))
		os.Exit(1)
	}

	logger.Info("server stopped")
}

func connectDB(cfg Config, logger *slog.Logger) *pgxpool.Pool {
	ctx := context.Background()
	pool, err := postgres.NewPool(ctx, cfg.DatabaseURL)
	if err != nil {
		logger.Error("database connection failed", slog.Any("error", err))
		os.Exit(1)
	}
	return pool
}

func newServer(cfg Config, logger *slog.Logger, pool *pgxpool.Pool) *http.Server {
	return &http.Server{
		Addr:         ":" + cfg.Port,
		Handler:      httpadapter.NewRouter(logger, pool),
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 15 * time.Second,
		IdleTimeout:  60 * time.Second,
	}
}

func startServer(srv *http.Server, logger *slog.Logger) {
	logger.Info("server starting", slog.String("addr", srv.Addr))
	if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
		logger.Error("server failed", slog.Any("error", err))
		os.Exit(1)
	}
}

func waitForShutdownSignal() {
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit
}
