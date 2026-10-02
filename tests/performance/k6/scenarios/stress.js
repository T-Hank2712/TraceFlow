import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL;
const API_KEY = __ENV.API_KEY;

export const options = {
    stages: [
        { duration: '2m', target: 15 },
        { duration: '2m', target: 20 },
        { duration: '3m', target: 20 },
        { duration: '2m', target: 0 },
    ],

    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
    },
};

export default function () {
    const eventId = `stress-${__VU}-${__ITER}-${Date.now()}`;

    const payload = JSON.stringify({
        service: 'order-service',
        environment: 'production',
        level: 2,
        message: `Order ${eventId} completed successfully`,
        timestamp: new Date().toISOString(),

        metadata: {
            requestId: `req-${eventId}`,
            traceId: `trace-${eventId}`,
            spanId: `span-${__VU}-${__ITER}`,

            user: {
                id: `user-${__VU}`,
                role: 'customer',
            },

            request: {
                method: 'POST',
                endpoint: '/api/v1/orders',
                clientIp: '10.10.24.15',
                userAgent: 'mobile-app/2.4.1',
            },

            order: {
                id: `order-${eventId}`,
                itemCount: 3,
                currency: 'USD',
                totalAmount: 149.97,
                paymentMethod: 'credit_card',
            },

            response: {
                statusCode: 201,
                durationMs: 87,
            },

            infrastructure: {
                host: `order-api-${__VU % 4 + 1}`,
                region: 'ap-southeast-1',
                version: '1.8.3',
            },
        },
    });

    const response = http.post(
        `${BASE_URL}/ingestion/v1/logs`,
        payload,
        {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `ApiKey ${API_KEY}`,
            },
        }
    );

    check(response, {
        'status is 200': (res) => res.status === 200,
    });
}