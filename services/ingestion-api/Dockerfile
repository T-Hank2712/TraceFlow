FROM golang:1.26 AS build
WORKDIR /src

COPY go.mod go.sum ./
RUN go mod download

COPY . .
RUN CGO_ENABLED=1 GOOS=linux go build -o /app/ingestion-api ./cmd/api

FROM debian:bookworm-slim AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates librdkafka1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/ingestion-api .

EXPOSE 8080

ENTRYPOINT ["./ingestion-api"]