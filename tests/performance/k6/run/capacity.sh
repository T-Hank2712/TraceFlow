#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../../deployments/performance" && pwd)"

COMPOSE_FILE="${SCRIPT_DIR}/docker-compose.performance.yml"
NGINX_CONTAINER="traceflow-performance-nginx"

API_KEY="${API_KEY:-}"
TARGET_RPS="${TARGET_RPS:-1000}"
DURATION="${DURATION:-3m}"
PRE_ALLOCATED_VUS="${PRE_ALLOCATED_VUS:-100}"
MAX_VUS="${MAX_VUS:-300}"

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
echo "Scenario: capacity"
echo "Target RPS: ${TARGET_RPS}"
echo "Duration: ${DURATION}"

docker compose \
    --env-file "${SCRIPT_DIR}/.env.performance" \
    -f "$COMPOSE_FILE" \
    run --rm \
    -e "BASE_URL=${BASE_URL}" \
    -e "API_KEY=${API_KEY}" \
    -e "TARGET_RPS=${TARGET_RPS}" \
    -e "DURATION=${DURATION}" \
    -e "PRE_ALLOCATED_VUS=${PRE_ALLOCATED_VUS}" \
    -e "MAX_VUS=${MAX_VUS}" \
    k6 run "/tests/scenarios/capacity.js"