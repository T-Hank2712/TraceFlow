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
	event.Level = strings.ToUpper(strings.TrimSpace(event.Level))
	event.Service = strings.TrimSpace(event.Service)
	event.Message = strings.TrimSpace(event.Message)
}
