#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
PERFORMANCE_DIR="${ROOT_DIR}/deployments/performance"
COMPOSE_FILE="${PERFORMANCE_DIR}/docker-compose.performance.yml"
NGINX_CONTAINER="traceflow-performance-nginx"
VERIFIER_IMAGE="traceflow-e2e-verifier"

API_KEY="${API_KEY:-}"
E2E_RUN_ID="${E2E_RUN_ID:-traceflow-e2e-$(date +%Y%m%d-%H%M%S)-$$}"
E2E_TIMEOUT_SECONDS="${E2E_TIMEOUT_SECONDS:-120}"
E2E_K6_P95_MS="${E2E_K6_P95_MS:-500}"

if [ -z "${API_KEY}" ]; then
    echo "Error: API_KEY is required."
    exit 1
fi

NGINX_IP="$(
    docker inspect \
        -f '{{with index .NetworkSettings.Networks "traceflow-performance-network"}}{{.IPAddress}}{{end}}' \
        "${NGINX_CONTAINER}"
)"

if [ -z "${NGINX_IP}" ]; then
    echo "Error: could not resolve Nginx IP."
    exit 1
fi

BASE_URL="http://${NGINX_IP}"

echo "Target: ${BASE_URL}"
echo "E2E run ID: ${E2E_RUN_ID}"
echo "Workload: configured in tests/performance/k6/scenarios/e2e-reconciliation.js"

set +e

docker compose \
    --env-file "${PERFORMANCE_DIR}/.env.performance" \
    -f "${COMPOSE_FILE}" \
    run --rm \
    -e "BASE_URL=${BASE_URL}" \
    -e "API_KEY=${API_KEY}" \
    -e "E2E_RUN_ID=${E2E_RUN_ID}" \
    -e "E2E_K6_P95_MS=${E2E_K6_P95_MS}" \
    k6 run /tests/scenarios/e2e-reconciliation.js

K6_EXIT_CODE=$?

set -e

if ! docker image inspect "${VERIFIER_IMAGE}" >/dev/null 2>&1; then
    echo "Building E2E verifier image..."
    docker build \
        -t "${VERIFIER_IMAGE}" \
        -f "${ROOT_DIR}/tests/e2e/TraceFlow.PipelineVerifier/Dockerfile" \
        "${ROOT_DIR}/tests/e2e"
else
    echo "Using existing E2E verifier image: ${VERIFIER_IMAGE}"
fi

set +e

docker run --rm \
    --network traceflow-performance-network \
    --env-file "${PERFORMANCE_DIR}/.env.performance" \
    -e "E2E_RUN_ID=${E2E_RUN_ID}" \
    -e "E2E_TIMEOUT_SECONDS=${E2E_TIMEOUT_SECONDS}" \
    "${VERIFIER_IMAGE}"

VERIFIER_EXIT_CODE=$?

set -e

if [ "${K6_EXIT_CODE}" -ne 0 ] || [ "${VERIFIER_EXIT_CODE}" -ne 0 ]; then
    echo "E2E reconciliation failed. k6=${K6_EXIT_CODE}, verifier=${VERIFIER_EXIT_CODE}"
    exit 1
fi
