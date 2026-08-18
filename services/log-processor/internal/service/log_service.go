package service

import (
	"errors"
	"strings"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
)

type LogService struct{}

func NewLogService() *LogService {
	return &LogService{}
}

func (s *LogService) Process(event *model.LogEvent) error {
	if strings.TrimSpace(event.Service) == "" {
		return errors.New("service name is required")
	}

	if strings.TrimSpace(event.Level) == "" {
		return errors.New("level is required")
	}

	if strings.TrimSpace(event.Message) == "" {
		return errors.New("message is required")
	}

	event.Level = strings.ToUpper(strings.TrimSpace(event.Level))
	event.Service = strings.TrimSpace(event.Service)
	event.Message = strings.TrimSpace(event.Message)

	return nil
}
