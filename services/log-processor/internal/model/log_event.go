package model

type LogEvent struct {
	EventID       string         `json:"eventId"`
	Timestamp     string         `json:"timestamp"`
	ReceivedAt    string         `json:"receivedAt"`
	WorkspaceID   string         `json:"workspaceId"`
	ProjectID     string         `json:"projectId"`
	ApplicationID string         `json:"applicationId"`
	Environment   string         `json:"environment"`
	Service       string         `json:"service"`
	Level         string         `json:"level"`
	Message       string         `json:"message"`
	TraceID       string         `json:"traceId,omitempty"`
	CorrelationID string         `json:"correlationId,omitempty"`
	Metadata      map[string]any `json:"metadata,omitempty"`
}
