#!/bin/sh

set -eu

echo "TraceFlow Performance Load Generator"

NETWORK_METADATA_FILE="/run/traceflow/network.env"
NETWORK_SIMULATOR_HOST="performance-gateway"

echo "Waiting for network metadata..."

while [ ! -s "${NETWORK_METADATA_FILE}" ]; do
    sleep 1
done

. "${NETWORK_METADATA_FILE}"

echo ""
echo "Detected performance network:"
echo "${PERFORMANCE_NETWORK}"

echo ""
echo "Resolving network simulator..."

NETWORK_SIMULATOR_IP="$(
    getent hosts "${NETWORK_SIMULATOR_HOST}" |
    awk 'NR == 1 {print $1}'
)"

if [ -z "${NETWORK_SIMULATOR_IP}" ]; then
    echo "Failed to resolve ${NETWORK_SIMULATOR_HOST}."
    exit 1
fi

echo "Network simulator IP: ${NETWORK_SIMULATOR_IP}"

echo ""
echo "Configuring performance network route..."

ip route replace \
    "${PERFORMANCE_NETWORK}" \
    via "${NETWORK_SIMULATOR_IP}"

echo ""
echo "Routing table:"
ip route

echo ""
echo "Load generator is ready."

exec sleep infinity