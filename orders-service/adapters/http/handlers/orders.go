package handlers

import (
	"context"
	"encoding/json"
	"io"
	"net/http"

	"github.com/go-chi/chi/v5"
	"github.com/google/uuid"

	"orders-service/adapters/http/dto"
	"orders-service/adapters/http/errors"
	"orders-service/adapters/http/idempotency"
	httpmw "orders-service/adapters/http/middleware"
	"orders-service/internal/application"
	inboundports "orders-service/internal/application/ports/inbound"
	"orders-service/internal/domain"
)

type OrderHandlers struct {
	create            inboundports.CreateOrder
	createAndPlace    inboundports.CreateAndPlace
	addLine           inboundports.AddLine
	updateLineQty     inboundports.UpdateLineQuantity
	removeLine        inboundports.RemoveLine
	setCustomer       inboundports.SetCustomer
	setAddress        inboundports.SetAddress
	setFulfillment    inboundports.SetFulfillmentType
	setComments       inboundports.SetComments
	place             inboundports.PlaceOrder
	accept            inboundports.AcceptOrder
	start             inboundports.StartOrder
	complete          inboundports.CompleteOrder
	cancel            inboundports.CancelOrder
	getOrder          inboundports.GetOrder
	listOrders        inboundports.ListOrders
	idempotency       idempotency.ReplayStore
}

func NewOrderHandlers(
	create inboundports.CreateOrder,
	createAndPlace inboundports.CreateAndPlace,
	addLine inboundports.AddLine,
	updateLineQty inboundports.UpdateLineQuantity,
	removeLine inboundports.RemoveLine,
	setCustomer inboundports.SetCustomer,
	setAddress inboundports.SetAddress,
	setFulfillment inboundports.SetFulfillmentType,
	setComments inboundports.SetComments,
	place inboundports.PlaceOrder,
	accept inboundports.AcceptOrder,
	start inboundports.StartOrder,
	complete inboundports.CompleteOrder,
	cancel inboundports.CancelOrder,
	getOrder inboundports.GetOrder,
	listOrders inboundports.ListOrders,
	idempotencyStore idempotency.ReplayStore,
) *OrderHandlers {
	return &OrderHandlers{
		create:         create,
		createAndPlace: createAndPlace,
		addLine:        addLine,
		updateLineQty:  updateLineQty,
		removeLine:     removeLine,
		setCustomer:    setCustomer,
		setAddress:     setAddress,
		setFulfillment: setFulfillment,
		setComments:    setComments,
		place:          place,
		accept:         accept,
		start:          start,
		complete:       complete,
		cancel:         cancel,
		getOrder:       getOrder,
		listOrders:     listOrders,
		idempotency:    idempotencyStore,
	}
}

