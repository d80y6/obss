import random
import uuid
from datetime import datetime, timedelta
from locust import HttpUser, task, between, constant

API_PREFIX = "/api/v1"


class IamUser(HttpUser):
    wait_time = between(1, 3)
    weight = 3

    def on_start(self):
        self.tenant_id = "tenant-locust-001"
        self.user_id = None
        self.token = None
        self._create_user()

    def _create_user(self):
        unique_id = uuid.uuid4().hex[:12]
        payload = {
            "tenantId": self.tenant_id,
            "username": f"locust_{unique_id}",
            "email": f"locust_{unique_id}@obss-test.com",
            "firstName": "Locust",
            "lastName": "User",
        }
        with self.client.post(
            f"{API_PREFIX}/iam/users",
            json=payload,
            catch_response=True,
            name="create_user",
        ) as response:
            if response.status_code == 201:
                self.user_id = response.json().get("id")

    @task(5)
    def list_users(self):
        self.client.get(
            f"{API_PREFIX}/iam/users",
            params={"page": 1, "pageSize": 20},
            name="list_users",
        )

    @task(3)
    def get_user(self):
        if self.user_id:
            self.client.get(
                f"{API_PREFIX}/iam/users/{self.user_id}",
                name="get_user",
            )

    @task(2)
    def create_user(self):
        unique_id = uuid.uuid4().hex[:12]
        payload = {
            "tenantId": self.tenant_id,
            "username": f"locust_new_{unique_id}",
            "email": f"locust_new_{unique_id}@obss-test.com",
            "firstName": "New",
            "lastName": "Locust",
        }
        self.client.post(
            f"{API_PREFIX}/iam/users",
            json=payload,
            name="create_user",
        )

    @task(1)
    def deactivate_user(self):
        if self.user_id:
            self.client.post(
                f"{API_PREFIX}/iam/users/{self.user_id}/deactivate",
                name="deactivate_user",
            )


class CrmUser(HttpUser):
    wait_time = between(2, 5)
    weight = 2

    def on_start(self):
        self.customer_id = None
        self._create_customer()

    def _create_customer(self):
        unique_id = uuid.uuid4().hex[:12]
        payload = {
            "tenantId": "tenant-locust-001",
            "name": f"Locust Customer {unique_id}",
            "email": f"customer_{unique_id}@obss-test.com",
            "phone": "+967712345678",
            "type": "Residential",
            "status": "Active",
        }
        with self.client.post(
            f"{API_PREFIX}/crm/customers",
            json=payload,
            catch_response=True,
            name="create_customer",
        ) as response:
            if response.status_code == 201:
                self.customer_id = response.json().get("id")

    @task(4)
    def search_customers(self):
        self.client.get(
            f"{API_PREFIX}/crm/customers",
            params={"searchTerm": "Locust", "page": 1, "pageSize": 20},
            name="search_customers",
        )

    @task(3)
    def get_customer(self):
        if self.customer_id:
            self.client.get(
                f"{API_PREFIX}/crm/customers/{self.customer_id}",
                name="get_customer",
            )

    @task(2)
    def create_customer(self):
        unique_id = uuid.uuid4().hex[:12]
        payload = {
            "tenantId": "tenant-locust-001",
            "name": f"Load Customer {unique_id}",
            "email": f"load_{unique_id}@obss-test.com",
            "type": "Business",
            "status": "Active",
        }
        self.client.post(
            f"{API_PREFIX}/crm/customers",
            json=payload,
            name="create_customer",
        )

    @task(1)
    def update_customer(self):
        if self.customer_id:
            payload = {
                "customerId": self.customer_id,
                "name": f"Updated Locust Customer {uuid.uuid4().hex[:8]}",
            }
            self.client.put(
                f"{API_PREFIX}/crm/customers/{self.customer_id}",
                json=payload,
                name="update_customer",
            )


class OrderUser(HttpUser):
    wait_time = between(3, 6)
    weight = 2

    def on_start(self):
        self.order_id = None
        self.customer_id = str(uuid.uuid4())

    @task(3)
    def create_and_process_order(self):
        product_id = str(uuid.uuid4())
        offer_id = str(uuid.uuid4())
        payload = {
            "customerId": self.customer_id,
            "customerName": f"Order Customer {uuid.uuid4().hex[:8]}",
            "orderType": random.choice(["New", "Renewal", "Change"]),
            "notes": "Locust performance test order",
            "currency": "USD",
            "items": [
                {
                    "productId": product_id,
                    "offerId": offer_id,
                    "productName": "Internet 100Mbps",
                    "offerName": "Fiber Standard",
                    "quantity": 1,
                    "unitPrice": 99.99,
                    "recurringPrice": 0,
                    "discountAmount": 0,
                    "taxAmount": 5.00,
                    "billingPeriod": "Monthly",
                }
            ],
        }

        with self.client.post(
            f"{API_PREFIX}/orders/orders",
            json=payload,
            catch_response=True,
            name="create_order",
        ) as response:
            if response.status_code == 201:
                self.order_id = response.json().get("id")

        if self.order_id:
            self.client.post(
                f"{API_PREFIX}/orders/orders/{self.order_id}/submit",
                name="submit_order",
            )

            self.client.post(
                f"{API_PREFIX}/orders/orders/{self.order_id}/approve",
                name="approve_order",
            )

    @task(2)
    def query_orders(self):
        self.client.get(
            f"{API_PREFIX}/orders/orders",
            params={"page": 1, "pageSize": 20},
            name="list_orders",
        )

    @task(1)
    def get_order(self):
        if self.order_id:
            self.client.get(
                f"{API_PREFIX}/orders/orders/{self.order_id}",
                name="get_order_detail",
            )


class BillingUser(HttpUser):
    wait_time = between(4, 8)
    weight = 1

    def on_start(self):
        self.bill_id = None
        self.customer_id = str(uuid.uuid4())

    @task(3)
    def generate_bill(self):
        period_start = datetime.utcnow().replace(day=1)
        if period_start.month == 1:
            period_end = period_start.replace(month=12, day=31)
        else:
            period_end = period_start.replace(month=period_start.month - 1, day=1) - timedelta(days=1)

        payload = {
            "customerId": self.customer_id,
            "customerName": f"Billing Customer {uuid.uuid4().hex[:8]}",
            "billingPeriod": "Monthly",
            "periodStart": period_start.isoformat(),
            "periodEnd": period_end.isoformat(),
            "dueDate": (period_end + timedelta(days=15)).isoformat(),
            "currency": "USD",
        }

        with self.client.post(
            f"{API_PREFIX}/billing/bills/generate",
            json=payload,
            catch_response=True,
            name="generate_bill",
        ) as response:
            if response.status_code == 201:
                self.bill_id = response.json().get("id")

        if self.bill_id:
            self.client.post(
                f"{API_PREFIX}/billing/bills/{self.bill_id}/calculate-taxes",
                name="calculate_taxes",
            )

            self.client.post(
                f"{API_PREFIX}/billing/bills/{self.bill_id}/finalize",
                name="finalize_bill",
            )

    @task(2)
    def list_bills(self):
        self.client.get(
            f"{API_PREFIX}/billing/bills/open",
            name="list_open_bills",
        )

    @task(1)
    def get_bill(self):
        if self.bill_id:
            self.client.get(
                f"{API_PREFIX}/billing/bills/{self.bill_id}",
                name="get_bill_detail",
            )

    @task(1)
    def list_tax_rules(self):
        self.client.get(
            f"{API_PREFIX}/billing/tax-rules",
            name="list_tax_rules",
        )
