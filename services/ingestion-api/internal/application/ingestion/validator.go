package ingestion

import (
	"strings"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
)

func validateLogRequest(
	request domain.LogRequest,
	policy Policy,
) error {
	details := collectLogValidationErrors(
		request,
		nil,
		policy,
	)

	if len(details) > 0 {
		return domain.ErrInvalidLogPayload
	}

	return nil
}

func validateBatchLogRequest(
	request domain.BatchLogRequest,
	policy Policy,
) error {
	var details []domain.ValidationErrorDetail

	if len(request.Logs) == 0 {
		details = append(details, domain.ValidationErrorDetail{
			Field:   "logs",
			Message: "Logs must contain at least one item.",
		})
	}

	if len(request.Logs) > policy.MaxBatchSize {
		details = append(details, domain.ValidationErrorDetail{
			Field:   "logs",
			Message: "Logs exceed the maximum batch size.",
		})
	}

	for index, logRequest := range request.Logs {
		details = append(
			details,
			collectLogValidationErrors(
				logRequest,
				intPointer(index),
				policy,
			)...,
		)
	}

	if len(details) > 0 {
		return domain.BatchValidationError{
			Details: details,
		}
	}

	return nil
}

func collectLogValidationErrors(
	request domain.LogRequest,
	index *int,
	policy Policy,
) []domain.ValidationErrorDetail {
	var errors []domain.ValidationErrorDetail

	if strings.TrimSpace(request.Service) == "" {
		errors = append(errors, domain.ValidationErrorDetail{
			Index:   index,
			Field:   "service",
			Message: "Service is required.",
		})
	}

	if strings.TrimSpace(request.Level) == "" {
		errors = append(errors, domain.ValidationErrorDetail{
			Index:   index,
			Field:   "level",
			Message: "Level is required.",
		})
	} else {
		level := strings.ToUpper(strings.TrimSpace(request.Level))
		if _, ok := policy.AllowedLevels[level]; !ok {
			errors = append(errors, domain.ValidationErrorDetail{
				Index:   index,
				Field:   "level",
				Message: "Level must be one of DEBUG, INFO, WARN, ERROR, FATAL.",
			})
		}
	}

	if strings.TrimSpace(request.Message) == "" {
		errors = append(errors, domain.ValidationErrorDetail{
			Index:   index,
			Field:   "message",
			Message: "Message is required.",
		})
	}

	return errors
}

func intPointer(value int) *int {
	return &value
}
