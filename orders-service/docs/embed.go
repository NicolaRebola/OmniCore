package docs

import _ "embed"

// OpenAPI is the embedded OpenAPI 3 contract (ORD-SPEC-010).
//
//go:embed openapi.yaml
var OpenAPI []byte
