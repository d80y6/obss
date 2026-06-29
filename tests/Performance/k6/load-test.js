import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { SharedArray } from 'k6/data';

function randomUUID() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

const errorRate = new Rate('errors');
const loginTrend = new Trend('login_duration');
const customerCreateTrend = new Trend('customer_create_duration');
const orderCreateTrend = new Trend('order_create_duration');
const invoiceTrend = new Trend('invoice_duration');

const users = new SharedArray('users', () => {
  const result = [];
  for (let i = 0; i < 100; i++) {
    result.push({
      username: `loaduser_${i}@obss-test.com`,
      password: 'Test123!',
      tenantId: 'tenant-load-001',
    });
  }
  return result;
});

const products = [
  { id: 'a1b2c3d4-0001-4000-8000-000000000001', name: 'Internet 100Mbps', price: 99.99 },
  { id: 'a1b2c3d4-0002-4000-8000-000000000002', name: 'TV Premium', price: 149.99 },
  { id: 'a1b2c3d4-0003-4000-8000-000000000003', name: 'VoIP Unlimited', price: 29.99 },
];

export const thresholds = {
  http_req_duration: ['p(95)<500'],
  http_req_failed: ['rate<0.01'],
  login_duration: ['p(95)<500'],
  customer_create_duration: ['p(95)<500'],
  order_create_duration: ['p(95)<500'],
  invoice_duration: ['p(95)<500'],
  errors: ['rate<0.01'],
};

export const options = {
  scenarios: {
    user_login: {
      executor: 'constant-vus',
      vus: 100,
      duration: '5m',
      exec: 'userLoginFlow',
      startTime: '0s',
    },
    customer_crud: {
      executor: 'constant-vus',
      vus: 50,
      duration: '5m',
      exec: 'customerCrudFlow',
      startTime: '30s',
    },
    order_processing: {
      executor: 'constant-vus',
      vus: 30,
      duration: '5m',
      exec: 'orderCreationFlow',
      startTime: '60s',
    },
    invoice_generation: {
      executor: 'constant-vus',
      vus: 20,
      duration: '5m',
      exec: 'invoiceGenerationFlow',
      startTime: '90s',
    },
  },
  thresholds: thresholds,
};

export function userLoginFlow() {
  group('User Login Flow', () => {
    const user = users[Math.floor(Math.random() * users.length)];
    const loginStart = Date.now();

    const loginPayload = JSON.stringify({
      email: user.username,
      password: user.password,
      tenantId: user.tenantId,
    });

    const loginRes = http.post(`${BASE_URL}/api/v1/iam/auth/login`, loginPayload, {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'login' },
    });

    loginTrend.add(Date.now() - loginStart);

    const loginOk = check(loginRes, {
      'login status is 200': (r) => r.status === 200,
      'login returns token': (r) => r.json('token') !== undefined,
    });

    errorRate.add(!loginOk);

    if (loginOk) {
      const token = loginRes.json('token');
      const headers = {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      };

      const profileRes = http.get(`${BASE_URL}/api/v1/iam/users/me`, {
        headers: headers,
        tags: { name: 'get_profile' },
      });

      check(profileRes, {
        'profile status is 200': (r) => r.status === 200,
      });
    }

    sleep(Math.random() * 3 + 1);
  });
}

export function customerCrudFlow() {
  group('Customer CRUD', () => {
    const customerId = randomUUID();

    const createPayload = JSON.stringify({
      tenantId: 'tenant-load-001',
      name: `LoadTest Customer ${customerId.substring(0, 8)}`,
      email: `customer_${customerId.substring(0, 8)}@obss-test.com`,
      phone: '+967712345678',
      type: 'Residential',
      status: 'Active',
    });

    const customerStart = Date.now();
    const createRes = http.post(`${BASE_URL}/api/v1/crm/customers`, createPayload, {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'create_customer' },
    });
    customerCreateTrend.add(Date.now() - customerStart);

    const createOk = check(createRes, {
      'customer create status is 201': (r) => r.status === 201,
    });
    errorRate.add(!createOk);

    if (createOk) {
      const createdId = createRes.json('id');

      const getRes = http.get(`${BASE_URL}/api/v1/crm/customers/${createdId}`, {
        tags: { name: 'get_customer' },
      });

      check(getRes, {
        'get customer status is 200': (r) => r.status === 200,
      });

      const updatePayload = JSON.stringify({
        customerId: createdId,
        name: `Updated Customer ${customerId.substring(0, 8)}`,
        email: `updated_${customerId.substring(0, 8)}@obss-test.com`,
      });

      const updateRes = http.put(`${BASE_URL}/api/v1/crm/customers/${createdId}`, updatePayload, {
        headers: { 'Content-Type': 'application/json' },
        tags: { name: 'update_customer' },
      });

      check(updateRes, {
        'update customer status is 200': (r) => r.status === 200,
      });

      const searchRes = http.get(`${BASE_URL}/api/v1/crm/customers?searchTerm=LoadTest&page=1&pageSize=20`, {
        tags: { name: 'search_customers' },
      });

      check(searchRes, {
        'search customers status is 200': (r) => r.status === 200,
      });
    }

    sleep(Math.random() * 2 + 1);
  });
}

