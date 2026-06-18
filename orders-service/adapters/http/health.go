package http

import (
    "encoding/json"
    "net/http"
)

type healthResponse struct {
    Status string `json:"status"`
}

type readyResponse struct {
    Status string `json:"status"`
    // DB check en ORD-SPEC-004
}

func HealthHandler(w http.ResponseWriter, r *http.Request) {
    writeJSON(w, http.StatusOK, healthResponse{Status: "ok"})
}

func ReadyHandler(w http.ResponseWriter, r *http.Request) {
    // Stub: siempre ready hasta ORD-SPEC-004
    writeJSON(w, http.StatusOK, readyResponse{Status: "ready"})
}

func writeJSON(w http.ResponseWriter, status int, body any) {
    w.Header().Set("Content-Type", "application/json")
    w.WriteHeader(status)
    _ = json.NewEncoder(w).Encode(body)
}