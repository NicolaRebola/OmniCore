package middleware

import (
    "context"
    "net/http"
)

type tenantContextKey struct{}

func TenantIDFromContext(ctx context.Context) (string, bool) {
    id, ok := ctx.Value(tenantContextKey{}).(string)
    return id, ok
}

func TenantPlaceholder() func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
				tenantID := r.Header.Get("X-Tenant-Id")
				ctx := context.WithValue(r.Context(), tenantContextKey{}, tenantID)
				next.ServeHTTP(w, r.WithContext(ctx))
		})
}
}