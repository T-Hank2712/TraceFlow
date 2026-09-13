package ingestion

import (
	"crypto/rand"
	"strings"
	"time"

	"github.com/oklog/ulid/v2"

	"github.com/T-Hank2712/traceflow/ingestion-api/internal/domain"
)

func newLogEvent(
	metadata *domain.APIKeyMetadata,
	request domain.LogRequest,
) domain.LogEvent {
	receivedAt := time.Now().UTC().Format(time.RFC3339Nano)

	timestamp := strings.TrimSpace(request.Timestamp)
	if timestamp == "" {
		timestamp = receivedAt
	}

	eventID := ulid.MustNew(ulid.Timestamp(time.Now()), rand.Reader).String()

	return domain.LogEvent{
		EventID:       eventID,
		Timestamp:     timestamp,
		ReceivedAt:    receivedAt,
		WorkspaceID:   metadata.WorkspaceID,
		ProjectID:     metadata.ProjectID,
		ApplicationID: metadata.ApplicationID,
		Environment:   metadata.Environment,
		Service:       strings.TrimSpace(request.Service),
		Level:         strings.ToUpper(strings.TrimSpace(request.Level)),
		Message:       strings.TrimSpace(request.Message),
		TraceID:       strings.TrimSpace(request.TraceID),
		CorrelationID: strings.TrimSpace(request.CorrelationID),
		Metadata:      request.Metadata,
	}
}
