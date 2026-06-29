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
const requestDuration = new Trend('request_duration');

export const options = {
  stages: [
    { target: 100, duration: '1m' },
    { target: 200, duration: '1m' },
    { target: 300, duration: '1m' },
    { target: 400, duration: '1m' },
    { target: 500, duration: '1m' },
    { target: 500, duration: '10m' },
    { target: 300, duration: '1m' },
    { target: 100, duration: '1m' },
    { target: 0, duration: '1m' },
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000', 'p(99)<5000'],
    http_req_failed: ['rate<0.05'],
    errors: ['rate<0.05'],
  },
};

const ENDPOINTS = [
  { method: 'GET', path: '/api/v1/iam/users', name: 'list_users' },
  { method: 'GET', path: '/api/v1/orders/orders', name: 'list_orders' },
  { method: 'GET', path: '/api/v1/billing/bills/open', name: 'list_open_bills' },
  { method: 'GET', path: '/api/v1/crm/customers', name: 'list_customers' },
  { method: 'GET', path: '/api/v1/billing/tax-rules', name: 'list_tax_rules' },
];

const WRITE_ENDPOINTS = [
  {
    method: 'POST',
    path: '/api/v1/iam/users',
    name: 'create_user',
    body: () => JSON.stringify({
      tenantId: 'tenant-stress-001',
      username: `stress-${Date.now()}-${Math.floor(Math.random() * 10000)}`,
      email: `stress-${Date.now()}-${Math.floor(Math.random() * 10000)}@obss-test.com`,
      firstName: 'Stress',
      lastName: 'Test',
    }),
  },
  {
    method: 'POST',
    path: '/api/v1/crm/customers',
    name: 'create_customer',
    body: () => JSON.stringify({
      tenantId: 'tenant-stress-001',
      name: `Stress Customer ${Math.floor(Math.random() * 10000)}`,
      email: `stress-${Date.now()}@obss-test.com`,
      type: 'Residential',
      status: 'Active',
    }),
  },
  {
    method: 'POST',
    path: '/api/v1/orders/orders',
    name: 'create_order',
    body: () => JSON.stringify({
      customerId: randomUUID(),
      customerName: `Stress Order ${Math.floor(Math.random() * 10000)}`,
      orderType: 'New',
      currency: 'USD',
      items: [{
        productId: randomUUID(),
        offerId: randomUUID(),
        productName: 'Internet 100Mbps',
        offerName: 'Fiber Standard',
        quantity: 1,
        unitPrice: 99.99,
        recurringPrice: 0,
        discountAmount: 0,
        taxAmount: 5.00,
        billingPeriod: 'Monthly',
      }],
    }),
  },
  {
    method: 'POST',
    path: '/api/v1/billing/bills/generate',
    name: 'generate_bill',
    body: () => JSON.stringify({
      customerId: randomUUID(),
      customerName: `Stress Bill ${Math.floor(Math.random() * 10000)}`,
      billingPeriod: 'Monthly',
      periodStart: '2026-06-01T00:00:00Z',
      periodEnd: '2026-06-30T23:59:59Z',
      dueDate: '2026-07-15T00:00:00Z',
      currency: 'USD',
    }),
  },
];

export default function () {
  group('Mixed API Workload', () => {
    const readEndpoint = ENDPOINTS[Math.floor(Math.random() * ENDPOINTS.length)];
    const durationStart = Date.now();

    const readRes = http.get(`${BASE_URL}${readEndpoint.path}`, {
      tags: { name: readEndpoint.name },
    });
    requestDuration.add(Date.now() - durationStart);

    const readOk = check(readRes, {
      [`${readEndpoint.name} status is 200`]: (r) => r.status === 200,
    });
    errorRate.add(!readOk);

    if (Math.random() < 0.3) {
      const writeEndpoint = WRITE_ENDPOINTS[Math.floor(Math.random() * WRITE_ENDPOINTS.length)];
      const writeStart = Date.now();

      const writeRes = http.post(`${BASE_URL}${writeEndpoint.path}`, writeEndpoint.body(), {
        headers: { 'Content-Type': 'application/json' },
        tags: { name: writeEndpoint.name },
      });
      requestDuration.add(Date.now() - writeStart);

      const writeOk = check(writeRes, {
        [`${writeEndpoint.name} status is 2xx`]: (r) => r.status >= 200 && r.status < 300,
      });
      errorRate.add(!writeOk);
    }

    sleep(Math.random() * 1 + 0.5);
  });
}