func (h *OrderHandlers) Create(w http.ResponseWriter, r *http.Request) {
	tenantID, ok := httpmw.TenantIDFromContext(r.Context())
	if !ok {
		errors.WriteError(w, r, application.ErrTenantRequired)
		return
	}

	idempotencyKey := r.Header.Get("Idempotency-Key")
	if idempotencyKey == "" {
		errors.WriteError(w, r, application.ErrIdempotencyKeyRequired)
		return
	}

	body, err := io.ReadAll(r.Body)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	requestHash := idempotency.HashRequestBody(body)
	if replay, found, err := h.idempotency.GetReplay(
		r.Context(), tenantID, idempotencyKey, idempotency.CreateOrderOperation, requestHash,
	); err != nil {
		errors.WriteError(w, r, err)
		return
	} else if found {
		writeJSON(w, http.StatusCreated, replay)
		return
	}

	var req dto.CreateOrderRequest
	if err := json.Unmarshal(body, &req); err != nil {
		errors.WriteError(w, r, errors.ErrInvalidJSON)
		return
	}

	source, err := dto.ParseOrderSource(req.Source)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	fulfillment, err := dto.ParseFulfillmentType(req.FulfillmentType)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	result, err := h.create.Execute(r.Context(), inboundports.CreateOrderCommand{
		TenantID:        tenantID,
		Source:          source,
		FulfillmentType: fulfillment,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	resp := dto.OrderFromDomain(result.Order)
	respBytes, err := idempotency.MarshalResponse(resp)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	if err := h.idempotency.SaveResponse(
		r.Context(), tenantID, idempotencyKey, idempotency.CreateOrderOperation, requestHash, respBytes,
	); err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeJSON(w, http.StatusCreated, respBytes)
}

func (h *OrderHandlers) CreateAndPlace(w http.ResponseWriter, r *http.Request) {
	tenantID, ok := httpmw.TenantIDFromContext(r.Context())
	if !ok {
		errors.WriteError(w, r, application.ErrTenantRequired)
		return
	}

	idempotencyKey := r.Header.Get("Idempotency-Key")
	if idempotencyKey == "" {
		errors.WriteError(w, r, application.ErrIdempotencyKeyRequired)
		return
	}

	body, err := io.ReadAll(r.Body)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	requestHash := idempotency.HashRequestBody(body)
	if replay, found, err := h.idempotency.GetReplay(
		r.Context(), tenantID, idempotencyKey, idempotency.CreateAndPlaceOperation, requestHash,
	); err != nil {
		errors.WriteError(w, r, err)
		return
	} else if found {
		writeJSON(w, http.StatusCreated, replay)
		return
	}

	var req dto.CreateAndPlaceRequest
	if err := json.Unmarshal(body, &req); err != nil {
		errors.WriteError(w, r, errors.ErrInvalidJSON)
		return
	}

	source, err := dto.ParseOrderSource(req.Source)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	fulfillment, err := dto.ParseFulfillmentType(req.FulfillmentType)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	actor, err := dto.ActorFromRequest(dto.ActorRequest{
		ActorType: req.ActorType,
		ActorID:   req.ActorID,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	if len(req.Lines) == 0 {
		errors.WriteError(w, r, domain.ErrPlaceWithoutLines)
		return
	}

	lines := make([]inboundports.CreateAndPlaceLine, len(req.Lines))
	for i, line := range req.Lines {
		if line.VariantID == uuid.Nil {
			errors.WriteError(w, r, errors.ErrInvalidVariantID)
			return
		}
		lines[i] = inboundports.CreateAndPlaceLine{
			VariantID: line.VariantID,
			Quantity:  line.Quantity,
		}
	}

	cmd := inboundports.CreateAndPlaceCommand{
		TenantID:        tenantID,
		Source:          source,
		FulfillmentType: fulfillment,
		Customer:        customerFromRequest(req.Customer),
		Comments:        req.Comments,
		Lines:           lines,
		Actor:           actor,
	}
	if req.Address != nil {
		addr := addressFromRequest(*req.Address)
		cmd.Address = &addr
	}

	result, err := h.createAndPlace.Execute(r.Context(), cmd)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	resp := dto.OrderFromDomain(result.Order)
	respBytes, err := idempotency.MarshalResponse(resp)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	if err := h.idempotency.SaveResponse(
		r.Context(), tenantID, idempotencyKey, idempotency.CreateAndPlaceOperation, requestHash, respBytes,
	); err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeJSON(w, http.StatusCreated, respBytes)
}

func (h *OrderHandlers) Place(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	idempotencyKey := r.Header.Get("Idempotency-Key")
	if idempotencyKey == "" {
		errors.WriteError(w, r, application.ErrIdempotencyKeyRequired)
		return
	}

	body, err := io.ReadAll(r.Body)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	requestHash := idempotency.HashScopedRequest(orderID.String(), body)
	if replay, found, err := h.idempotency.GetReplay(
		r.Context(), tenantID, idempotencyKey, idempotency.PlaceOrderOperation, requestHash,
	); err != nil {
		errors.WriteError(w, r, err)
		return
	} else if found {
		writeJSON(w, http.StatusOK, replay)
		return
	}

	var req dto.ActorRequest
	if err := json.Unmarshal(body, &req); err != nil {
		errors.WriteError(w, r, errors.ErrInvalidJSON)
		return
	}

	actor, err := dto.ActorFromRequest(req)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	result, err := h.place.Execute(r.Context(), inboundports.PlaceOrderCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		Actor:    actor,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	resp := dto.OrderFromDomain(result.Order)
	respBytes, err := idempotency.MarshalResponse(resp)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	if err := h.idempotency.SaveResponse(
		r.Context(), tenantID, idempotencyKey, idempotency.PlaceOrderOperation, requestHash, respBytes,
	); err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeJSON(w, http.StatusOK, respBytes)
}

func (h *OrderHandlers) AddLine(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	var req dto.AddLineRequest
	if !decodeJSON(w, r, &req) {
		return
	}
	if req.VariantID == uuid.Nil {
		errors.WriteError(w, r, errors.ErrInvalidVariantID)
		return
	}

	result, err := h.addLine.Execute(r.Context(), inboundports.AddLineCommand{
		TenantID:  tenantID,
		OrderID:   orderID,
		VariantID: req.VariantID,
		Quantity:  req.Quantity,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) UpdateLineQuantity(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	lineID, ok := h.parseLineID(w, r)
	if !ok {
		return
	}

	var req dto.UpdateLineQuantityRequest
	if !decodeJSON(w, r, &req) {
		return
	}

	result, err := h.updateLineQty.Execute(r.Context(), inboundports.UpdateLineQuantityCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		LineID:   lineID,
		Quantity: req.Quantity,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) RemoveLine(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	lineID, ok := h.parseLineID(w, r)
	if !ok {
		return
	}

	result, err := h.removeLine.Execute(r.Context(), inboundports.RemoveLineCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		LineID:   lineID,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) SetCustomer(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	var req dto.CustomerRequest
	if !decodeJSON(w, r, &req) {
		return
	}

	result, err := h.setCustomer.Execute(r.Context(), inboundports.SetCustomerCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		Customer: customerFromRequest(req),
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) SetAddress(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	var req dto.AddressRequest
	if !decodeJSON(w, r, &req) {
		return
	}

	result, err := h.setAddress.Execute(r.Context(), inboundports.SetAddressCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		Address:  addressFromRequest(req),
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) SetFulfillment(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	var req dto.SetFulfillmentRequest
	if !decodeJSON(w, r, &req) {
		return
	}

	fulfillment, err := dto.ParseFulfillmentType(req.FulfillmentType)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	result, err := h.setFulfillment.Execute(r.Context(), inboundports.SetFulfillmentTypeCommand{
		TenantID:        tenantID,
		OrderID:         orderID,
		FulfillmentType: fulfillment,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) SetComments(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	var req dto.SetCommentsRequest
	if !decodeJSON(w, r, &req) {
		return
	}

	result, err := h.setComments.Execute(r.Context(), inboundports.SetCommentsCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		Comments: req.Comments,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) Accept(w http.ResponseWriter, r *http.Request) {
	h.handleLifecycleTransition(w, r, h.accept.Execute)
}

func (h *OrderHandlers) Start(w http.ResponseWriter, r *http.Request) {
	h.handleLifecycleTransition(w, r, h.start.Execute)
}

func (h *OrderHandlers) Complete(w http.ResponseWriter, r *http.Request) {
	h.handleLifecycleTransition(w, r, h.complete.Execute)
}

func (h *OrderHandlers) Cancel(w http.ResponseWriter, r *http.Request) {
	h.handleLifecycleTransition(w, r, h.cancel.Execute)
}

func (h *OrderHandlers) GetByID(w http.ResponseWriter, r *http.Request) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	result, err := h.getOrder.Execute(r.Context(), inboundports.GetOrderQuery{
		TenantID: tenantID,
		OrderID:  orderID,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) List(w http.ResponseWriter, r *http.Request) {
	tenantID, ok := httpmw.TenantIDFromContext(r.Context())
	if !ok {
		errors.WriteError(w, r, application.ErrTenantRequired)
		return
	}

	pagination, err := dto.ParsePagination(r.URL.Query().Get("page"), r.URL.Query().Get("pageSize"))
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	query := inboundports.ListOrdersQuery{
		TenantID: tenantID,
		Page:     pagination.Page,
		PageSize: pagination.PageSize,
	}

	if statusParam := r.URL.Query().Get("status"); statusParam != "" {
		status, err := dto.ParseOrderStatus(statusParam)
		if err != nil {
			errors.WriteError(w, r, err)
			return
		}
		query.Status = &status
	}

	result, err := h.listOrders.Execute(r.Context(), query)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	resp := dto.ListOrdersFromDomain(result.Orders, pagination.Page, pagination.PageSize, result.Total)
	writeJSONValue(w, http.StatusOK, resp)
}

type lifecycleExecutor func(context.Context, inboundports.LifecycleTransitionCommand) (inboundports.OrderMutationResult, error)

func (h *OrderHandlers) handleLifecycleTransition(
	w http.ResponseWriter,
	r *http.Request,
	execute lifecycleExecutor,
) {
	tenantID, orderID, ok := h.tenantAndOrderID(w, r)
	if !ok {
		return
	}

	var req dto.ActorRequest
	if !decodeJSON(w, r, &req) {
		return
	}

	actor, err := dto.ActorFromRequest(req)
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	result, err := execute(r.Context(), inboundports.LifecycleTransitionCommand{
		TenantID: tenantID,
		OrderID:  orderID,
		Actor:    actor,
	})
	if err != nil {
		errors.WriteError(w, r, err)
		return
	}

	writeOrder(w, http.StatusOK, result.Order)
}

func (h *OrderHandlers) tenantAndOrderID(w http.ResponseWriter, r *http.Request) (uuid.UUID, uuid.UUID, bool) {
	tenantID, ok := httpmw.TenantIDFromContext(r.Context())
	if !ok {
		errors.WriteError(w, r, application.ErrTenantRequired)
		return uuid.Nil, uuid.Nil, false
	}

	orderID, err := uuid.Parse(chi.URLParam(r, "id"))
	if err != nil || orderID == uuid.Nil {
		errors.WriteError(w, r, errors.ErrInvalidOrderID)
		return uuid.Nil, uuid.Nil, false
	}

	return tenantID, orderID, true
}

func (h *OrderHandlers) parseLineID(w http.ResponseWriter, r *http.Request) (uuid.UUID, bool) {
	lineID, err := uuid.Parse(chi.URLParam(r, "lineId"))
	if err != nil || lineID == uuid.Nil {
		errors.WriteError(w, r, errors.ErrInvalidLineID)
		return uuid.Nil, false
	}
	return lineID, true
}

func decodeJSON(w http.ResponseWriter, r *http.Request, dst any) bool {
	if err := json.NewDecoder(r.Body).Decode(dst); err != nil {
		errors.WriteError(w, r, errors.ErrInvalidJSON)
		return false
	}
	return true
}

func writeOrder(w http.ResponseWriter, status int, order *domain.Order) {
	resp := dto.OrderFromDomain(order)
	writeJSONValue(w, status, resp)
}

func writeJSONValue(w http.ResponseWriter, status int, v any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(v)
}

func writeJSON(w http.ResponseWriter, status int, body []byte) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_, _ = w.Write(body)
}

func customerFromRequest(req dto.CustomerRequest) domain.CustomerSnapshot {
	return domain.CustomerSnapshot{
		Name:  req.Name,
		Email: req.Email,
		Phone: req.Phone,
	}
}

func addressFromRequest(req dto.AddressRequest) domain.Address {
	return domain.Address{
		Street:     req.Street,
		Number:     req.Number,
		Apartment:  req.Apartment,
		City:       req.City,
		References: req.References,
	}
}
