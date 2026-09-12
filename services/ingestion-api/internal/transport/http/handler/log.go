package handler

import (
	"encoding/json"
	"errors"
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/application/ingestion"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
	transportauth "github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http/auth"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/transport/http/response"
)

type LogHandler struct {
	ingestionService domain.IngestionService
	policy           ingestion.Policy
}

func NewLogHandler(
	ingestionService domain.IngestionService,
	policy ingestion.Policy,
) *LogHandler {
	return &LogHandler{
		ingestionService: ingestionService,
		policy:           policy,
	}
}

func (h *LogHandler) Handle(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		response.Error(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	r.Body = http.MaxBytesReader(w, r.Body, h.policy.MaxRequestBodyBytes)

	apiKey, err := transportauth.ExtractAPIKey(r)
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

func (h *LogHandler) HandleBatch(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		response.Error(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	r.Body = http.MaxBytesReader(w, r.Body, h.policy.MaxRequestBodyBytes)

	apiKey, err := transportauth.ExtractAPIKey(r)
	if err != nil {
		response.Error(w, http.StatusUnauthorized, "Unauthorized")
		return
	}

	var req domain.BatchLogRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		response.Error(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	metadata, acceptedLogs, err := h.ingestionService.AcceptBatchLogs(
		r.Context(),
		apiKey,
		req,
	)

	if err != nil {
		if errors.Is(err, domain.ErrInvalidAPIKey) {
			response.Error(w, http.StatusUnauthorized, "Unauthorized")
			return
		}

		if errors.Is(err, domain.ErrInvalidLogPayload) {
			response.Error(w, http.StatusBadRequest, "Invalid log payload")
			return
		}

		log.Printf("Failed to accept batch logs: %v", err)
		response.Error(w, http.StatusInternalServerError, "Failed to publish logs")
		return
	}

	log.Printf(
		"Received and published batch logs: workspaceId=%s projectId=%s applicationId=%s environment=%s acceptedLogs=%d",
		metadata.WorkspaceID,
		metadata.ProjectID,
		metadata.ApplicationID,
		metadata.Environment,
		acceptedLogs,
	)

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusAccepted)

	_ = json.NewEncoder(w).Encode(domain.BatchLogResponse{
		Status:       "Batch logs accepted",
		AcceptedLogs: acceptedLogs,
	})
}
