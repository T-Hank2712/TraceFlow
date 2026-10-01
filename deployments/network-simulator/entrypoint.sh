#!/bin/sh

set -eu

EXTERNAL_INTERFACE="${EXTERNAL_INTERFACE:-eth0}"
PERFORMANCE_INTERFACE="${PERFORMANCE_INTERFACE:-eth1}"

echo "Starting network simulator..."
echo "External interface: ${EXTERNAL_INTERFACE}"
echo "Performance interface: ${PERFORMANCE_INTERFACE}"

echo "Enabling IP forwarding..."

iptables -A FORWARD \
    -i "${EXTERNAL_INTERFACE}" \
    -o "${PERFORMANCE_INTERFACE}" \
    -j ACCEPT

iptables -A FORWARD \
    -i "${PERFORMANCE_INTERFACE}" \
    -o "${EXTERNAL_INTERFACE}" \
    -m conntrack \
    --ctstate ESTABLISHED,RELATED \
    -j ACCEPT

iptables -t nat -A POSTROUTING \
    -o "${PERFORMANCE_INTERFACE}" \
    -j MASQUERADE

echo "Network routing configured."

echo "Routing table:"
ip route

echo "Network interfaces:"
ip addr

echo "Network simulator is ready."

exec tail -f /dev/null