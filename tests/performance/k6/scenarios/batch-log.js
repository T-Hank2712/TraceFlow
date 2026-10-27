import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL;
const API_KEY = __ENV.API_KEY;

const BATCH_SIZE = Number(__ENV.BATCH_SIZE || 10);

export const options = {
    vus: 1,
    iterations: 1,
};

function createLog(index) {
    return {
        service: 'payment-service',
        level: 4,
        message: `Payment succeeded - batch item ${index}`,
        metadata: {
            amount: 150000,
        },
    };
}

export default function () {
    const logs = [];

    for (let i = 0; i < BATCH_SIZE; i++) {
        logs.push(createLog(i));
    }

    const payload = JSON.stringify({
        logs: logs,
    });

    const response = http.post(
        `${BASE_URL}/ingestion/v1/batch-logs`,
        payload,
        {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `ApiKey ${API_KEY}`,
            },
        }
    );

    console.log(`Status: ${response.status}`);
    console.log(`Body: ${response.body}`);
    console.log(`Headers: ${JSON.stringify(response.headers)}`);

    check(response, {
        'batch request accepted': (res) =>
            res.status === 200 || res.status === 202 || res.status === 207,
    });
}