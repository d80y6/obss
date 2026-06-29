#!/usr/bin/env bash
set -euo pipefail

BASE_URL="http://localhost:5020"
KEYCLOAK_URL="http://localhost:8080"
REALM="obss"
CLIENT_ID="obss-api"
CLIENT_SECRET="obss-api-secret"
USERNAME="admin"
PASSWORD="admin123"
TENANT_ID="default"

step=0

fail() {
  echo "FAIL: step $step - $1"
  exit 1
}

check_http() {
  local label="$1" code="$2"
  if [ "$code" -ge 200 ] && [ "$code" -lt 300 ]; then
    echo "  [PASS] $label (HTTP $code)"
  else
    echo "  [FAIL] $label (HTTP $code)"
    fail "$label failed with HTTP $code"
  fi
}

echo "======================================"
echo " OBSS End-to-End Test Suite"
echo "======================================"

# --------------------------------------------------
step=1
echo ""
echo "--- Step $step: Get OIDC Token ---"
TOKEN_RESP=$(curl -s -X POST "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "client_secret=$CLIENT_SECRET" \
  -d "grant_type=password" \
  -d "username=$USERNAME" \
  -d "password=$PASSWORD")

ACCESS_TOKEN=$(echo "$TOKEN_RESP" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    print(data['access_token'])
except Exception as e:
    print(f'ERROR: {e}', file=sys.stderr)
    sys.exit(1)
")
echo "  Token obtained: ${ACCESS_TOKEN:0:20}..."

# --------------------------------------------------
step=2
echo ""
echo "--- Step $step: Create Customer ---"
CUST_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/crm/customers" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "'"$TENANT_ID"'",
    "customerType": "Business",
    "companyName": "OBSS E2E Customer",
    "displayName": "OBSS E2E Customer",
    "email": "e2e-customer@obss.com",
    "currency": "USD"
  }')
CUST_HTTP=$(echo "$CUST_RESP" | tail -1)
CUST_BODY=$(echo "$CUST_RESP" | sed '$d')
check_http "Create Customer" "$CUST_HTTP"
echo "  Response: $CUST_BODY" | head -c 300
CUSTOMER_ID=$(echo "$CUST_BODY" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
echo "  Customer ID: $CUSTOMER_ID"

# --------------------------------------------------
step=3
echo ""
echo "--- Step $step: Create Product ---"
PROD_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/catalog/products" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "'"$TENANT_ID"'",
    "name": "Fiber Internet 1Gbps",
    "description": "High-speed fiber internet",
    "productType": 3,
    "isShippable": false,
    "taxable": true,
    "taxCategory": "Services"
  }')
PROD_HTTP=$(echo "$PROD_RESP" | tail -1)
PROD_BODY=$(echo "$PROD_RESP" | sed '$d')
check_http "Create Product" "$PROD_HTTP"
echo "  Response: $PROD_BODY" | head -c 300
PRODUCT_ID=$(echo "$PROD_BODY" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
echo "  Product ID: $PRODUCT_ID"

# --------------------------------------------------
step=4
echo ""
echo "--- Step $step: Create Offer ---"
OFFER_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/catalog/offers" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "'"$TENANT_ID"'",
    "name": "Fiber 1Gbps - Monthly",
    "description": "Monthly fiber internet subscription",
    "offerType": 2,
    "isContract": false,
    "taxInclusive": false,
    "sortOrder": 1,
    "pricings": [
      {
        "pricingType": 1,
        "currency": "USD",
        "recurringPrice": 79.99,
        "oneTimePrice": 0,
        "usagePrice": 0,
        "unitOfMeasure": "month",
        "minQuantity": 1,
        "maxQuantity": 1,
        "isActive": true
      }
    ]
  }')
OFFER_HTTP=$(echo "$OFFER_RESP" | tail -1)
OFFER_BODY=$(echo "$OFFER_RESP" | sed '$d')
check_http "Create Offer" "$OFFER_HTTP"
echo "  Response: $OFFER_BODY" | head -c 300
OFFER_ID=$(echo "$OFFER_BODY" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
echo "  Offer ID: $OFFER_ID"

# --------------------------------------------------
step=5
echo ""
echo "--- Step $step: Create Order ---"
ORD_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/orders/orders" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "'"$CUSTOMER_ID"'",
    "customerName": "OBSS E2E Customer",
    "orderType": "New",
    "currency": "USD",
    "items": [
      {
        "productId": "'"$PRODUCT_ID"'",
        "offerId": "'"$OFFER_ID"'",
        "productName": "Fiber Internet 1Gbps",
        "offerName": "Fiber 1Gbps - Monthly",
        "quantity": 1,
        "unitPrice": 79.99,
        "recurringPrice": 79.99,
        "discountAmount": 0,
        "taxAmount": 0,
        "billingPeriod": "Monthly"
      }
    ]
  }')
ORD_HTTP=$(echo "$ORD_RESP" | tail -1)
ORD_BODY=$(echo "$ORD_RESP" | sed '$d')
check_http "Create Order" "$ORD_HTTP"
echo "  Response: $ORD_BODY" | head -c 300
ORDER_ID=$(echo "$ORD_BODY" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
echo "  Order ID: $ORDER_ID"

# --------------------------------------------------
step=6
echo ""
echo "--- Step $step: Submit Order ---"
SUBMIT_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/orders/orders/$ORDER_ID/submit" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID")
SUBMIT_HTTP=$(echo "$SUBMIT_RESP" | tail -1)
check_http "Submit Order" "$SUBMIT_HTTP"
echo "  Order $ORDER_ID submitted"

# --------------------------------------------------
step=7
echo ""
echo "--- Step $step: Approve Order ---"
APPR_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/orders/orders/$ORDER_ID/approve" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID")
APPR_HTTP=$(echo "$APPR_RESP" | tail -1)
check_http "Approve Order" "$APPR_HTTP"
echo "  Order $ORDER_ID approved"

