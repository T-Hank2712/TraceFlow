package handler

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/model"
	"github.com/T-Hank2712/traceflow/ingestion-api/internal/producer"
)

type LogHandler struct {
	producer *producer.KafkaProducer
}

func NewLogHandler(producer *producer.KafkaProducer) *LogHandler {
	return &LogHandler{
		producer: producer,
	}
}

func (h *LogHandler) Handle(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	var req model.LogRequest

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	payload, err := json.Marshal(req)
	if err != nil {
		http.Error(w, "Failed to serialize log", http.StatusInternalServerError)
		return
	}

	if err := h.producer.Publish(payload); err != nil {
		log.Printf("Failed to publish log to Kafka: %v", err)
		http.Error(w, "Failed to publish log", http.StatusInternalServerError)
		return
	}

	log.Printf(
		"Received and published log: service=%s level=%s message=%s",
		req.Service,
		req.Level,
		req.Message,
	)

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusAccepted)

	_ = json.NewEncoder(w).Encode(map[string]string{
		"status": "Log accepted",
	})
}
