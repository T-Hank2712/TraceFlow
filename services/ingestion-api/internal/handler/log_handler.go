package handler

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/auth"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/model"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/producer"
)

type LogHandler struct {
	producer  *producer.KafkaProducer
	validator *auth.ControlAPIValidator
}

func NewLogHandler(
	producer *producer.KafkaProducer,
	validator *auth.ControlAPIValidator,
) *LogHandler {
	return &LogHandler{
		producer:  producer,
		validator: validator,
	}
}

func (h *LogHandler) Handle(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		writeJSONError(w, http.StatusMethodNotAllowed, "Method not allowed")
		return
	}

	apiKey, err := auth.ExtractAPIKey(r)
	if err != nil {
		writeJSONError(w, http.StatusUnauthorized, "Unauthorized")
		return
	}

	validationResult, err := h.validator.Validate(apiKey)
	if err != nil {
		writeJSONError(w, http.StatusUnauthorized, "Unauthorized")
		return
	}

	var req model.LogRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		writeJSONError(w, http.StatusBadRequest, "Invalid request body")
		return
	}

	payload, err := json.Marshal(req)
	if err != nil {
		writeJSONError(w, http.StatusInternalServerError, "Failed to serialize log")
		return
	}

	if err := h.producer.Publish(payload); err != nil {
		log.Printf("Failed to publish log to Kafka: %v", err)
		writeJSONError(w, http.StatusInternalServerError, "Failed to publish log")
		return
	}

	log.Printf(
		"Received and published log: workspaceId=%s projectId=%s applicationId=%s environment=%s service=%s level=%s",
		validationResult.WorkspaceID,
		validationResult.ProjectID,
		validationResult.ApplicationID,
		validationResult.Environment,
		req.Service,
		req.Level,
	)

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusAccepted)

	_ = json.NewEncoder(w).Encode(map[string]string{
		"status": "Log accepted",
	})
}

func writeJSONError(w http.ResponseWriter, statusCode int, message string) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)

	_ = json.NewEncoder(w).Encode(map[string]string{
		"error": message,
	})
}
