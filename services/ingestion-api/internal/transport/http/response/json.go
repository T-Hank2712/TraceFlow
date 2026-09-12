package response

import (
	"encoding/json"
	"net/http"
)

func Error(w http.ResponseWriter, statusCode int, message string) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)

	_ = json.NewEncoder(w).Encode(map[string]string{
		"error": message,
	})
}

func ValidationError(
	w http.ResponseWriter,
	statusCode int,
	message string,
	details any,
) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)

	_ = json.NewEncoder(w).Encode(map[string]any{
		"error":   message,
		"details": details,
	})
}
