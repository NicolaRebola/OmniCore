package domain

import "errors"

type DomainError struct {
	Code    string
	Message string
}

func (e DomainError) Error() string {
	return e.Message
}

var (
	ErrInvalidTransition    = DomainError{Code: "ORD-DOM-001", Message: "invalid order status transition"}
	ErrMutationNotInDraft   = DomainError{Code: "ORD-DOM-002", Message: "order content can only be modified in Draft status"}
	ErrInvalidQuantity      = DomainError{Code: "ORD-DOM-003", Message: "quantity must be greater than zero"}
	ErrPlaceWithoutLines    = DomainError{Code: "ORD-DOM-004", Message: "place requires at least one order line"}
	ErrPlaceWithoutCustomer = DomainError{Code: "ORD-DOM-005", Message: "place requires customer name"}
	ErrDeliveryWithoutAddr  = DomainError{Code: "ORD-DOM-006", Message: "delivery fulfillment requires a valid address"}
	ErrCancelReasonRequired = DomainError{Code: "ORD-DOM-007", Message: "cancel reason is required outside Draft status"}
	ErrInvalidVariant       = DomainError{Code: "ORD-DOM-008", Message: "variant is invalid, inactive, or missing price at place"}
)

func HasDomainCode(err error, code string) bool {
	var domainErr DomainError
	if errors.As(err, &domainErr) {
		return domainErr.Code == code
	}
	return false
}
