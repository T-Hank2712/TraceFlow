#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../../deployments/performance" && pwd)"

COMPOSE_FILE="${SCRIPT_DIR}/docker-compose.performance.yml"

NGINX_CONTAINER="traceflow-performance-nginx"

API_KEY="${API_KEY:-}"

if [ -z "$API_KEY" ]; then
    echo "Error: API_KEY is required."
    exit 1
fi

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
echo "Scenario: spike"

docker compose \
    --env-file "${SCRIPT_DIR}/.env.performance" \
    -f "$COMPOSE_FILE" \
    run --rm \
    -e "BASE_URL=${BASE_URL}" \
    -e "API_KEY=${API_KEY}" \
    k6 run "/tests/scenarios/spike.js"