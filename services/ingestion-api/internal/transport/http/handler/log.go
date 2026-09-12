package handler

import (
	"encoding/json"
	"errors"
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/auth"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http/response"
)

type LogHandler struct {
	ingestionService domain.IngestionService
}

func NewLogHandler(ingestionService domain.IngestionService) *LogHandler {
	return &LogHandler{
		ingestionService: ingestionService,
	}
}

func (h *LogHandler) Handle(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		response.Error(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	apiKey, err := auth.ExtractAPIKey(r)
	if err != nil {
		response.Error(w, http.StatusUnauthorized, "Unauthorized")
		return
	}

	var req domain.LogRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		response.Error(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	metadata, err := h.ingestionService.AcceptLog(r.Context(), apiKey, req)
	if err != nil {
		if errors.Is(err, domain.ErrInvalidAPIKey) {
			response.Error(w, http.StatusUnauthorized, "Unauthorized")
			return
		}

		if errors.Is(err, domain.ErrInvalidLogPayload) {
			response.Error(w, http.StatusBadRequest, "Invalid log payload")
			return
		}

		log.Printf("Failed to accept log: %v", err)
		response.Error(w, http.StatusInternalServerError, "Failed to publish log")
		return
	}

	log.Printf(
		"Received and published log: workspaceId=%s projectId=%s applicationId=%s environment=%s service=%s level=%s",
		metadata.WorkspaceID,
		metadata.ProjectID,
		metadata.ApplicationID,
		metadata.Environment,
		req.Service,
		req.Level,
	)

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusAccepted)

	_ = json.NewEncoder(w).Encode(map[string]string{
		"status": "Log accepted",
	})
}
