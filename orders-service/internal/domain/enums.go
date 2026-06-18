package domain

type OrderStatus string

const (
	StatusDraft      OrderStatus = "Draft"
	StatusPlaced     OrderStatus = "Placed"
	StatusAccepted   OrderStatus = "Accepted"
	StatusInProgress OrderStatus = "InProgress"
	StatusCompleted  OrderStatus = "Completed"
	StatusCancelled  OrderStatus = "Cancelled"
)

type OrderSource string

const (
	SourcePOS        OrderSource = "POS"
	SourceQRMenu     OrderSource = "QR_MENU"
	SourceWeb        OrderSource = "WEB"
	SourceBackoffice OrderSource = "BACKOFFICE"
)

type FulfillmentType string

const (
	FulfillmentDineIn   FulfillmentType = "DINE_IN"
	FulfillmentTakeaway FulfillmentType = "TAKEAWAY"
	FulfillmentDelivery FulfillmentType = "DELIVERY"
)

type ActorType string

const (
	ActorBuyer ActorType = "buyer"
	ActorStaff ActorType = "staff"
	ActorAdmin ActorType = "admin"
)
