package main

import (
    "os"
    "strconv"
		"log/slog"
)

type Config struct {
    Port            string
    DatabaseURL     string
    CatalogBaseURL  string
    LogLevel        string
    ShutdownTimeout int // segundos, ej. 10
}

func LoadConfig() Config {
    return Config{
        Port:            envOr("PORT", "8081"),
        DatabaseURL:     os.Getenv("DATABASE_URL"),
        CatalogBaseURL:  envOr("CATALOG_BASE_URL", "http://localhost:5080"),
        LogLevel:        envOr("LOG_LEVEL", "info"),
        ShutdownTimeout: envIntOr("SHUTDOWN_TIMEOUT_SEC", 10),
    }
}

func envOr(key, fallback string) string {
    if v := os.Getenv(key); v != "" {
        return v
    }
    return fallback
}

func envIntOr(key string, fallback int) int {
    if v := os.Getenv(key); v != "" {
        if n, err := strconv.Atoi(v); err == nil {
            return n
        }
    }
    return fallback
}

func parseLogLevel(level string) slog.Level {
	switch level {
	case "debug":
			return slog.LevelDebug
	case "warn":
			return slog.LevelWarn
	case "error":
			return slog.LevelError
	default:
			return slog.LevelInfo
	}
}