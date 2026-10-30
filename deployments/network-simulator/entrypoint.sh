#!/bin/sh

set -eu

RUNTIME_DIRECTORY="/run/traceflow"
NETWORK_METADATA_FILE="${RUNTIME_DIRECTORY}/network.env"

echo "Starting network simulator..."

mkdir -p "${RUNTIME_DIRECTORY}"

echo "Detecting network interfaces..."

EXTERNAL_INTERFACE="$(
    ip -4 route show default |
    awk 'NR == 1 {print $5}'
)"

if [ -z "${EXTERNAL_INTERFACE}" ]; then
    echo "Failed to detect external interface."
    exit 1
fi

PERFORMANCE_INTERFACE="$(
    ip -4 route show scope link |
    awk -v external="${EXTERNAL_INTERFACE}" \
        '$2 == "dev" && $3 != external {print $3; exit}'
)"

if [ -z "${PERFORMANCE_INTERFACE}" ]; then
    echo "Failed to detect performance interface."
    exit 1
fi

EXTERNAL_NETWORK="$(
    ip -4 route show \
        dev "${EXTERNAL_INTERFACE}" \
        scope link |
    awk 'NR == 1 {print $1}'
)"

PERFORMANCE_NETWORK="$(
    ip -4 route show \
        dev "${PERFORMANCE_INTERFACE}" \
        scope link |
    awk 'NR == 1 {print $1}'
)"

EXTERNAL_IP="$(
    ip -4 addr show dev "${EXTERNAL_INTERFACE}" |
    awk '
        /inet / {
            split($2, address, "/")
            print address[1]
            exit
        }
    '
)"

PERFORMANCE_IP="$(
    ip -4 addr show dev "${PERFORMANCE_INTERFACE}" |
    awk '
        /inet / {
            split($2, address, "/")
            print address[1]
            exit
        }
    '
)"

if [ -z "${EXTERNAL_NETWORK}" ]; then
    echo "Failed to detect external network."
    exit 1
fi

if [ -z "${PERFORMANCE_NETWORK}" ]; then
    echo "Failed to detect performance network."
    exit 1
fi

if [ -z "${EXTERNAL_IP}" ]; then
    echo "Failed to detect external IP."
    exit 1
fi

if [ -z "${PERFORMANCE_IP}" ]; then
    echo "Failed to detect performance IP."
    exit 1
fi

echo ""
echo "Network discovery completed."
echo "External interface:    ${EXTERNAL_INTERFACE}"
echo "External IP:           ${EXTERNAL_IP}"
echo "External network:      ${EXTERNAL_NETWORK}"
echo "Performance interface: ${PERFORMANCE_INTERFACE}"
echo "Performance IP:        ${PERFORMANCE_IP}"
echo "Performance network:   ${PERFORMANCE_NETWORK}"
echo ""

cat > "${NETWORK_METADATA_FILE}.tmp" <<EOF
EXTERNAL_INTERFACE=${EXTERNAL_INTERFACE}
EXTERNAL_IP=${EXTERNAL_IP}
EXTERNAL_NETWORK=${EXTERNAL_NETWORK}
PERFORMANCE_INTERFACE=${PERFORMANCE_INTERFACE}
PERFORMANCE_IP=${PERFORMANCE_IP}
PERFORMANCE_NETWORK=${PERFORMANCE_NETWORK}
EOF

mv "${NETWORK_METADATA_FILE}.tmp" "${NETWORK_METADATA_FILE}"

echo "Network metadata written to:"
echo "${NETWORK_METADATA_FILE}"

echo ""
echo "Configuring forwarding rules..."

iptables -C FORWARD \
    -i "${EXTERNAL_INTERFACE}" \
    -o "${PERFORMANCE_INTERFACE}" \
    -j ACCEPT 2>/dev/null || \
iptables -A FORWARD \
    -i "${EXTERNAL_INTERFACE}" \
    -o "${PERFORMANCE_INTERFACE}" \
    -j ACCEPT

iptables -C FORWARD \
    -i "${PERFORMANCE_INTERFACE}" \
    -o "${EXTERNAL_INTERFACE}" \
    -m conntrack \
    --ctstate ESTABLISHED,RELATED \
    -j ACCEPT 2>/dev/null || \
iptables -A FORWARD \
    -i "${PERFORMANCE_INTERFACE}" \
    -o "${EXTERNAL_INTERFACE}" \
    -m conntrack \
    --ctstate ESTABLISHED,RELATED \
    -j ACCEPT

echo "Configuring NAT..."

iptables -t nat -C POSTROUTING \
    -o "${PERFORMANCE_INTERFACE}" \
    -j MASQUERADE 2>/dev/null || \
iptables -t nat -A POSTROUTING \
    -o "${PERFORMANCE_INTERFACE}" \
    -j MASQUERADE

echo ""
echo "IP forwarding status:"
cat /proc/sys/net/ipv4/ip_forward

echo ""
echo "Routing table:"
ip route

echo ""
echo "Network interfaces:"
ip addr

echo ""
echo "Forwarding rules:"
iptables -L FORWARD -n -v

echo ""
echo "NAT rules:"
iptables -t nat -L POSTROUTING -n -v

echo ""
echo "Network simulator is ready."

exec tail -f /dev/null