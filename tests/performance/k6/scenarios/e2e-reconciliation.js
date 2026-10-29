import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL;
const API_KEY = __ENV.API_KEY;
const RUN_ID = __ENV.E2E_RUN_ID;
const P95_THRESHOLD_MS = 10;

const VUS = 50;
const ITERATIONS_PER_VU = 1000;

if (!RUN_ID) {
    throw new Error('E2E_RUN_ID is required.');
}

export const options = {
    scenarios: {
        e2e_reconciliation: {
            executor: 'per-vu-iterations',
            vus: VUS,
            iterations: ITERATIONS_PER_VU,
            maxDuration: '2m',
        },
    },
    thresholds: {
        http_req_failed: ['rate==0'],
        http_req_duration: [`p(95)<${P95_THRESHOLD_MS}`],
        checks: ['rate==1'],
    },
};

export default function () {
    const eventKey = `${RUN_ID}-${__VU}-${__ITER}`;

    const payload = JSON.stringify({
        service: 'e2e-reconciliation',
        environment: 'performance',
        level: 2,
        message: `E2E reconciliation event ${eventKey}`,
        timestamp: new Date().toISOString(),
        metadata: {
            e2eRunId: RUN_ID,
            e2eEventKey: eventKey,
            e2eVu: __VU,
            e2eIteration: __ITER,
            e2eExpectedVus: VUS,
            e2eExpectedIterationsPerVu: ITERATIONS_PER_VU,
        },
    });

    const response = http.post(
        `${BASE_URL}/ingestion/v1/logs`,
        payload,
        {
            headers: {
                'Content-Type': 'application/json',
                Authorization: `ApiKey ${API_KEY}`,
                'Idempotency-Key': eventKey,
            },
        },
    );

    check(response, {
        'ingestion accepted event': (res) => res.status === 200,
        'ingestion returned event id': (res) => {
            if (res.status !== 200) {
                return false;
            }

            try {
                return Boolean(res.json('eventId'));
            } catch (_) {
                return false;
            }
        },
    });
}
