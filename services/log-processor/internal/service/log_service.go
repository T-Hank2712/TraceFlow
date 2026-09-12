package service

import (
	"context"
	"errors"
	"strings"

	"github.com/T-Hank2712/traceflow/log-processor/internal/model"
	"github.com/T-Hank2712/traceflow/log-processor/internal/repository"
)

type LogService struct {
	repository *repository.OpenSearchRepository
}

func NewLogService(
	repository *repository.OpenSearchRepository,
) *LogService {
	return &LogService{
		repository: repository,
	}
}

func (s *LogService) Process(event *model.LogEvent) error {

	if err := validate(event); err != nil {
		return err
	}

	normalize(event)

	if err := s.repository.IndexLog(
		context.Background(),
		event,
	); err != nil {
		return err
	}

	return nil
}

func validate(event *model.LogEvent) error {
	if strings.TrimSpace(event.EventID) == "" {
		return errors.New("event id is required")
	}

	if strings.TrimSpace(event.Timestamp) == "" {
		return errors.New("timestamp is required")
	}

	if strings.TrimSpace(event.ReceivedAt) == "" {
		return errors.New("received at is required")
	}

	if strings.TrimSpace(event.WorkspaceID) == "" {
		return errors.New("workspace id is required")
	}

	if strings.TrimSpace(event.ProjectID) == "" {
		return errors.New("project id is required")
	}

	if strings.TrimSpace(event.ApplicationID) == "" {
		return errors.New("application id is required")
	}

	if strings.TrimSpace(event.Environment) == "" {
		return errors.New("environment is required")
	}

	if strings.TrimSpace(event.Service) == "" {
		return errors.New("service name is required")
	}

	if strings.TrimSpace(event.Level) == "" {
		return errors.New("level is required")
	}

	if strings.TrimSpace(event.Message) == "" {
		return errors.New("message is required")
	}

	return nil
}

func normalize(event *model.LogEvent) {
	event.EventID = strings.TrimSpace(event.EventID)
	event.Timestamp = strings.TrimSpace(event.Timestamp)
	event.ReceivedAt = strings.TrimSpace(event.ReceivedAt)
	event.WorkspaceID = strings.TrimSpace(event.WorkspaceID)
	event.ProjectID = strings.TrimSpace(event.ProjectID)
	event.ApplicationID = strings.TrimSpace(event.ApplicationID)
	event.Environment = strings.ToLower(strings.TrimSpace(event.Environment))
	event.Service = strings.TrimSpace(event.Service)
	event.Level = strings.ToUpper(strings.TrimSpace(event.Level))
	event.Message = strings.TrimSpace(event.Message)
	event.TraceID = strings.TrimSpace(event.TraceID)
	event.CorrelationID = strings.TrimSpace(event.CorrelationID)
}
