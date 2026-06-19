package middleware

import (
	"context"
	"net/http"

	"github.com/google/uuid"

	httperrors "orders-service/adapters/http/errors"
	"orders-service/internal/application"
)

type tenantContextKey struct{}

func TenantIDFromContext(ctx context.Context) (uuid.UUID, bool) {
	id, ok := ctx.Value(tenantContextKey{}).(uuid.UUID)
	return id, ok
}

func ContextWithTenantID(parent context.Context, tenantID uuid.UUID) context.Context {
	return context.WithValue(parent, tenantContextKey{}, tenantID)
}

func TenantRequired() func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			header := r.Header.Get("X-Tenant-Id")
			if header == "" {
				httperrors.WriteError(w, r, application.ErrTenantRequired)
				return
			}

			tenantID, err := uuid.Parse(header)
			if err != nil || tenantID == uuid.Nil {
				httperrors.WriteError(w, r, application.ErrTenantRequired)
				return
			}

			ctx := context.WithValue(r.Context(), tenantContextKey{}, tenantID)
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}