export function orderCreationFlow() {
  group('Order Creation and Processing', () => {
    const orderStart = Date.now();

    const orderPayload = JSON.stringify({
      customerId: randomUUID(),
      customerName: `Order Customer ${Math.floor(Math.random() * 10000)}`,
      orderType: 'New',
      notes: 'Performance load test order',
      currency: 'USD',
      items: [
        {
          productId: products[0].id,
          offerId: randomUUID(),
          productName: products[0].name,
          offerName: `${products[0].name} - Standard`,
          quantity: 1,
          unitPrice: products[0].price,
          recurringPrice: 0,
          discountAmount: 0,
          taxAmount: products[0].price * 0.05,
          billingPeriod: 'Monthly',
        },
      ],
    });

    const createRes = http.post(`${BASE_URL}/api/v1/orders/orders`, orderPayload, {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'create_order' },
    });

    orderCreateTrend.add(Date.now() - orderStart);

    const createOk = check(createRes, {
      'order create status is 201': (r) => r.status === 201,
    });
    errorRate.add(!createOk);

    if (createOk) {
      const orderId = createRes.json('id');

      const submitRes = http.post(`${BASE_URL}/api/v1/orders/orders/${orderId}/submit`, null, {
        tags: { name: 'submit_order' },
      });

      check(submitRes, {
        'submit order status is 204': (r) => r.status === 204,
      });

      sleep(0.5);

      const approveRes = http.post(`${BASE_URL}/api/v1/orders/orders/${orderId}/approve`, null, {
        tags: { name: 'approve_order' },
      });

      check(approveRes, {
        'approve order status is 204': (r) => r.status === 204,
      });

      const validateRes = http.post(`${BASE_URL}/api/v1/orders/orders/${orderId}/validate`, null, {
        tags: { name: 'validate_order' },
      });

      check(validateRes, {
        'validate order status is 200': (r) => r.status === 200,
      });

      const getRes = http.get(`${BASE_URL}/api/v1/orders/orders/${orderId}`, {
        tags: { name: 'get_order' },
      });

      check(getRes, {
        'get order status is 200': (r) => r.status === 200,
      });
    }

    sleep(Math.random() * 3 + 2);
  });
}

export function invoiceGenerationFlow() {
  group('Invoice Generation', () => {
    const invoiceStart = Date.now();

    const customerId = randomUUID();

    const billPayload = JSON.stringify({
      customerId: customerId,
      customerName: `Invoice Customer ${Math.floor(Math.random() * 10000)}`,
      billingPeriod: 'Monthly',
      periodStart: '2026-06-01T00:00:00Z',
      periodEnd: '2026-06-30T23:59:59Z',
      dueDate: '2026-07-15T00:00:00Z',
      currency: 'USD',
    });

    const generateRes = http.post(`${BASE_URL}/api/v1/billing/bills/generate`, billPayload, {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'generate_bill' },
    });

    invoiceTrend.add(Date.now() - invoiceStart);

    const generateOk = check(generateRes, {
      'bill generate status is 201': (r) => r.status === 201,
    });
    errorRate.add(!generateOk);

    if (generateOk) {
      const billId = generateRes.json('id');

      const calculateRes = http.post(`${BASE_URL}/api/v1/billing/bills/${billId}/calculate-taxes`, null, {
        tags: { name: 'calculate_taxes' },
      });

      check(calculateRes, {
        'calculate taxes status is 200': (r) => r.status === 200,
      });

      sleep(0.3);

      const finalizeRes = http.post(`${BASE_URL}/api/v1/billing/bills/${billId}/finalize`, null, {
        tags: { name: 'finalize_bill' },
      });

      check(finalizeRes, {
        'finalize bill status is 204': (r) => r.status === 204,
      });

      const getRes = http.get(`${BASE_URL}/api/v1/billing/bills/${billId}`, {
        tags: { name: 'get_bill' },
      });

      check(getRes, {
        'get bill status is 200': (r) => r.status === 200,
      });
    }

    sleep(Math.random() * 4 + 2);
  });
}
