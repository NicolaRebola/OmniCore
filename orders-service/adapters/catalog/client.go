package catalog

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"time"

	app "orders-service/internal/application"
	outbound "orders-service/internal/application/ports/outbound"
	"orders-service/internal/domain"

	"github.com/google/uuid"
)

type Client struct {
	baseURL    string
	httpClient *http.Client
}

func NewClient(baseURL string) *Client {
	return &Client{
		baseURL:    baseURL,
		httpClient: &http.Client{Timeout: 2 * time.Second},
	}
}

var _ outbound.CatalogReferenceValidator = (*Client)(nil)

func (c *Client) ValidateAndResolve(
	ctx context.Context,
	tenantID uuid.UUID,
	variantIDs []uuid.UUID,
) ([]domain.VariantSnapshot, error) {
	req, err := http.NewRequestWithContext(
		ctx,
		http.MethodGet,
		c.baseURL+"/api/v1/projections/catalog",
		nil,
	)
	if err != nil {
		return nil, fmt.Errorf("build catalog request: %w", err)
	}
	req.Header.Set("X-Tenant-Id", tenantID.String())

	resp, err := c.httpClient.Do(req)
	if err != nil {
		// timeout, connection refused, etc.
		return nil, app.ErrCatalogUnavailable
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, app.ErrCatalogUnavailable
	}

	var projection catalogProjectionDTO
	if err := json.NewDecoder(resp.Body).Decode(&projection); err != nil {
		return nil, app.ErrCatalogUnavailable
	}

	byVariant := projection.indexByVariantID()
	return matchSnapshots(variantIDs, byVariant)
}
