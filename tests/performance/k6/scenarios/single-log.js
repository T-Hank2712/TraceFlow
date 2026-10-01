import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL;
const API_KEY = __ENV.API_KEY;

export const options = {
    vus: 1,
    iterations: 1,
};

export default function () {
    const payload = JSON.stringify({
        service: 'performance-test',
        level: 2,
        message: 'TraceFlow performance single-log test'
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

    console.log(`Status: ${response.status}`);
    console.log(`Body: ${response.body}`);

    check(response, {
        'status is 200': (res) => res.status === 200,
    });
}