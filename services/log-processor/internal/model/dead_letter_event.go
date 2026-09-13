package model

type DeadLetterEvent struct {
	FailedAt        string `json:"failedAt"`
	SourceTopic     string `json:"sourceTopic"`
	Partition       int32  `json:"partition"`
	Offset          int64  `json:"offset"`
	FailureStage    string `json:"failureStage"`
	FailureReason   string `json:"failureReason"`
	OriginalPayload string `json:"originalPayload"`

	EventID       string `json:"eventId,omitempty"`
	WorkspaceID   string `json:"workspaceId,omitempty"`
	ProjectID     string `json:"projectId,omitempty"`
	ApplicationID string `json:"applicationId,omitempty"`
}

const (
	FailureStageJSONDecode = "json_decode"
	FailureStageValidation = "validation"
	FailureStageIndexing   = "indexing"
)