# --------------------------------------------------
step=8
echo ""
echo "--- Step $step: Generate Bill ---"
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
NEXT_MONTH=$(date -u -d "+30 days" +"%Y-%m-%dT%H:%M:%SZ")
BILL_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/billing/bills/generate" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "'"$CUSTOMER_ID"'",
    "customerName": "OBSS E2E Customer",
    "billingPeriod": "Monthly",
    "periodStart": "'"$NOW"'",
    "periodEnd": "'"$NEXT_MONTH"'",
    "dueDate": "'"$(date -u -d "+45 days" +"%Y-%m-%dT%H:%M:%SZ")"'",
    "currency": "USD"
  }')
BILL_HTTP=$(echo "$BILL_RESP" | tail -1)
BILL_BODY=$(echo "$BILL_RESP" | sed '$d')
check_http "Generate Bill" "$BILL_HTTP"
echo "  Response: $BILL_BODY" | head -c 300
BILL_ID=$(echo "$BILL_BODY" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
echo "  Bill ID: $BILL_ID"

# --------------------------------------------------
step=9
echo ""
echo "--- Step $step: Finalize Bill ---"
FINAL_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/billing/bills/$BILL_ID/finalize" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID")
FINAL_HTTP=$(echo "$FINAL_RESP" | tail -1)
check_http "Finalize Bill" "$FINAL_HTTP"
echo "  Bill $BILL_ID finalized"

# --------------------------------------------------
step=10
echo ""
echo "--- Step $step: Create Invoice from Bill ---"
INV_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/invoices/invoices" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "'"$TENANT_ID"'",
    "billId": "'"$BILL_ID"'",
    "customerId": "'"$CUSTOMER_ID"'",
    "customerName": "OBSS E2E Customer",
    "customerEmail": "e2e-customer@obss.com",
    "customerAddress": "123 Main St, City",
    "currency": "USD"
  }')
INV_HTTP=$(echo "$INV_RESP" | tail -1)
INV_BODY=$(echo "$INV_RESP" | sed '$d')
check_http "Create Invoice" "$INV_HTTP"
echo "  Response: $INV_BODY" | head -c 300
INVOICE_ID=$(echo "$INV_BODY" | python3 -c "
import sys,json
try:
    print(json.load(sys.stdin)['id'])
except:
    print('unknown')
")
echo "  Invoice ID: $INVOICE_ID"

# --------------------------------------------------
step=11
echo ""
echo "--- Step $step: Record Payment ---"
PAY_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/payments/payments" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "'"$CUSTOMER_ID"'",
    "amount": 79.99,
    "currency": "USD",
    "paymentMethod": "CreditCard",
    "paymentReference": "PAY-E2E-001",
    "invoiceId": "'"$INVOICE_ID"'",
    "notes": "E2E test payment"
  }')
PAY_HTTP=$(echo "$PAY_RESP" | tail -1)
PAY_BODY=$(echo "$PAY_RESP" | sed '$d')
check_http "Record Payment" "$PAY_HTTP"
echo "  Response: $PAY_BODY" | head -c 300

# --------------------------------------------------
step=12
echo ""
echo "--- Step $step: Create Support Ticket ---"
TKT_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/ticketing/tickets" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "'"$TENANT_ID"'",
    "customerId": "'"$CUSTOMER_ID"'",
    "customerName": "OBSS E2E Customer",
    "subject": "E2E Test - Billing Issue",
    "description": "This is a test ticket created by the E2E test suite",
    "priority": "High",
    "category": "Billing",
    "source": "Portal"
  }')
TKT_HTTP=$(echo "$TKT_RESP" | tail -1)
TKT_BODY=$(echo "$TKT_RESP" | sed '$d')
check_http "Create Ticket" "$TKT_HTTP"
echo "  Response: $TKT_BODY" | head -c 300
TICKET_ID=$(echo "$TKT_BODY" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
echo "  Ticket ID: $TICKET_ID"

# --------------------------------------------------
echo ""
echo "======================================"
echo " ALL E2E TESTS PASSED SUCCESSFULLY"
echo "======================================"
echo "  Customer ID: $CUSTOMER_ID"
echo "  Product ID:  $PRODUCT_ID"
echo "  Offer ID:    $OFFER_ID"
echo "  Order ID:    $ORDER_ID"
echo "  Invoice ID:  $INVOICE_ID"
echo "  Ticket ID:   $TICKET_ID"
echo "======================================"
