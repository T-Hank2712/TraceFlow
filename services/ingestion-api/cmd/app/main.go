package main

import (
	"log"
	"net/http"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/handler"
)

func main() {
	http.HandleFunc("/health", handler.HealthHandler)
	http.HandleFunc("/logs", handler.LogHandler)
	log.Println("Ingestion API listening on :8080")
	if err := http.ListenAndServe(":8080", nil); err != nil {
		log.Fatal(err)
	}
}
