package ports

import "context"

type LogPublisher interface {
	Publish(ctx context.Context, value []byte) error
}
