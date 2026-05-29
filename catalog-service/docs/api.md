# Catalog Service API

This document records the current REST contract for `catalog-service`. All tenant-scoped endpoints require the `X-Tenant-Id` header with a non-empty UUID.

## Tenant Header

| Header | Required | Description |
|---|---|---|
| `X-Tenant-Id` | Yes | Tenant context used to isolate catalog items, variants, and categories. |

Tenant errors:

| Scenario | Status | Error code |
|---|---:|---|
| Missing or empty tenant header | `400` | `CAT-API-001` |
| Invalid tenant UUID | `400` | `CAT-API-002` |

## Catalog Items

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/v1/catalog-items` | Lists catalog items for the current tenant. |
| `GET` | `/api/v1/catalog-items/{id}` | Returns administrative detail for one item, including variants. |
| `POST` | `/api/v1/catalog-items` | Creates a catalog item and its default variant. |
| `POST` | `/api/v1/catalog-items/{itemId}/variants` | Adds a variant to an existing item. |
| `PATCH` | `/api/v1/catalog-items/{itemId}/variants/{variantId}` | Updates variant descriptive fields, status and price. |
| `DELETE` | `/api/v1/catalog-items/{itemId}/variants/{variantId}` | Deactivates a variant. |

### Create Catalog Item

Request:

```json
{
  "name": "Burger",
  "tenantId": "00000000-0000-0000-0000-000000000000",
  "description": "Classic burger",
  "type": "simple",
  "visibility": "commercial",
  "status": "active",
  "categoryId": "aaaaaaaa-0000-0000-0000-000000000001"
}
```

`categoryId` is optional. When provided, the category must belong to the same tenant and be active.

Response:

```json
{
  "id": "uuid",
  "name": "Burger",
  "description": "Classic burger",
  "type": "simple",
  "visibility": "commercial",
  "status": "active",
  "tenantId": "uuid",
  "categoryId": "uuid",
  "variants": [
    {
      "id": "uuid",
      "name": "Burger",
      "description": "Classic burger",
      "status": "active",
      "tenantId": "uuid",
      "categoryId": "uuid",
      "price": null
    }
  ]
}
```

Creation errors:

| Scenario | Status | Error code |
|---|---:|---|
| Invalid item name | `400` | `CAT-DOM-001` |
| Category does not exist for tenant | `404` | `CAT-APP-004` |
| Category exists but is inactive | `400` | `CAT-APP-007` |

### Add Catalog Variant

Request:

```json
{
  "name": "XL",
  "description": "Extra large",
  "status": "active",
  "price": {
    "amount": 12.5,
    "currency": "ARS"
  }
}
```

Response:

```json
{
  "id": "uuid",
  "name": "XL",
  "description": "Extra large",
  "status": "active",
  "tenantId": "uuid",
  "categoryId": "uuid",
  "price": {
    "amount": 12.5,
    "currency": "ARS"
  }
}
```

### Update Catalog Variant

Request:

```json
{
  "name": "Small",
  "description": "Small size",
  "status": "inactive",
  "price": {
    "amount": 9.99,
    "currency": "USD"
  }
}
```

All fields are optional at the command level. Omitted fields keep their current value.

Variant errors:

| Scenario | Status | Error code |
|---|---:|---|
| Item does not exist for tenant | `404` | `CAT-APP-001` |
| Variant does not exist in item | `404` | `CAT-APP-008` |
| Invalid variant status | `400` | `CAT-APP-009` |
| Deactivating the last active variant | `409` | `CAT-APP-002` |
| Invalid price | `400` | `CAT-DOM-013` |

## Categories

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/v1/categories` | Lists active categories for the current tenant. |
| `POST` | `/api/v1/categories` | Creates an active category for the current tenant. |
| `PATCH` | `/api/v1/categories/{id}` | Partially updates category name and/or status. |
| `DELETE` | `/api/v1/categories/{id}` | Semantically deactivates the category. |

### Create Category

Request:

```json
{
  "name": "Burgers"
}
```

Response:

```json
{
  "id": "uuid",
  "tenantId": "uuid",
  "name": "Burgers",
  "status": "active"
}
```

### Update Category

Request:

```json
{
  "name": "Sandwiches",
  "status": "active"
}
```

Both fields are optional at the command level. `DELETE /api/v1/categories/{id}` reuses this mutation path internally by applying `status = "inactive"`.

Category errors:

| Scenario | Status | Error code |
|---|---:|---|
| Invalid name | `400` | `CAT-DOM-009` |
| Category does not exist for tenant | `404` | `CAT-APP-004` |
| Invalid category status | `400` | `CAT-APP-006` |

## OpenAPI

The built-in OpenAPI JSON is available only in Development:

```bash
curl http://localhost:5080/openapi/v1.json
```
