import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL;

export const options = {
    vus: 1,
    iterations: 1,
};

export default function () {
    const response = http.get(`${BASE_URL}/control/health`);

    check(response, {
        'status is 200': (res) => res.status === 200,
    });
}