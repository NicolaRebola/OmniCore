package main

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"
	"time"

	"go.uber.org/fx"

	"github.com/go-chi/chi/v5"
)

func main() {
	fx.New(
		AppModule,
		fx.Invoke(RegisterHTTPServer),
	).Run()
}

func NewLogger(cfg Config) *slog.Logger {
	return slog.New(slog.NewJSONHandler(os.Stdout, &slog.HandlerOptions{
		Level: parseLogLevel(cfg.LogLevel),
	}))
}

func RegisterHTTPServer(
	lc fx.Lifecycle,
	cfg Config,
	logger *slog.Logger,
	router chi.Router,
) {
	srv := &http.Server{
		Addr:         ":" + cfg.Port,
		Handler:      router,
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 15 * time.Second,
		IdleTimeout:  60 * time.Second,
	}

	lc.Append(fx.Hook{
		OnStart: func(_ context.Context) error {
			go func() {
				logger.Info("server starting", slog.String("addr", srv.Addr))
				if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
					logger.Error("server failed", slog.Any("error", err))
					os.Exit(1)
				}
			}()
			return nil
		},
		OnStop: func(ctx context.Context) error {
			logger.Info("server shutting down")
			shutdownCtx, cancel := context.WithTimeout(ctx, time.Duration(cfg.ShutdownTimeout)*time.Second)
			defer cancel()
			return srv.Shutdown(shutdownCtx)
		},
	})
}
