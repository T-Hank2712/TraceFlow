package auth

type ValidationResult struct {
	Valid         bool   `json:"valid"`
	WorkspaceID   string `json:"workspaceId"`
	ProjectID     string `json:"projectId"`
	ApplicationID string `json:"applicationId"`
	Environment   string `json:"environment"`
}
