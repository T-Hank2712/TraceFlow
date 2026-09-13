package ports

type HealthChecker interface {
	HealthCheck() error
}
