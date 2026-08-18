package model

type LogRequest struct {
	Service string `json:"service"`
	Level   string `json:"level"`
	Message string `json:"message"`
}
