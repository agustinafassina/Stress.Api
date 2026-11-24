// Usage: k6 run loadtest/k6-stress.js
//        k6 run -e BASE_URL=http://localhost:8787 loadtest/k6-stress.js
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5142';

export const options = {
  stages: [
    { duration: '30s', target: 20 }, // ramp up to 20 virtual users
    { duration: '1m', target: 20 },  // hold
    { duration: '15s', target: 0 },  // ramp down
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],   // less than 1% errors
    http_req_duration: ['p(95)<2000'], // 95% of requests under 2s
  },
};

const endpoints = [
  '/api/stress/cpu?iterations=5000000',
  '/api/stress/memory?sizeMb=50',
  '/api/stress/request?tasks=20',
  '/api/stress/latency?delayMs=100',
  '/api/stress/load?delayMs=200&sizeMb=10',
];

export default function () {
  const path = endpoints[Math.floor(Math.random() * endpoints.length)];
  const res = http.get(`${BASE_URL}${path}`);
  check(res, { 'status is 200': (r) => r.status === 200 });
  sleep(1);
}
