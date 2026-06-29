import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend } from 'k6/metrics';

function randomUUID() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

const errorRate = new Rate('errors');
const spikeResponseTime = new Trend('spike_response_time');

export const options = {
  stages: [
    { target: 1000, duration: '30s' },
    { target: 1000, duration: '2m' },
    { target: 0, duration: '1m' },
  ],
  thresholds: {
    http_req_duration: ['p(95)<3000', 'p(99)<8000'],
    http_req_failed: ['rate<0.10'],
    http_reqs: ['rate>100'],
    errors: ['rate<0.10'],
  },
};

const SPIKE_ENDPOINTS = [
  {
    method: 'GET',
    path: () => '/api/v1/iam/users',
    name: 'list_users',
  },
  {
    method: 'GET',
    path: () => `/api/v1/iam/users/${randomUUID()}`,
    name: 'get_user',
  },
  {
    method: 'GET',
    path: () => '/api/v1/orders/orders',
    name: 'list_orders',
  },
  {
    method: 'GET',
    path: () => `/api/v1/orders/orders/${randomUUID()}`,
    name: 'get_order',
  },
  {
    method: 'GET',
    path: () => '/api/v1/billing/bills/open',
    name: 'open_bills',
  },
  {
    method: 'GET',
    path: () => `/api/v1/billing/bills/${randomUUID()}`,
    name: 'get_bill',
  },
  {
    method: 'GET',
    path: () => '/api/v1/crm/customers',
    name: 'list_customers',
  },
  {
    method: 'POST',
    path: () => '/api/v1/iam/users',
    name: 'create_user',
    body: () => JSON.stringify({
      tenantId: 'tenant-spike-001',
      username: `spike-${Date.now()}-${Math.floor(Math.random() * 100000)}`,
      email: `spike-${Date.now()}-${Math.floor(Math.random() * 100000)}@obss-test.com`,
      firstName: 'Spike',
      lastName: 'Test',
    }),
  },
  {
    method: 'POST',
    path: () => '/api/v1/orders/orders',
    name: 'create_order',
    body: () => JSON.stringify({
      customerId: randomUUID(),
      customerName: `Spike Order ${Math.floor(Math.random() * 100000)}`,
      orderType: 'New',
      currency: 'USD',
      items: [{
        productId: randomUUID(),
        offerId: randomUUID(),
        productName: 'Internet 1Gbps',
        offerName: 'Fiber Gigabit',
        quantity: 1,
        unitPrice: 199.99,
        recurringPrice: 0,
        discountAmount: 0,
        taxAmount: 10.00,
        billingPeriod: 'Monthly',
      }],
    }),
  },
  {
    method: 'POST',
    path: () => '/api/v1/billing/bills/generate',
    name: 'generate_bill',
    body: () => JSON.stringify({
      customerId: randomUUID(),
      customerName: `Spike Bill ${Math.floor(Math.random() * 100000)}`,
      billingPeriod: 'Monthly',
      periodStart: '2026-06-01T00:00:00Z',
      periodEnd: '2026-06-30T23:59:59Z',
      dueDate: '2026-07-15T00:00:00Z',
      currency: 'USD',
    }),
  },
  {
    method: 'POST',
    path: () => '/api/v1/crm/customers',
    name: 'create_customer',
    body: () => JSON.stringify({
      tenantId: 'tenant-spike-001',
      name: `Spike Customer ${Math.floor(Math.random() * 100000)}`,
      email: `spike-${Date.now()}@obss-test.com`,
      type: 'Residential',
      status: 'Active',
    }),
  },
];

export default function () {
  group('Spike Request', () => {
    const endpoint = SPIKE_ENDPOINTS[Math.floor(Math.random() * SPIKE_ENDPOINTS.length)];
    const startTime = Date.now();

    const params = {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: endpoint.name },
    };

    let res;
    switch (endpoint.method) {
      case 'GET':
        res = http.get(`${BASE_URL}${endpoint.path()}`, params);
        break;
      case 'POST':
        res = http.post(`${BASE_URL}${endpoint.path()}`, endpoint.body(), params);
        break;
    }

    spikeResponseTime.add(Date.now() - startTime);

    const ok = check(res, {
      [`${endpoint.name} request succeeded`]: (r) => {
        if (r.status === 404) return true;
        if (r.status === 400) return true;
        return r.status >= 200 && r.status < 500;
      },
    });
    errorRate.add(!ok);

    const recoveryCheck = check(res, {
      [`${endpoint.name} endpoint responsive`]: (r) => r.status !== 0 && r.status !== 503,
    });
    errorRate.add(!recoveryCheck);
  });

  sleep(Math.random() * 0.5 + 0.1);
}
