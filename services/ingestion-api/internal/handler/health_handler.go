package handler

import (
	"encoding/json"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/producer"
)

func HealthHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	_ = json.NewEncoder(w).Encode(map[string]string{
		"status": "OK",
	})
}

func KafkaHealthHandler(producer *producer.KafkaProducer) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")

		if err := producer.HealthCheck(); err != nil {
			w.WriteHeader(http.StatusServiceUnavailable)

			_ = json.NewEncoder(w).Encode(map[string]string{
				"status": "UNAVAILABLE",
			})

			return
		}

		w.WriteHeader(http.StatusOK)

		_ = json.NewEncoder(w).Encode(map[string]string{
			"status": "Kafka OK",
		})
	}
}
