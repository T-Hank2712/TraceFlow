package controlapi

type validateAPIKeyResponse struct {
	Valid         bool   `json:"valid"`
	WorkspaceID   string `json:"workspaceId"`
	ProjectID     string `json:"projectId"`
	ApplicationID string `json:"applicationId"`
	Environment   string `json:"environment"`
}
