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
)

func main() {
    cfg := LoadConfig()
    logger := slog.New(slog.NewJSONHandler(os.Stdout, &slog.HandlerOptions{
			Level: parseLogLevel(cfg.LogLevel),
	}))

    srv := &http.Server{
        Addr:         ":" + cfg.Port,
        Handler:      httpadapter.NewRouter(logger),
        ReadTimeout:  15 * time.Second,
        WriteTimeout: 15 * time.Second,
        IdleTimeout:  60 * time.Second,
    }

    go func() {
        logger.Info("server starting", slog.String("addr", srv.Addr))
        if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
            logger.Error("server failed", slog.Any("error", err))
            os.Exit(1)
        }
    }()

    quit := make(chan os.Signal, 1)
    signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
    <-quit

    logger.Info("server shutting down")

    ctx, cancel := context.WithTimeout(
        context.Background(),
        time.Duration(cfg.ShutdownTimeout)*time.Second,
    )
    defer cancel()

    if err := srv.Shutdown(ctx); err != nil {
        logger.Error("shutdown failed", slog.Any("error", err))
        os.Exit(1)
    }

    logger.Info("server stopped")
}