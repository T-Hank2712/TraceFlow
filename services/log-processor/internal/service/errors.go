package service

import "errors"

var (
	ErrInvalidLogEvent = errors.New("invalid log event")
	ErrIndexLogFailed  = errors.New("index log failed")
)
