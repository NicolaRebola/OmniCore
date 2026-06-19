package http

import (
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
	"gopkg.in/yaml.v3"

	"orders-service/docs"
)

type SwaggerConfig struct {
	Enabled bool
}

func MountSwaggerIfEnabled(r chi.Router, cfg SwaggerConfig) {
	if !cfg.Enabled {
		return
	}
	mountSwagger(r)
}

func mountSwagger(r chi.Router) {
	r.Route("/swagger", func(r chi.Router) {
		r.Get("/", serveSwaggerUI)
		r.Get("/index.html", serveSwaggerUI)
		r.Get("/v1/openapi.yaml", serveOpenAPISpecYAML)
		r.Get("/v1/swagger.json", serveOpenAPISpecJSON)
	})
}

func serveOpenAPISpecYAML(w http.ResponseWriter, _ *http.Request) {
	w.Header().Set("Content-Type", "application/yaml")
	w.WriteHeader(http.StatusOK)
	_, _ = w.Write(docs.OpenAPI)
}

func serveOpenAPISpecJSON(w http.ResponseWriter, _ *http.Request) {
	var spec any
	if err := yaml.Unmarshal(docs.OpenAPI, &spec); err != nil {
		http.Error(w, "invalid openapi spec", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	_ = json.NewEncoder(w).Encode(spec)
}

func serveSwaggerUI(w http.ResponseWriter, _ *http.Request) {
	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	w.WriteHeader(http.StatusOK)
	_, _ = w.Write([]byte(swaggerUIHTML))
}

const swaggerUIHTML = `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Orders Service API</title>
  <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui.css">
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="https://unpkg.com/swagger-ui-dist@5.18.2/swagger-ui-bundle.js"></script>
  <script>
    SwaggerUIBundle({
      url: "/swagger/v1/openapi.yaml",
      dom_id: "#swagger-ui",
      deepLinking: true,
      persistAuthorization: true
    });
  </script>
</body>
</html>`
