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

## Catalog Templates

Catalog templates are global structures managed by Omnicore. They are not tenant-scoped in this MVP, so these endpoints do not require `X-Tenant-Id`.

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/v1/catalog-templates` | Lists all catalog templates. |
| `GET` | `/api/v1/catalog-templates/{id}` | Returns one catalog template by id. |

### List Catalog Templates

Example:

```bash
curl http://localhost:5080/api/v1/catalog-templates
```

Response:

```json
{
  "items": [
    {
      "id": "uuid",
      "name": "Restaurant Item",
      "description": "Template for menu-style products",
      "status": "active",
      "attributes": [
        {
          "id": "uuid",
          "key": "serving-size",
          "name": "Serving Size",
          "type": "select",
          "required": true,
          "defaultValue": "regular",
          "options": ["regular", "large"]
        }
      ]
    }
  ]
}
```

The MVP response includes global Omnicore-managed attribute definitions. Templates remain global; item and variant attribute values are tenant-scoped through their owning item/variant.

### Get Catalog Template

Example:

```bash
curl http://localhost:5080/api/v1/catalog-templates/bbbbbbbb-0000-0000-0000-000000000001
```

Response:

```json
{
  "id": "uuid",
  "name": "Restaurant Item",
  "description": "Template for menu-style products",
  "status": "active",
  "attributes": [
    {
      "id": "uuid",
      "key": "serving-size",
      "name": "Serving Size",
      "type": "select",
      "required": true,
      "defaultValue": "regular",
      "options": ["regular", "large"]
    }
  ]
}
```

Template errors:

| Scenario | Status | Error code |
|---|---:|---|
| Invalid template UUID | `400` | `CAT-API-003` |
| Template does not exist | `404` | `CAT-APP-010` |

The endpoints are intentionally global. Do not send `X-Tenant-Id`; tenant-owned templates are outside MVP 1.

## Catalog Items

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/v1/catalog-items` | Lists catalog items for the current tenant (paginated, filterable). |
| `GET` | `/api/v1/catalog-items/{id}` | Returns administrative detail for one item, including variants. |
| `POST` | `/api/v1/catalog-items` | Creates a catalog item and its default variant. |
| `PATCH` | `/api/v1/catalog-items/{itemId}` | Updates catalog item editable fields and category assignment. |
| `DELETE` | `/api/v1/catalog-items/{itemId}/category` | Removes category assignment from an item and its variants. |
| `POST` | `/api/v1/catalog-items/{itemId}/variants` | Adds a variant to an existing item. |
| `PATCH` | `/api/v1/catalog-items/{itemId}/variants/{variantId}` | Updates variant descriptive fields, status and price. |
| `DELETE` | `/api/v1/catalog-items/{itemId}/variants/{variantId}` | Deactivates a variant. |

### List Catalog Items

Query parameters (all required unless noted):

| Parameter | Required | Description |
|---|---|---|
| `page` | Yes | Page number, starting at `1`. |
| `pageSize` | Yes | Items per page (`1`–`100`). |
| `type` | No | Filter by item type (`simple`, `variable`). |
| `visibility` | No | Filter by visibility (`commercial`, `internal`). |
| `status` | No | Filter by status (`active`, `inactive`). |
| `categoryId` | No | Filter by the item's assigned category UUID. |

Response:

```json
{
  "items": [
    {
      "id": "uuid",
      "name": "Burger",
      "description": "Classic burger",
      "type": "simple",
      "visibility": "commercial",
      "status": "active",
      "templateId": "uuid",
      "tenantId": "uuid",
      "categoryId": "uuid",
      "attributes": [],
      "variants": []
    }
  ],
  "page": 1,
  "pageSize": 20,
  "total": 42
}
```

List errors:

| Scenario | Status | Error code |
|---|---:|---|
| Missing `page` or `pageSize` | `400` | `CAT-API-007` |
| Invalid `page` or `pageSize` (out of range) | `400` | `CAT-API-008` |
| Invalid filter enum (`type`, `visibility`, `status`) | `400` | Domain validation via Problem Details |
| Invalid `categoryId` UUID | `400` | `CAT-API-003` |

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
  "templateId": "bbbbbbbb-0000-0000-0000-000000000001",
  "categoryId": "aaaaaaaa-0000-0000-0000-000000000001",
  "attributes": [
    { "key": "spicy", "value": "false" }
  ]
}
```

`templateId` is required and must point to an active global template. `categoryId` is optional. When provided, the category must belong to the same tenant and be active.

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
  "templateId": "uuid",
  "categoryId": "uuid",
  "attributes": [
    { "key": "spicy", "value": "false" }
  ],
  "variants": [
    {
      "id": "uuid",
      "name": "Burger",
      "description": "Classic burger",
      "status": "active",
      "tenantId": "uuid",
      "categoryId": "uuid",
      "price": null,
      "attributes": []
    }
  ]
}
```

Creation errors:

| Scenario | Status | Error code |
|---|---:|---|
| Invalid item name | `400` | `CAT-DOM-001` |
| Missing or empty `templateId` | `400` | `CAT-DOM-016` |
| Template does not exist | `404` | `CAT-APP-010` |
| Template is inactive | `400` | `CAT-APP-011` |
| Attribute value references an unknown template key | `400` | `CAT-DOM-026` |
| Attribute value does not match template options | `400` | `CAT-DOM-025` |
| Required attribute cannot be resolved | `400` | `CAT-DOM-027` |
| Category does not exist for tenant | `404` | `CAT-APP-004` |
| Category exists but is inactive | `400` | `CAT-APP-007` |

### Update Catalog Item

Request:

```json
{
  "name": "Updated Burger",
  "description": "Updated description",
  "visibility": "commercial",
  "status": "active",
  "categoryId": "aaaaaaaa-0000-0000-0000-000000000001"
}
```

All fields are optional at the command level. Omitted fields keep their current value. `type` is immutable and is not part of the update command.

When `categoryId` is provided, the category must belong to the same tenant and be active. A valid category change is propagated to all variants in the item.

### Remove Catalog Item Category

```bash
curl -X DELETE http://localhost:5080/api/v1/catalog-items/{itemId}/category \
  -H "X-Tenant-Id: <tenant-id>"
```

Successful removal returns `204 No Content`. The item and all its variants keep existing, with `categoryId = null`.

Item update/category errors:

| Scenario | Status | Error code |
|---|---:|---|
| Item does not exist for tenant | `404` | `CAT-APP-001` |
| Invalid item name | `400` | `CAT-DOM-001` |
| Invalid visibility | `400` | `CAT-DOM-004` |
| Invalid status | `400` | `CAT-DOM-005` |
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
  },
  "attributes": [
    { "key": "serving-size", "value": "large" }
  ]
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
  },
  "attributes": [
    { "key": "serving-size", "value": "large" }
  ]
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
  },
  "attributes": [
    { "key": "serving-size", "value": "regular" }
  ]
}
```

All fields are optional at the command level. Omitted fields keep their current value.

Variant errors:

| Scenario | Status | Error code |
|---|---:|---|
| Item does not exist for tenant | `404` | `CAT-APP-001` |
| Variant does not exist in item | `404` | `CAT-APP-008` |
| Invalid variant status | `400` | `CAT-APP-009` |
| Deactivating the last active variant through `PATCH` or `DELETE` | `409` | `CAT-APP-002` |
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

## Swagger / OpenAPI

Swagger UI and the generated OpenAPI JSON are available only in Development:

```bash
curl http://localhost:5080/swagger/v1/swagger.json
```

Open the interactive UI at:

```text
http://localhost:5080/swagger
```
