FROM alpine:3.22

RUN apk add --no-cache \
    curl \
    iproute2 \
    iputils

WORKDIR /workspace

COPY entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]