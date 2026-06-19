package application

import "errors"

type AppError struct {
	Code       string
	Message    string
	HTTPStatus int
}

func (e AppError) Error() string { return e.Message }

var (
	ErrOrderNotFound      = AppError{Code: "ORD-APP-001", Message: "order not found", HTTPStatus: 404}
	ErrCatalogUnavailable = AppError{Code: "ORD-APP-004", Message: "catalog service unavailable", HTTPStatus: 503}
)

func HasAppCode(err error, code string) bool {
	var appErr AppError
	if errors.As(err, &appErr) {
		return appErr.Code == code
	}
	return false
}
