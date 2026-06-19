package idempotency

import (
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"errors"

	"github.com/google/uuid"

	"orders-service/adapters/postgres"
	"orders-service/internal/application"
	postgresports "orders-service/internal/application/ports/outbound/repositories/postgres"
)

const (
	CreateOrderOperation      = "create_order"
	PlaceOrderOperation       = "place_order"
	CreateAndPlaceOperation   = "create_and_place"
)

type Store struct {
	repo postgresports.IdempotencyRepository
}

func NewStore(repo postgresports.IdempotencyRepository) *Store {
	return &Store{repo: repo}
}

func HashRequestBody(body []byte) string {
	sum := sha256.Sum256(body)
	return hex.EncodeToString(sum[:])
}

func HashScopedRequest(scope string, body []byte) string {
	payload := append([]byte(scope+":"), body...)
	return HashRequestBody(payload)
}

func (s *Store) GetReplay(
	ctx context.Context,
	tenantID uuid.UUID,
	key, operation, requestHash string,
) ([]byte, bool, error) {
	record, err := s.repo.Find(ctx, tenantID, key, operation)
	if err != nil {
		return nil, false, err
	}
	if record == nil {
		return nil, false, nil
	}
	if record.RequestHash != requestHash {
		return nil, false, application.ErrIdempotencyConflict
	}
	return record.ResponseBody, true, nil
}

func (s *Store) SaveResponse(
	ctx context.Context,
	tenantID uuid.UUID,
	key, operation, requestHash string,
	responseBody []byte,
) error {
	err := s.repo.Save(ctx, postgresports.IdempotencyRecord{
		TenantID:       tenantID,
		IdempotencyKey: key,
		Operation:      operation,
		RequestHash:    requestHash,
		ResponseBody:   responseBody,
	})
	if errors.Is(err, postgres.ErrIdempotencyKeyExists) {
		record, findErr := s.repo.Find(ctx, tenantID, key, operation)
		if findErr != nil {
			return findErr
		}
		if record == nil {
			return err
		}
		if record.RequestHash != requestHash {
			return application.ErrIdempotencyConflict
		}
		return nil
	}
	return err
}

func MarshalResponse(v any) ([]byte, error) {
	return json.Marshal(v)
}
