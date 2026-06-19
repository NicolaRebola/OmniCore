package errors

import (
	"encoding/json"
	"errors"
	"net/http"
	"strings"

	chimw "github.com/go-chi/chi/v5/middleware"

	"orders-service/internal/application"
	"orders-service/internal/domain"
)

type Problem struct {
	Type      string `json:"type"`
	Title     string `json:"title"`
	Status    int    `json:"status"`
	Detail    string `json:"detail"`
	Instance  string `json:"instance,omitempty"`
	ErrorCode string `json:"errorCode"`
	Layer     string `json:"layer"`
	TraceID   string `json:"traceId,omitempty"`
}

var (
	ErrInvalidJSON = application.AppError{
		Code: "ORD-API-002", Message: "request body is invalid json", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidOrderID = application.AppError{
		Code: "ORD-API-003", Message: "order id must be a valid uuid", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidLineID = application.AppError{
		Code: "ORD-API-004", Message: "line id must be a valid uuid", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidVariantID = application.AppError{
		Code: "ORD-API-005", Message: "variant id must be a valid uuid", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidSource = application.AppError{
		Code: "ORD-API-006", Message: "source is invalid", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidFulfillmentType = application.AppError{
		Code: "ORD-API-007", Message: "fulfillment type is invalid", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidActorType = application.AppError{
		Code: "ORD-API-008", Message: "actor type is invalid", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidPagination = application.AppError{
		Code: "ORD-API-009", Message: "page must be >= 1 and pageSize must be between 1 and 100", HTTPStatus: http.StatusBadRequest,
	}
	ErrInvalidStatus = application.AppError{
		Code: "ORD-API-010", Message: "status is invalid", HTTPStatus: http.StatusBadRequest,
	}
)

func WriteError(w http.ResponseWriter, r *http.Request, err error) {
	problem := MapError(r, err)
	w.Header().Set("Content-Type", "application/problem+json")
	w.WriteHeader(problem.Status)
	_ = json.NewEncoder(w).Encode(problem)
}

func MapError(r *http.Request, err error) Problem {
	traceID := chimw.GetReqID(r.Context())

	var appErr application.AppError
	if errors.As(err, &appErr) {
		return problemFromApp(r.URL.Path, traceID, appErr)
	}

	var domainErr domain.DomainError
	if errors.As(err, &domainErr) {
		return problemFromDomain(r.URL.Path, traceID, domainErr)
	}

	return Problem{
		Type:      "https://docs.omnicore.local/problems/orders/api/unexpected",
		Title:     "Unexpected error",
		Status:    http.StatusInternalServerError,
		Detail:    err.Error(),
		Instance:  r.URL.Path,
		ErrorCode: "ORD-API-001",
		Layer:     "Api",
		TraceID:   traceID,
	}
}

func problemFromApp(path, traceID string, err application.AppError) Problem {
	return Problem{
		Type:      "https://docs.omnicore.local/problems/orders/application/" + lowerCode(err.Code),
		Title:     err.Message,
		Status:    err.HTTPStatus,
		Detail:    err.Message,
		Instance:  path,
		ErrorCode: err.Code,
		Layer:     "Application",
		TraceID:   traceID,
	}
}

func problemFromDomain(path, traceID string, err domain.DomainError) Problem {
	status := http.StatusBadRequest
	switch err.Code {
	case domain.ErrMutationNotInDraft.Code,
		domain.ErrInvalidTransition.Code,
		domain.ErrCancelReasonRequired.Code:
		status = http.StatusUnprocessableEntity
	}

	return Problem{
		Type:      "https://docs.omnicore.local/problems/orders/domain/" + lowerCode(err.Code),
		Title:     err.Message,
		Status:    status,
		Detail:    err.Message,
		Instance:  path,
		ErrorCode: err.Code,
		Layer:     "Domain",
		TraceID:   traceID,
	}
}

func lowerCode(code string) string {
	return strings.ToLower(code)
}
