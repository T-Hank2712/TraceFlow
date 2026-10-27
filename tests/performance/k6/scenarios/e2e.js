import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL;
const API_KEY = __ENV.API_KEY;
const TEST_MARKER = __ENV.TEST_MARKER;

export const options = {
    vus: 1,
    iterations: 1,
};

export default function () {
    const payload = JSON.stringify({
        service: 'performance-test',
        level: 4,
        message: `TraceFlow E2E test ${TEST_MARKER}`,
        metadata: {
            testMarker: TEST_MARKER,
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

    console.log(`E2E marker: ${TEST_MARKER}`);
    console.log(`Ingestion status: ${response.status}`);
    console.log(`Ingestion response: ${response.body}`);

    check(response, {
        'ingestion request accepted': (res) =>
            res.status >= 200 && res.status < 300,
    });
}