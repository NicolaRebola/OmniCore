package application

import "errors"

type AppError struct {
	Code       string
	Message    string
	HTTPStatus int
}

func (e AppError) Error() string { return e.Message }

var (
	ErrOrderNotFound           = AppError{Code: "ORD-APP-001", Message: "order not found", HTTPStatus: 404}
	ErrTenantRequired          = AppError{Code: "ORD-APP-002", Message: "tenant id is required", HTTPStatus: 400}
	ErrLineNotFound            = AppError{Code: "ORD-APP-003", Message: "order line not found", HTTPStatus: 404}
	ErrCatalogUnavailable      = AppError{Code: "ORD-APP-004", Message: "catalog service unavailable", HTTPStatus: 503}
	ErrIdempotencyKeyRequired  = AppError{Code: "ORD-APP-005", Message: "idempotency key is required", HTTPStatus: 400}
	ErrIdempotencyConflict     = AppError{Code: "ORD-APP-006", Message: "idempotency key reused with different payload", HTTPStatus: 409}
)

func HasAppCode(err error, code string) bool {
	var appErr AppError
	if errors.As(err, &appErr) {
		return appErr.Code == code
	}
	return false
}
