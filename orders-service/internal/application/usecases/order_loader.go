package usecases

import (
	"context"

	"github.com/google/uuid"

	"orders-service/internal/application"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
	"orders-service/internal/domain"
)

type orderLoader struct {
	orders postgresports.OrderRepository
}

func (l *orderLoader) loadForMutation(
	ctx context.Context,
	tenantID, orderID uuid.UUID,
) (*domain.Order, error) {
	order, err := l.orders.GetByID(ctx, tenantID, orderID)
	if err != nil {
		return nil, err
	}
	if order == nil {
		return nil, application.ErrOrderNotFound
	}
	return order, nil
}

func (l *orderLoader) save(ctx context.Context, order *domain.Order) error {
	return l.orders.Save(ctx, order)
}

func orderHasLine(order *domain.Order, lineID uuid.UUID) bool {
	for _, line := range order.Lines() {
		if line.ID == lineID {
			return true
		}
	}
	return false
}
