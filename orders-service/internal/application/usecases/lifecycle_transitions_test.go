package usecases_test

import (
	"context"
	"testing"

	inboundports "orders-service/internal/application/ports/inbound"
	"orders-service/internal/application/usecases"
	"orders-service/internal/domain"
)

func TestLifecycleTransitions_HappyPath(t *testing.T) {
	order := placedOrder(t)
	repo := &fakeOrderRepo{order: order}

	accept := usecases.NewAcceptOrderHandler(repo, fakeClock{})
	start := usecases.NewStartOrderHandler(repo, fakeClock{})
	complete := usecases.NewCompleteOrderHandler(repo, fakeClock{})

	actor := domain.Actor{Type: domain.ActorStaff, ID: "staff-1"}
	cmd := inboundports.LifecycleTransitionCommand{
		TenantID: testTenantID,
		OrderID:  testOrderID,
		Actor:    actor,
	}

	result, err := accept.Execute(context.Background(), cmd)
	if err != nil {
		t.Fatalf("Accept() error = %v", err)
	}
	if result.Order.Status != domain.StatusAccepted {
		t.Fatalf("after Accept status = %s", result.Order.Status)
	}

	result, err = start.Execute(context.Background(), cmd)
	if err != nil {
		t.Fatalf("Start() error = %v", err)
	}
	if result.Order.Status != domain.StatusInProgress {
		t.Fatalf("after Start status = %s", result.Order.Status)
	}

	result, err = complete.Execute(context.Background(), cmd)
	if err != nil {
		t.Fatalf("Complete() error = %v", err)
	}
	if result.Order.Status != domain.StatusCompleted {
		t.Fatalf("after Complete status = %s", result.Order.Status)
	}
	if !repo.saveCalled {
		t.Fatal("expected Save to be called")
	}
}

func TestAcceptOrderHandler_FromDraft_ReturnsORDDOM001(t *testing.T) {
	order := draftOrder(t)
	repo := &fakeOrderRepo{order: order}
	handler := usecases.NewAcceptOrderHandler(repo, fakeClock{})

	_, err := handler.Execute(context.Background(), inboundports.LifecycleTransitionCommand{
		TenantID: testTenantID,
		OrderID:  testOrderID,
		Actor:    domain.Actor{Type: domain.ActorStaff},
	})
	if !domain.HasDomainCode(err, domain.ErrInvalidTransition.Code) {
		t.Fatalf("error = %v", err)
	}
	if repo.saveCalled {
		t.Fatal("expected Save not to be called")
	}
}

func TestCancelOrderHandler_FromPlaced_RequiresReason(t *testing.T) {
	order := placedOrder(t)
	repo := &fakeOrderRepo{order: order}
	handler := usecases.NewCancelOrderHandler(repo, fakeClock{})

	_, err := handler.Execute(context.Background(), inboundports.LifecycleTransitionCommand{
		TenantID: testTenantID,
		OrderID:  testOrderID,
		Actor:    domain.Actor{Type: domain.ActorStaff},
	})
	if !domain.HasDomainCode(err, domain.ErrCancelReasonRequired.Code) {
		t.Fatalf("error = %v", err)
	}
	if repo.saveCalled {
		t.Fatal("expected Save not to be called")
	}
}

func TestCancelOrderHandler_FromPlaced_WithReason(t *testing.T) {
	order := placedOrder(t)
	repo := &fakeOrderRepo{order: order}
	handler := usecases.NewCancelOrderHandler(repo, fakeClock{})

	result, err := handler.Execute(context.Background(), inboundports.LifecycleTransitionCommand{
		TenantID: testTenantID,
		OrderID:  testOrderID,
		Actor:    domain.Actor{Type: domain.ActorStaff, Reason: "customer request"},
	})
	if err != nil {
		t.Fatalf("Cancel() error = %v", err)
	}
	if result.Order.Status != domain.StatusCancelled {
		t.Fatalf("status = %s", result.Order.Status)
	}
	if !repo.saveCalled {
		t.Fatal("expected Save to be called")
	}
}
