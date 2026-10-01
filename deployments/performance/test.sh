#!/usr/bin/env bash

set -euo pipefail

COMPOSE_FILE="docker-compose.performance.yml"
ENV_FILE=".env.performance"
NGINX_CONTAINER="traceflow-performance-nginx"

SCENARIO="${1:-smoke}"

echo "Resolving Nginx target..."

NGINX_IP="$(
    docker inspect \
        -f '{{with index .NetworkSettings.Networks "traceflow-performance-network"}}{{.IPAddress}}{{end}}' \
        "$NGINX_CONTAINER"
)"

if [ -z "$NGINX_IP" ]; then
    echo "Error: could not resolve Nginx IP."
    exit 1
fi

BASE_URL="http://${NGINX_IP}"

echo "Target: ${BASE_URL}"
echo "Scenario: ${SCENARIO}"

docker compose \
    --env-file "$ENV_FILE" \
    -f "$COMPOSE_FILE" \
    run --rm \
    -e "BASE_URL=${BASE_URL}" \
    k6 run "/tests/scenarios/${SCENARIO}.js"