# Contract Diff Report

**Date:** 2026-07-08
**Context:** WP-017 — Compare backend API contracts with frontend generated client

## Critical: Hooks Calling Non-Existent Paths (404 at runtime)

| Hook | URL Called | Correct Backend Path | Issue |
|------|-----------|---------------------|-------|
| `useTicketSla.ts` | `GET /api/v1/tickets/${id}/sla` | `GET /api/v1/ticketing/tickets/{id}/sla` | Missing `ticketing/` prefix |
| `useBillingCycles.ts` | `GET /api/v1/billing/cycles` | `POST /api/v1/billing/cycles` (no GET) | Wrong method |
| `useCustomerSegments.ts` | `GET /api/v1/crm/customers/${id}/segments` | Does not exist | No matching endpoint |
| `useCaseActions.ts` | `GET /api/v1/collections/cases/${id}/actions` | `POST /collections/cases/{id}/actions` (no GET) | Wrong method |
| `useCaseArrangements.ts` | `GET /api/v1/collections/cases/${id}/arrangements` | `POST /collections/cases/{id}/arrangements` (no GET) | Wrong method |
| `useDeleteSlaDefinition.ts` | `DELETE /api/v1/ticketing/sla-definitions/${id}` | Does not exist | No DELETE endpoint |
| `useUsageRecords.ts` | `GET /api/v1/rating/usage` | `POST /rating/usage` (no GET) | Wrong method |
| `useUsageRecord.ts` | `GET /api/v1/rating/usage/${id}` | Does not exist | No GET by id |
| `useRatingRule.ts` | `GET /api/v1/rating/rules/${id}` | Does not exist | No GET by id |
| `usePromotion.ts` | `GET /api/v1/rating/promotions/${id}` | Does not exist | No GET by id |

## Backend Endpoints with NO Frontend Coverage (~60+)

- **Audit:** entries CRUD, summary, compliance, alerts, alert-rules, purge
- **IAM:** tenant create, user deactivate, role id get/delete, role permissions, party-roles
- **CRM:** contact create/delete, note create, customer delete/patch, characteristics, credit-profiles, related-parties, hubs, contact-media, account/agreement/payment-method refs, individuals/organizations endpoints, agreements, segments
- **Catalog:** catalogs CRUD, category/product/product-specification/offer patch, nested sub-resources
- **Orders:** order patch/delete, order item delete, customer orders list
- **Billing:** billing-accounts CRUD, tax-exemptions
- **Invoices:** view/pdf/html, summary, credit-notes by invoice, disputes by invoice
- **Payments:** payment allocate
- **NetworkInventory:** element create/interfaces/ip-addresses/activate, OLT ports, vlans
- **NumberInventory:** number create/port-in/port-out/reserve/suspend/resume/disconnect
- **Notifications:** send direct, from-template, preferences
- **Ticketing:** open/sla-breached lists, assign/resolve/close/escalate, apply-sla
- **Workflow:** pending tasks, running instances, dashboard, metrics, slas
- **EventManagement:** **Completely missing** (0 hooks for 3 endpoints)

## Best-Covered Modules

ApiGateway, Subscriptions, ServiceCatalog, ServiceInventory, Orders, Reporting
