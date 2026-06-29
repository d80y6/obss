import puppeteer from "puppeteer";
import fs from "fs";

const FRONTEND_URL = "http://localhost:3080";
const API_URL = "http://localhost:5020";
const KEYCLOAK_URL = "http://localhost:8080";

const results = [];
let passed = 0;
let failed = 0;

function report(page, test, status, detail = "") {
  results.push({ page, test, status, detail });
  if (status === "PASS") passed++;
  else failed++;
  console.log(`  [${status}] ${page} \u2039 ${test}${detail ? ": " + detail : ""}`);
}

async function sleep(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

async function main() {
  console.log("\n=== PHASE 8A \u2014 FRONTEND RUNTIME VALIDATION (Puppeteer) ===\n");

  const browser = await puppeteer.launch({
    executablePath: "/snap/bin/chromium",
    headless: true,
    args: ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"],
  });

  try {
    const page = await browser.newPage();
    page.on("pageerror", (err) => console.log(`  [PAGE ERROR] ${err.message}`));

    // STEP 1: Login
    console.log("\n--- STEP 1: Authentication ---");
    await page.goto(`${FRONTEND_URL}/login`, { waitUntil: "domcontentloaded", timeout: 20000 });
    await sleep(1500);
    await page.type('input[name="username"]', "admin");
    await page.type('input[type="password"]', "admin123");
    await page.click('button[type="submit"]');
    await sleep(5000);

    const hasToken = await page.evaluate(() => !!localStorage.getItem("auth-token"));
    report("Login", "Login with Keycloak", hasToken ? "PASS" : "FAIL", hasToken ? "Token stored, URL: " + page.url() : "No token, URL: " + page.url());
    if (!hasToken) { console.log("  Login failed - continuing with limited tests"); }

    // STEP 2: Page Content Validation
    console.log("\n--- STEP 2: All Pages ---");

    const pagesToTest = [
      "/login","/dashboard","/customers","/customers/new","/products","/products/new",
      "/products/offers","/products/categories","/orders","/orders/new","/subscriptions",
      "/billing","/billing/cycles","/billing/jobs","/billing/tax-rules","/invoices",
      "/invoices/new","/invoices/disputes","/invoices/credit-notes","/payments",
      "/payments/new","/payments/reconciliation","/payments/refunds","/tickets",
      "/tickets/new","/tickets/sla","/collections","/collections/new",
      "/collections/reports/aging","/services","/service-inventory",
      "/service-inventory/new","/service-inventory/discovery","/network",
      "/network/elements","/network/elements/new","/network/olts","/network/vlans",
      "/network/vlans/new","/network/topology","/network/capacity","/provisioning",
      "/provisioning/jobs/new","/provisioning/templates","/provisioning/templates/new",
      "/workflow","/workflow/definitions/new","/workflow/instances","/workflow/slas",
      "/workflow/slas/new","/notifications","/notifications/templates",
      "/notifications/templates/new","/notifications/preferences","/reporting",
      "/reporting/definitions","/reporting/definitions/new","/reporting/scheduled",
      "/reporting/scheduled/new","/audit","/audit/alerts","/audit/alert-rules",
      "/audit/alert-rules/new","/admin","/admin/users/new","/admin/roles",
      "/admin/roles/new","/admin/tenants","/admin/tenants/new","/api-gateway",
      "/api-gateway/routes","/api-gateway/routes/new","/api-gateway/api-keys",
      "/api-gateway/api-keys/new","/api-gateway/partners","/api-gateway/partners/new",
      "/customers/test-id","/orders/test-id","/tickets/test-id","/admin/users/test-id",
    ];

    for (const path of pagesToTest) {
      const name = path.replace(/^\//, "").replace(/\//g, " \u203a ") || "home";
      try {
        const resp = await page.goto(`${FRONTEND_URL}${path}`, {
          waitUntil: "domcontentloaded", timeout: 20000,
        });
        await sleep(1500);
        const status = resp?.status() ?? 0;
        const hasError = await page.evaluate(() => {
          return document.getElementById("__next_error__") ? true : false;
        });
        if ((status === 200 || status === 304) && !hasError) {
          report(name, "Page renders", "PASS", `HTTP ${status}`);
        } else if ((status === 200 || status === 304) && hasError) {
          report(name, "Page renders", "FAIL", `__next_error__ present`);
        } else {
          report(name, "Page renders", "FAIL", `HTTP ${status}`);
        }
      } catch (e) {
        report(name, "Page renders", "FAIL", e.message.substring(0, 80));
      }
    }

    // STEP 3: Form Tests
    console.log("\n--- STEP 3: Forms ---");

    await page.goto(`${FRONTEND_URL}/customers/new`, { waitUntil: "domcontentloaded", timeout: 20000 });
    await sleep(2000);
    const inputs = await page.$$('input');
    report("Customers/Create", "Input fields", inputs.length > 0 ? "PASS" : "FAIL", `${inputs.length} inputs`);
    const submitBtn = await page.$('button[type="submit"]');
    if (submitBtn) {
      await submitBtn.click(); await sleep(1000);
      const errs = await page.evaluate(() => document.body.innerText.includes("required") ? "Has validation" : "No errors");
      report("Customers/Create", "Validation on empty submit", errs.includes("validation") ? "PASS" : "FAIL", errs);
    }

    await page.goto(`${FRONTEND_URL}/orders/new`, { waitUntil: "domcontentloaded", timeout: 20000 });
    await sleep(2000);
    const steps = await page.$$('[class*="rounded-full"]');
    report("Orders/Create", "Multi-step wizard", steps.length >= 2 ? "PASS" : "FAIL", `${steps.length} steps`);

    // STEP 4: Navigation
    console.log("\n--- STEP 4: Navigation ---");
    await page.goto(`${FRONTEND_URL}/customers`, { waitUntil: "domcontentloaded", timeout: 20000 });
    await sleep(2000);
    const navLinks = await page.$$('nav a');
    report("Navigation", "Sidebar links", navLinks.length >= 18 ? "PASS" : "FAIL", `${navLinks.length} links`);

    // STEP 5: UI Elements
    console.log("\n--- STEP 5: UI ---");
    await page.goto(`${FRONTEND_URL}/dashboard`, { waitUntil: "domcontentloaded", timeout: 20000 });
    await sleep(2000);
    const themeBtn = await page.$('[title*="dark"], [title*="light"]');
    report("Theme", "Theme toggle", themeBtn ? "PASS" : "FAIL");
    const text = await page.evaluate(() => document.body.innerText);
    report("Locale", "Locale toggle (AR)", text.includes("العربية") ? "PASS" : "FAIL");
    report("Auth", "Logout button visible", text.includes("Logout") ? "PASS" : "FAIL");

    // STEP 6: API Verification
    console.log("\n--- STEP 6: Backend APIs ---");
    const lr = await fetch(`${KEYCLOAK_URL}/realms/obss/protocol/openid-connect/token`, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({ client_id: "obss-frontend", username: "admin", password: "admin", grant_type: "password" }),
    });
    if (lr.ok) {
      const d = await lr.json();
      const token = d.access_token;
      const apis = [
        ["Tenants", `${API_URL}/api/v1/iam/tenants`, {}],
        ["Reporting Dashboard", `${API_URL}/api/v1/reporting/dashboard`, { "X-Tenant-Id": "default" }],
        ["CRM Customers", `${API_URL}/api/v1/crm/customers`, { "X-Tenant-Id": "default" }],
        ["Provisioning Templates", `${API_URL}/api/v1/provisioning/templates`, { "X-Tenant-Id": "default" }],
      ];
      for (const [name, url, h] of apis) {
        const r = await fetch(url, { headers: { Authorization: `Bearer ${token}`, ...h } });
        report("API", name, r.ok ? "PASS" : "FAIL", `HTTP ${r.status}`);
      }
    }

  } finally {
    await browser.close();
  }

  console.log("\n" + "=".repeat(60));
  console.log(`RESULTS: ${passed}/${passed + failed} passed (${((passed/(passed+failed))*100).toFixed(1)}%)`);
  console.log("=".repeat(60));
  const failures = results.filter((r) => r.status === "FAIL");
  if (failures.length) {
    console.log("\nFAILURES:");
    failures.forEach((f) => console.log(`  [FAIL] ${f.page} \u2039 ${f.test}: ${f.detail}`));
  }
  fs.writeFileSync("/tmp/validation-results.json", JSON.stringify(results, null, 2));
}

main().catch(console.error);
