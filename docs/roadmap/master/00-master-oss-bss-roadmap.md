# MASTER OSS/BSS CAPABILITY ROADMAP

**Platform:** OBSS - Telecom Operations Support / Business Support System
**Version:** 2.0
**Date:** 2026-07-07
**Status:** FINAL - GOVERNANCE DOCUMENT

---

## TABLE OF CONTENTS

1. Executive Summary
2. Repository Inventory
3. Business Capability Inventory
4. Capability Matrix
5. TM Forum Mapping Matrix
6. Commercial Service Traceability Matrix
7. Cross-Module Dependency Graph
8. DDD Validation Report
9. Technical Debt Inventory
10. Gap Analysis Matrix
11. Execution Waves
12. Master Prioritized Backlog
13. Production Readiness Matrix
14. Recommended Execution Order
15. Definition of Repository Completion
16. First Implementation Prompt

---

## 1. EXECUTIVE SUMMARY

### Platform Overview
The OBSS platform is a .NET 9 modular monolith (~83 projects, ~1,566 C# files) with a Next.js 16 frontend (React 19, Tailwind CSS 4, TypeScript 5). It implements a complete Telecom OSS/BSS stack covering order-to-cash-to-care lifecycle for residential and enterprise services.

### Current Completion Status: ~90% implemented

**Phase 1-5: Complete** (Architecture, Commercial Core, Revenue Core, Operations Core, Service Assurance)
**Phase 6: Not Started** (Commercial Release: Portal, Partner APIs, Mobile, Hardening)

### Key Findings

| Dimension | Score | Assessment |
|-----------|-------|------------|
| **Backend Implementation** | ~85% | All 22 modules have Domain/Application/Infrastructure/Api layers |
| **Frontend Implementation** | ~80% | All modules have pages except EventManagement (0%) |
| **TMF Compliance** | ~58% | ProductCatalog 90% → Provisioning 30% |
| **Contract Alignment** | ~79% | Generated client stale for later modules |
| **Testing Coverage** | ~30% | 57 SharedKernel tests + 5 module integration test projects |
| **External Integrations** | ~5% | All gateways/services are MOCK implementations |
| **Production Readiness** | ~50% | Security gaps, mock integrations, no real-time frontend bridge |
| **Documentation** | ~85% | Architecture, ADRs, business reqs, validation reports complete |

### Critical Risks

1. **ALL external integrations are MOCK** - Payment gateways, email/SMS, network device protocols, provisioning engine. The system cannot process real payments, send real notifications, or manage real network devices.
2. **Security gaps** - JWT validation skips issuer/audience validation; credentials in source code; rate limiting JWT-users bypass; background service exceptions ignored.
3. **No distributed transaction safety** - UnitOfWork spans separate databases without atomicity guarantees.
4. **Cross-module command coupling** - Provisioning directly invokes Workflow commands, breaking module isolation.
5. **Subscriptions mis-architected** - Should be Product Inventory (TMF637), not subscription management.
6. **Missing production capabilities** - TMF648 Quote, TMF645 Service Qualification, real-time frontend events, SignalR bridge.

### Remaining Effort: ~5-6 months (4 waves)

| Wave | Focus | Duration | Effort (dev-weeks) |
|------|-------|----------|-------------------|
| Wave 0 | Foundation & Governance | Weeks 1-2 | 4 |
| Wave 1 | Commercial Foundation | Weeks 3-6 | 16 |
| Wave 2 | Customer Journey & Fulfillment | Weeks 7-14 | 32 |
| Wave 3 | Revenue & Experience | Weeks 15-22 | 32 |
| Wave 4 | Production Hardening | Weeks 23-26 | 16 |
| **Total** | | **26 weeks** | **100 dev-weeks** |

---

## 2. REPOSITORY INVENTORY

### 2.1 Backend Modules (22)

| # | Module | Path | Layers | DB Schema | TMF |
|---|--------|------|--------|-----------|-----|
| 1 | IAM | `src/Modules/IAM/` | D/A/I/Api | `iam` | TMF632/669 |
| 2 | CRM | `src/Modules/CRM/` | D/A/I/Api | `crm` | TMF629 |
| 3 | ProductCatalog | `src/Modules/ProductCatalog/` | D/A/I/Api | `catalog` | TMF620 |
| 4 | Orders | `src/Modules/Orders/` | D/A/I/Api | `orders` | TMF622 |
| 5 | Subscriptions | `src/Modules/Subscriptions/` | D/A/I/Api | `subscriptions` | TMF637 |
| 6 | ServiceCatalog | `src/Modules/ServiceCatalog/` | D/A/I/Api | `service_catalog` | TMF633 |
| 7 | ServiceInventory | `src/Modules/ServiceInventory/` | D/A/I/Api | `service_inventory` | TMF638 |
| 8 | Provisioning | `src/Modules/Provisioning/` | D/A/I/Api | `provisioning` | TMF641 |
| 9 | Billing | `src/Modules/Billing/` | D/A/I/Api | `billing` | TMF666 |
| 10 | Invoices | `src/Modules/Invoices/` | D/A/I/Api | `invoices` | TMF678 |
| 11 | Payments | `src/Modules/Payments/` | D/A/I/Api | `payments` | TMF676 |
| 12 | Collections | `src/Modules/Collections/` | D/A/I/Api | `collections` | — |
| 13 | Rating | `src/Modules/Rating/` | D/A/I/Api | `rating` | TMF677 |
| 14 | NetworkInventory | `src/Modules/NetworkInventory/` | D/A/I/Api | `network_inventory` | TMF639 |
| 15 | NumberInventory | `src/Modules/NumberInventory/` | D/A/I/Api | `number_inventory` | TMF639 |
| 16 | Workflow | `src/Modules/Workflow/` | D/A/I/Api | `workflow` | TMF701 |
| 17 | Ticketing | `src/Modules/Ticketing/` | D/A/I/Api | `ticketing` | TMF702 |
| 18 | Notifications | `src/Modules/Notifications/` | D/A/I/Api | `notifications` | TMF681 |
| 19 | Audit | `src/Modules/Audit/` | D/A/I/Api | `audit` | — |
| 20 | Reporting | `src/Modules/Reporting/` | D/A/I/Api | `reporting` | — |
| 21 | ApiGateway | `src/Modules/ApiGateway/` | D/A/I/Api | `gateway` | — |
| 22 | EventManagement | `src/Modules/EventManagement/` | D/A/I/Api | `event_management` | TMF688 |

### 2.2 Shared Infrastructure

| Component | Location | Purpose |
|-----------|----------|---------|
| Obss.SharedKernel | `src/Shared/Obss.SharedKernel/` | Entity, AggregateRoot, ValueObject, DomainEvent, IntegrationEvent, Repository, UnitOfWork, Outbox/Inbox, FieldSelector, Pagination |
| Obss.ModuleRegistration | `src/BuildingBlocks/Obss.ModuleRegistration/` | Auto-discovery module registration interface |
| Obss.Host | `src/Host/Obss.Host/` | Entry point: 22 DbContexts, MediatR, FluentValidation, Mapster, Keycloak auth, API key middleware, RabbitMQ outbox, OpenTelemetry |

### 2.3 Frontend Structure

| Component | Location | Details |
|-----------|----------|---------|
| Generated Client | `src/api/generated/` | 94 DTO interfaces, ~240 command types |
| Hooks | `src/api/hooks/` | 177 React Query hooks |
| Forms | `src/forms/` | Zod schemas + form components |
| Pages | `src/app/*/` | 29 route directories (23 with pages, 6 empty/admin) |
| Types | `src/types/` | Shared type definitions |
| Stores | `src/stores/` | Zustand stores (auth, UI state) |
| Services | `src/services/api.ts` | Base API service |
| Components | `src/components/` | Shared UI, Sidebar, Header, AppShell, Radix primitives |

### 2.4 Database

22 PostgreSQL schemas in a single database instance, each with independent EF Core migrations (latest migration: 2026-07-03).

### 2.5 Infrastructure (Docker Compose)

| Service | Version | Purpose |
|---------|---------|---------|
| PostgreSQL | 16 | Primary database (22 schemas) |
| Redis | 7 | Caching, session, rate limiting |
| RabbitMQ | 3.13 | Message broker (management UI) |
| MinIO | latest | Object storage (not integrated in code) |
| OpenSearch | 2.x | Log analytics (Serilog sink) |
| Keycloak | 26.x | Identity provider (JWT) |
| Prometheus | latest | Metrics collection |
| Grafana | latest | Metrics visualization |
| OTel Collector | latest | Telemetry pipeline |

### 2.6 CI/CD

GitHub Actions workflow: restore → build (Release) → test (Release) → format verification. Security scan workflow also exists.

---

## 3. BUSINESS CAPABILITY INVENTORY

### 3.1 Complete Capability List

| # | Capability | Category | Module | Status |
|---|-----------|----------|--------|--------|
| 1 | Identity Management | Foundation | IAM | COMPLETE |
| 2 | Authentication | Foundation | IAM | COMPLETE |
| 3 | Authorization | Foundation | IAM | COMPLETE |
| 4 | Multi-Tenancy | Foundation | IAM | COMPLETE |
| 5 | Role Management | Foundation | IAM | COMPLETE |
| 6 | Permission Management | Foundation | IAM | PARTIAL |
| 7 | Party Management | CRM | IAM | PARTIAL |
| 8 | Party Role Management | CRM | IAM | PARTIAL |
| 9 | Customer Management | CRM | CRM | COMPLETE |
| 10 | Individual Management | CRM | CRM | COMPLETE |
| 11 | Organization Management | CRM | CRM | COMPLETE |
| 12 | Customer Segmentation | CRM | CRM | COMPLETE |
| 13 | Customer Contact Management | CRM | CRM | COMPLETE |
| 14 | Customer Agreement Management | CRM | CRM | PARTIAL |
| 15 | Customer Credit Profile | CRM | CRM | COMPLETE |
| 16 | KYC/Identity Verification | CRM | CRM | COMPLETE |
| 17 | Product Catalog Management | Catalog | ProductCatalog | COMPLETE |
| 18 | Category Management | Catalog | ProductCatalog | COMPLETE |
| 19 | Product Specification Management | Catalog | ProductCatalog | COMPLETE |
| 20 | Offer Management | Catalog | ProductCatalog | COMPLETE |
| 21 | Pricing Management | Catalog | ProductCatalog | COMPLETE |
| 22 | Discount Management | Catalog | ProductCatalog | COMPLETE |
| 23 | Bundle Management | Catalog | ProductCatalog | COMPLETE |
| 24 | Product Configuration | Catalog | ProductCatalog | COMPLETE |
| 25 | Service Catalog Management | Catalog | ServiceCatalog | COMPLETE |
| 26 | Service Specification Management | Catalog | ServiceCatalog | COMPLETE |
| 27 | Service Category Management | Catalog | ServiceCatalog | COMPLETE |
| 28 | Service Candidate Management | Catalog | ServiceCatalog | COMPLETE |
| 29 | Quote Management | Pre-Order | — | MISSING |
| 30 | Service Qualification | Pre-Order | — | MISSING |
| 31 | Order Capture | Ordering | Orders | COMPLETE |
| 32 | Order Management | Ordering | Orders | COMPLETE |
| 33 | Order Fulfillment | Ordering | Orders | COMPLETE |
| 34 | Order Validation | Ordering | Orders | COMPLETE |
| 35 | Subscription Management | Inventory | Subscriptions | COMPLETE |
| 36 | Product Inventory | Inventory | Subscriptions | PARTIAL |
| 37 | Entitlement Management | Inventory | Subscriptions | COMPLETE |
| 38 | Service Inventory | Inventory | ServiceInventory | COMPLETE |
| 39 | Service Topology | Inventory | ServiceInventory | COMPLETE |
| 40 | Resource Discovery | Inventory | ServiceInventory | COMPLETE |
| 41 | Network Inventory | Inventory | NetworkInventory | COMPLETE |
| 42 | Network Topology | Inventory | NetworkInventory | COMPLETE |
| 43 | IP Address Management | Inventory | NetworkInventory | COMPLETE |
| 44 | VLAN Management | Inventory | NetworkInventory | COMPLETE |
| 45 | OLT/ONT Management | Inventory | NetworkInventory | COMPLETE |
| 46 | Number Inventory | Inventory | NumberInventory | COMPLETE |
| 47 | Number Portability | Inventory | NumberInventory | COMPLETE |
| 48 | Provisioning | Fulfillment | Provisioning | COMPLETE |
| 49 | Service Activation | Fulfillment | Provisioning | COMPLETE |
| 50 | Service Configuration | Fulfillment | Provisioning | COMPLETE |
| 51 | Workflow Management | Fulfillment | Workflow | COMPLETE |
| 52 | Workflow Execution | Fulfillment | Workflow | COMPLETE |
| 53 | Workflow SLA | Fulfillment | Workflow | COMPLETE |
| 54 | Usage Collection | Rating | Rating | COMPLETE |
| 55 | Usage Rating | Rating | Rating | COMPLETE |
| 56 | Real-Time Rating | Rating | Rating | COMPLETE |
| 57 | Promotion Management | Rating | Rating | COMPLETE |
| 58 | Discount Application | Rating | Rating | COMPLETE |
| 59 | Bill Generation | Billing | Billing | COMPLETE |
| 60 | Billing Cycle Management | Billing | Billing | COMPLETE |
| 61 | Tax Management | Billing | Billing | COMPLETE |
| 62 | Invoice Management | Billing | Invoices | COMPLETE |
| 63 | Credit Note Management | Billing | Invoices | COMPLETE |
| 64 | Invoice Disputes | Billing | Invoices | COMPLETE |
| 65 | Invoice PDF Generation | Billing | Invoices | COMPLETE |
| 66 | Payment Processing | Billing | Payments | COMPLETE |
| 67 | Payment Gateway Management | Billing | Payments | COMPLETE |
| 68 | Refund Management | Billing | Payments | COMPLETE |
| 69 | Payment Reconciliation | Billing | Payments | COMPLETE |
| 70 | Collection Management | Billing | Collections | COMPLETE |
| 71 | Payment Arrangements | Billing | Collections | COMPLETE |
| 72 | Dunning Management | Billing | Collections | COMPLETE |
| 73 | Ticketing | Service Assurance | Ticketing | COMPLETE |
| 74 | Ticket Assignment | Service Assurance | Ticketing | COMPLETE |
| 75 | Ticket Escalation | Service Assurance | Ticketing | COMPLETE |
| 76 | SLA Management | Service Assurance | Ticketing | COMPLETE |
| 77 | Notification Management | Service Assurance | Notifications | COMPLETE |
| 78 | Notification Templates | Service Assurance | Notifications | COMPLETE |
| 79 | Email Notifications | Service Assurance | Notifications | MOCK |
| 80 | SMS Notifications | Service Assurance | Notifications | MOCK |
| 81 | Push Notifications | Service Assurance | Notifications | MOCK |
| 82 | Audit Logging | Governance | Audit | COMPLETE |
| 83 | Audit Alerts | Governance | Audit | COMPLETE |
| 84 | Audit Compliance | Governance | Audit | COMPLETE |
| 85 | Reporting | Governance | Reporting | COMPLETE |
| 86 | Dashboard | Governance | Reporting | COMPLETE |
| 87 | Scheduled Reports | Governance | Reporting | COMPLETE |
| 88 | API Gateway | Integration | ApiGateway | COMPLETE |
| 89 | API Key Management | Integration | ApiGateway | COMPLETE |
| 90 | Rate Limiting | Integration | ApiGateway | COMPLETE |
| 91 | Partner Management | Integration | ApiGateway | COMPLETE |
| 92 | Event Subscription | Integration | EventManagement | COMPLETE |
| 93 | Webhook Dispatch | Integration | EventManagement | COMPLETE |
| 94 | External OSS Integration | Integration | — | MISSING |
| 95 | Customer Portal | Frontend | — | MISSING |
| 96 | Mobile Application | Frontend | — | MISSING |
| 97 | Real-Time Frontend Events | Frontend | — | MISSING |
| 98 | Service Qualification | Pre-Order | — | MISSING |

### 3.2 Missing Capabilities (7)

| Capability | Business Justification | Priority |
|------------|----------------------|----------|
| Quote Management (TMF648) | Pre-order price negotiation for enterprise quotes | HIGH |
| Service Qualification (TMF645) | Check address/service eligibility before ordering | HIGH |
| External OSS Integration | Integration with Radius, DHCP, OLT/ACS, BNG, NMS systems | HIGH |
| Customer Portal | Self-service portal for customers | MEDIUM |
| Mobile Application | Customer mobile app | LOW |
| Real-Time Frontend Events | SignalR/WebSocket bridge for live updates | MEDIUM |
| Partner API Portal | API marketplace for partners | LOW |

---

## 4. CAPABILITY MATRIX

### Per-Capability Completeness Assessment

| Capability | Backend | Frontend | DB | OpenAPI | Client | TMF | Integration | Testing | Security | Risk |
|------------|---------|----------|----|---------|--------|-----|-------------|---------|----------|------|
| Identity Management | 100% | 100% | 100% | 100% | 90% | 100% | 100% | 50% | 70% | LOW |
| AuthN/AuthZ | 90% | 90% | 90% | 90% | 90% | 100% | 100% | 30% | 40% | HIGH |
| Multi-Tenancy | 80% | 60% | 90% | 70% | 70% | 100% | 80% | 10% | 50% | HIGH |
| Role/Permission Mgmt | 90% | 80% | 90% | 80% | 80% | 70% | 90% | 30% | 70% | LOW |
| Party Management | 40% | 40% | 40% | 30% | 30% | 40% | 30% | 10% | 60% | HIGH |
| Customer Management | 90% | 90% | 90% | 90% | 90% | 75% | 60% | 30% | 70% | LOW |
| Customer Segmentation | 90% | 80% | 90% | 80% | 80% | 50% | 30% | 10% | 60% | LOW |
| Product Catalog | 95% | 90% | 95% | 90% | 90% | 90% | 40% | 20% | 70% | LOW |
| Service Catalog | 90% | 80% | 90% | 80% | 80% | 85% | 30% | 10% | 70% | LOW |
| Offer Management | 95% | 90% | 95% | 90% | 90% | 85% | 40% | 20% | 70% | LOW |
| Pricing/Discount | 90% | 80% | 90% | 80% | 80% | 80% | 30% | 10% | 70% | LOW |
| Quote Management | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | CRITICAL |
| Service Qualification | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | 0% | CRITICAL |
| Order Management | 95% | 90% | 95% | 90% | 90% | 85% | 70% | 40% | 70% | LOW |
| Order Fulfillment | 80% | 70% | 80% | 70% | 70% | 60% | 50% | 20% | 60% | MEDIUM |
| Subscription Mgmt | 85% | 80% | 85% | 80% | 80% | 50% | 60% | 20% | 60% | HIGH |
| Product Inventory | 30% | 20% | 30% | 20% | 20% | 20% | 20% | 10% | 50% | HIGH |
| Entitlement Mgmt | 90% | 80% | 90% | 80% | 80% | 30% | 40% | 10% | 50% | MEDIUM |
| Service Inventory | 85% | 80% | 85% | 80% | 80% | 80% | 40% | 10% | 60% | LOW |
| Service Topology | 80% | 60% | 80% | 70% | 70% | 60% | 30% | 10% | 50% | LOW |
| Resource Discovery | 70% | 50% | 70% | 60% | 60% | 40% | 20% | 10% | 50% | LOW |
| Network Inventory | 80% | 70% | 80% | 70% | 60% | 50% | 0% | 10% | 40% | HIGH |
| OLT/ONT Management | 70% | 50% | 70% | 50% | 40% | 30% | 0% | 0% | 30% | HIGH |
| IP Address Mgmt | 80% | 60% | 80% | 70% | 60% | 30% | 0% | 0% | 40% | HIGH |
| Number Inventory | 85% | 80% | 85% | 80% | 80% | 40% | 30% | 10% | 50% | MEDIUM |
| Provisioning | 75% | 60% | 75% | 70% | 70% | 30% | 30% | 10% | 50% | HIGH |
| Service Activation | 70% | 50% | 70% | 60% | 60% | 20% | 20% | 10% | 40% | HIGH |
| Workflow Management | 80% | 70% | 80% | 70% | 70% | 35% | 30% | 10% | 50% | MEDIUM |
| Workflow Execution | 70% | 50% | 70% | 60% | 60% | 25% | 20% | 10% | 40% | MEDIUM |
| Usage Collection | 85% | 70% | 85% | 80% | 80% | 60% | 30% | 10% | 50% | LOW |
| Usage Rating | 80% | 70% | 80% | 70% | 70% | 55% | 30% | 10% | 50% | LOW |
| Promotion Management | 85% | 75% | 85% | 80% | 80% | 40% | 30% | 10% | 50% | LOW |
| Bill Generation | 80% | 70% | 80% | 70% | 70% | 55% | 30% | 10% | 50% | MEDIUM |
| Billing Cycle Mgmt | 70% | 50% | 70% | 60% | 60% | 40% | 20% | 10% | 40% | MEDIUM |
| Tax Management | 70% | 50% | 70% | 60% | 50% | 30% | 20% | 10% | 40% | MEDIUM |
| Invoice Management | 85% | 80% | 85% | 80% | 80% | 70% | 40% | 30% | 60% | LOW |
| Credit Note Mgmt | 70% | 60% | 70% | 60% | 60% | 40% | 20% | 10% | 50% | LOW |
| Invoice Disputes | 80% | 70% | 80% | 70% | 70% | 50% | 30% | 10% | 50% | LOW |
| Payment Processing | 80% | 70% | 80% | 70% | 70% | 75% | 10% | 20% | 50% | HIGH |
| Payment Gateways | 60% | 40% | 60% | 50% | 50% | 40% | 10% | 10% | 30% | HIGH |
| Refund Management | 75% | 60% | 75% | 70% | 70% | 60% | 10% | 10% | 40% | HIGH |
| Payment Reconciliation | 60% | 40% | 60% | 50% | 40% | 30% | 10% | 10% | 30% | HIGH |
| Collection Management | 80% | 70% | 80% | 70% | 70% | 40% | 20% | 10% | 50% | LOW |
| Payment Arrangements | 75% | 60% | 75% | 60% | 60% | 30% | 20% | 10% | 50% | LOW |
| Dunning Management | 60% | 40% | 60% | 50% | 50% | 20% | 10% | 0% | 40% | LOW |
| Ticketing | 85% | 80% | 85% | 80% | 80% | 40% | 30% | 10% | 60% | LOW |
| SLA Management | 80% | 70% | 80% | 70% | 70% | 30% | 20% | 10% | 50% | LOW |
| Notifications | 85% | 80% | 85% | 80% | 80% | 50% | 10% | 20% | 50% | MEDIUM |
| Notification Templates | 80% | 70% | 80% | 70% | 70% | 30% | 10% | 10% | 50% | LOW |
| Audit Logging | 90% | 80% | 90% | 80% | 80% | 70% | 60% | 30% | 70% | LOW |
| Audit Alerts | 70% | 50% | 70% | 50% | 40% | 50% | 30% | 10% | 50% | LOW |
| Reporting | 80% | 70% | 80% | 70% | 70% | 30% | 20% | 10% | 50% | LOW |
| Dashboard | 70% | 60% | 70% | 60% | 60% | 20% | 20% | 10% | 40% | LOW |
| API Gateway | 85% | 70% | 85% | 80% | 80% | 40% | 50% | 10% | 60% | LOW |
| API Key Management | 85% | 70% | 85% | 80% | 80% | 30% | 50% | 10% | 50% | LOW |
| Rate Limiting | 60% | 20% | 60% | 50% | 50% | 20% | 30% | 10% | 30% | MEDIUM |
| Event Subscription | 70% | 0% | 70% | 60% | 50% | 45% | 30% | 0% | 40% | HIGH |
| Webhook Dispatch | 60% | 0% | 60% | 50% | 50% | 30% | 30% | 0% | 40% | HIGH |

---

## 5. TM FORUM MAPPING MATRIX

### 5.1 Full TMF API Mapping

| TMF API | Name | Version | Module | Score | Status | Missing Resources | Missing Operations |
|---------|------|---------|--------|-------|--------|-------------------|-------------------|
| TMF620 | Product Catalog | 5.x | ProductCatalog | 90% | Minor Gaps | Attachment, Constraint, MarketSegment | batch, events |
| TMF622 | Product Ordering | 5.x | Orders | 85% | Minor Gaps | CancelOrder task, Milestone | informationRequired, events |
| TMF629 | Customer Mgmt | 5.x | CRM | 75% | Partial | CustomerAccount, ShoppingCart, Loyalty | merge, events |
| TMF632 | Party Mgmt | 5.x | IAM | 40% | Major Gaps | Individual address, Organization, Identification, ExternalId | merge, split, events |
| TMF633 | Service Catalog | 5.x | ServiceCatalog | 85% | Minor Gaps | ResourceSpecRef, FeatureSpec | events |
| TMF637 | Product Inventory | 5.x | Subscriptions | 50% | Major Gaps | Product resource, ProductRelationship, ProductTerm | lifecycle events |
| TMF638 | Service Inventory | 5.x | ServiceInventory | 80% | Partial | ServiceCharacteristic, ServicePrice, SupportingService | events |
| TMF639 | Resource Inventory | 5.x | NetworkInventory, NumberInventory | 45% | Major Gaps | Resource base entity, ResourceSpec, Admin/Oper/Usage states | events |
| TMF641 | Service Ordering | 5.x | Provisioning | 30% | Major Gaps | ServiceOrder resource, lifecycle, CancelServiceOrder | all operations |
| TMF645 | Service Qual | 5.x | — | 0% | Not Impl | All resources | all operations |
| TMF648 | Quote Mgmt | 5.x | — | 0% | Not Impl | All resources | all operations |
| TMF651 | Agreement | 5.x | CRM (partial) | 15% | Major Gaps | AgreementSpecification, full lifecycle | events |
| TMF666 | Account Mgmt | 5.x | Billing | 55% | Partial | BillingAccount, SettlementAccount, Balance | lifecycle events |
| TMF669 | Party Role | 5.x | IAM | 60% | Partial | Engagement, Characteristic, RelatedParty | events |
| TMF676 | Payment Mgmt | 5.x | Payments | 75% | Partial | PaymentMethod, PaymentCard, BankAccount | events |
| TMF677 | Usage Consumption | 5.x | Rating | 60% | Partial | UsageSpecification, UsageConsumptionReport, Bucket | events |
| TMF678 | Customer Bill | 5.x | Invoices | 70% | Partial | BillItem, BillFormat, BillStructure | events |
| TMF681 | Communication | 5.x | Notifications | 50% | Major Gaps | CommunicationMessage, Sender/Receiver, Flow | delivery tracking |
| TMF688 | Event Mgmt | 5.x | EventManagement | 45% | Major Gaps | EventSubscription query/filter, Hub, Retry | all operations |
| TMF701 | Process Flow | 5.x | Workflow | 35% | Major Gaps | ProcessFlowSpecification, FlowGraph, parallel | events |
| TMF702 | Task Mgmt | 5.x | Ticketing | 40% | Major Gaps | Task resource, Relationship, Characteristic | events |

### 5.2 TMF Compliance by Module

**FULLY COMPLIANT (85-100%):** None
**MINOR GAPS (70-84%):** ProductCatalog (90%), Orders (85%), ServiceCatalog (85%)
**PARTIAL (50-69%):** ServiceInventory (80%), CRM (75%), Payments (75%), Invoices (70%)
**MAJOR GAPS (25-49%):** Rating (60%), Billing (55%), IAM Party Role (60%), Collections (40%), Subscriptions (50%), EventManagement (45%), NumberInventory (40%), NetworkInventory (45%), Ticketing (40%), Notifications (50%)
**NOT IMPLEMENTED:** Quote (0%), Service Qualification (0%), Workflow (35%)
**CRITICAL GAPS (<25%):** Provisioning (30%), IAM Party Management (40%), IAM Party Role (40%), Workflow (35%), Ticketing (40%)

---

## 6. COMMERCIAL SERVICE TRACEABILITY MATRIX

### 6.1 Service Lifecycle Trace

| Service Type | Order→Provision→Activate→Inventory→Sub→Rate→Bill→Invoice→Pay |
|-------------|---------------------------------------------------------------|
| **FTTH** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **ADSL** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Super Shamel** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Yemen WiFi** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Fixed Wireless** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **4G Home** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Fixed Telephone** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Static IP** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **DIA** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Ethernet** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **PRI** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **ATM** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Dedicated Servers** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **VPS** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Colocation** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Web Hosting** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Domain Registration** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |
| **Managed Services** | Orders → Provisioning → ServiceInventory → Subscriptions → Rating → Billing → Invoices → Payments |

All 18 services trace through the same end-to-end flow. No service-specific divergences exist in the current implementation.

### 6.2 Missing Traceability Elements

| Lifecycle Step | Gaps |
|---------------|------|
| **Pre-Order (Quote + Qualification)** | Entirely missing — no quote or service qualification |
| **Order Capture** | Complete — but no cart/negotiation |
| **Provisioning** | MOCK — no real device/network integration |
| **Activation** | MOCK — no real OLT/BNG/Radius/DHCP integration |
| **Service Inventory** | Complete — but no real network polling to verify |
| **Subscriptions** | Complete — but `Subscriptions` should reference `Product` instances |
| **Rating** | Complete — but no real usage collection from network |
| **Billing** | Complete — but no external tax engine, no PDF billing |
| **Invoices** | Complete — but PDF generation is template-based, not real |
| **Payments** | MOCK — all gateways return hardcoded success |
| **Collections** | Complete — but no automated dunning letters (mock email) |

### 6.3 Service-Specific Validation

| Service | Required Network Integration | Status |
|---------|---------------------------|--------|
| FTTH | OLT provisioning, ONT activation, Radius | NOT INTEGRATED |
| ADSL | DSLAM, Radius, PPPoE | NOT INTEGRATED |
| Super Shamel | Custom platform | NOT INTEGRATED |
| Yemen WiFi | WiFi controller, Radius | NOT INTEGRATED |
| Fixed Wireless | CPE management, Radio | NOT INTEGRATED |
| 4G Home | HSS/HLR, PGW | NOT INTEGRATED |
| Fixed Telephone | Softswitch, SBC, SIP | NOT INTEGRATED |
| Static IP | IPAM, BGP, Router config | NOT INTEGRATED |
| DIA | BNG, Router config, QoS | NOT INTEGRATED |
| Ethernet | Switch config, VLAN | NOT INTEGRATED |
| PRI | PBX, E1/T1 signaling | NOT INTEGRATED |
| ATM | ATM switch | NOT INTEGRATED |
| Dedicated Servers | IPMI, PXE, provisioning scripts | NOT INTEGRATED |
| VPS | Hypervisor API (Proxmox/VMware) | NOT INTEGRATED |
| Colocation | DCIM, power monitoring | NOT INTEGRATED |
| Web Hosting | cPanel/Plesk API | NOT INTEGRATED |
| Domain Registration | EPP API (registry) | NOT INTEGRATED |
| Managed Services | RMM, monitoring | NOT INTEGRATED |

---

## 7. CROSS-MODULE DEPENDENCY GRAPH

### 7.1 Dependency Structure

```
Layer 0: Shared Kernel
  └── Obss.SharedKernel (Entity, AggregateRoot, ValueObject, DomainEvent, IntegrationEvent, Repository, UnitOfWork)

Layer 1: Foundation
  ├── IAM
  ├── Audit
  ├── EventManagement
  └── ApiGateway

Layer 2: Catalog
  ├── ProductCatalog
  └── ServiceCatalog

Layer 3: Customer
  └── CRM

Layer 4: Ordering
  ├── Orders ──► ProductCatalog (product/offer validation)
  ├── Subscriptions ──► ProductCatalog (offer reference)
  └── NumberInventory

Layer 5: Service & Network
  ├── ServiceInventory ──► Subscriptions, CRM (customer/subscription refs)
  ├── NetworkInventory
  └── Provisioning ──► Workflow (DIRECT COMMAND COUPLING - VIOLATION)

Layer 6: Workflow
  └── Workflow

Layer 7: Rating
  └── Rating ──► Subscriptions (usage by subscription)

Layer 8: Billing
  ├── Billing
  ├── Invoices ──► Billing (consumes BillFinalizedIntegrationEvent)
  ├── Payments
  └── Collections

Layer 9: Assurance
  ├── Ticketing
  ├── Notifications
  └── Reporting

Layer 10: Cross-Cutting
  └── (none at this time - no module depends on the cross-cutting layer)
```

### 7.2 Known Dependency Violations

1. **Provisioning → Workflow (DIRECT COMPILE-TIME DEPENDENCY)**
   - `CreateProvisioningJobCommandHandler.cs` imports `Obss.Workflow.Application.Abstractions` and sends `StartWorkflowInstanceCommand` via MediatR
   - Violates: Module isolation, aggregate boundary, transaction boundary
   - Fix: Replace with integration event (e.g., `WorkflowRequiredIntegrationEvent`)

2. **Orders → ProductCatalog, CRM (COMPILE-TIME DEPENDENCY via .csproj)**
   - Order creation validates product/offer existence and customer existence
   - Acceptable for modular monolith, but should use application service layer, not direct domain references

3. **Invoices → Billing (EVENT-DRIVEN - CORRECT)**
   - Consumes `BillFinalizedIntegrationEvent` via RabbitMQ
   - This is the correct pattern

### 7.3 Integration Event Flow

```
                    ┌─────────────┐
                    │   Orders    │
                    └──────┬──────┘
                           │
              ┌────────────┼────────────┐
              ▼            ▼            ▼
        ┌──────────┐ ┌──────────┐ ┌──────────┐
        │Provision │ │Subscript │ │ (future) │
        │ -ing     │ │ -ions    │ │  Rating? │
        └────┬─────┘ └────┬─────┘ └──────────┘
             │            │
             ▼            ▼
      ┌──────────┐  ┌──────────┐
      │Service   │  │  Rating  │
      │Inventory │  └────┬─────┘
      └──────────┘       │
                         ▼
                    ┌──────────┐
                    │ Billing  │
                    └────┬─────┘
                         │ (BillFinalized)
                         ▼
                    ┌──────────┐
                    │ Invoices │
                    ├──────────┤
                    │ Payments │
                    └──────────┘
```

---

## 8. DDD VALIDATION REPORT

### 8.1 Summary of Violations

| # | Violation | Severity | Location | Fix |
|---|-----------|----------|----------|-----|
| 1 | Cross-module command invocation | CRITICAL | Provisioning→Workflow | Replace with integration event |
| 2 | UnitOfWork spans separate databases | CRITICAL | SharedKernel | Saga pattern or single DB |
| 3 | Entity with Repository (not AggregateRoot) | HIGH | Category, SubscriptionEntitlement, WebhookEvent | Promote to AggregateRoot |
| 4 | Over-granular aggregate roots | MEDIUM | Audit (AuditPolicy, AuditAlertRule), NetworkInventory (TopologyMap), Workflow (WorkflowMetric) | Demote to Entity |
| 5 | Dual aggregate root (CRM) | MEDIUM | Customer ← Individual/Organization | Make Individual/Org entities |
| 6 | Naming collision (SlaDefinition) | LOW | Ticketing, Workflow | Rename or namespace clearly |
| 7 | Domain event outside events folder | LOW | Invoices | Move to Events/ |
| 8 | Missing inheritance (OLT ← NetworkElement) | MEDIUM | NetworkInventory | Add inheritance |
| 9 | Plain class (RatingTier) | LOW | Rating | Extend Entity/ValueObject |
| 10 | TransactionBehavior disabled | MEDIUM | Host | Re-enable or remove explicitly |

### 8.2 Aggregate Root Inventory

| Module | AggregateRoot(s) | Child Entities | Correct? |
|--------|-----------------|----------------|----------|
| IAM | User, Tenant | Role (but has repo), UserRole, Permission (has repo) | ⚠️ Role/Permission have repos |
| CRM | Customer, Individual, Organization, CustomerSegment, Agreement | Contact, CustomerNote, CreditProfile, IdentityDocument | ⚠️ Dual root (Customer+Individual) |
| ProductCatalog | Catalog, Product, ProductSpecification, Offer | Category (entity with repo!), OfferPricing, PriceRange, etc. | ⚠️ Category has repo |
| Orders | Order | OrderItem, OrderPayment | ✅ |
| Subscriptions | Subscription | SubscriptionAddOn, SubscriptionService, SubscriptionEntitlement (has repo!) | ⚠️ Entitlement has repo |
| Billing | Bill, BillingAccount, BillingJob, TaxRule | BillLine | ⚠️ BillingJob is not aggregate |
| Invoices | Invoice, InvoiceDispute, CreditNote | InvoiceLine, InvoicePayment, InvoiceNote | ✅ |
| Payments | Payment (aggregate) | — | ✅ |
| Collections | CollectionCase, DunningPolicy | CollectionAction, PaymentArrangement, Installment | ⚠️ Dual root (Case+Arrangement) |
| Rating | RatingRule, Promotion, UsageRecord | PromotionRule | ✅ |
| NetworkInventory | NetworkElement, OLT, Subnet, VLAN, ConnectivityLink, TopologyMap | FiberCable, PONPort, NetworkInterface | ⚠️ Over-granular |
| Provisioning | ProvisioningJob, ProvisioningTemplate | ProvisioningTask | ✅ |
| Workflow | WorkflowDefinition, WorkflowInstance, WorkflowSla, SlaDefinition, WorkflowMetric | WorkflowStep, WorkflowTaskInstance | ⚠️ Metric is not aggregate |
| Ticketing | Ticket, SlaDefinition | TicketComment, TicketAttachment | ✅ |
| Notifications | Notification, NotificationTemplate | NotificationPreference | ✅ |
| ServiceCatalog | ServiceSpecification, ServiceCategory, ServiceCandidate | ServiceSpecCharacteristic, ServiceSpecCharValue | ✅ |
| ServiceInventory | Service, ServiceTopology, ResourceDiscoveryJob | ServiceResource, TopologyLink | ✅ |
| EventManagement | EventSubscription | WebhookEvent (has repo!) | ⚠️ WebhookEvent has repo |

### 8.3 Context Mapping

| Module | Relationship | Type | Notes |
|--------|-------------|------|-------|
| IAM → All | Identity/Auth | Shared Kernel | O/S - Everyone uses IAM for auth |
| CRM → Orders | Customer data | Customer-Supplier | Orders consumes Customer data |
| ProductCatalog → Orders | Product/Offer data | Customer-Supplier | Orders validates against catalog |
| Orders → Provisioning | Order fulfillment | Event-driven | Via integration events |
| Orders → Subscriptions | Subscription creation | Event-driven | Via integration events |
| Provisioning → Workflow | Workflow execution | **Corrupting** | Direct command call (violation) |
| Subscriptions → Billing | Billing trigger | Event-driven | Via integration events |
| Billing → Invoices | Invoice creation | Event-driven | Via BillFinalizedIntegrationEvent |
| ServiceInventory → Subscriptions | Service-to-subscription | Partnership | Shares subscription reference |

### 8.4 Transaction Boundaries

Each module has its own database schema. The `UnitOfWork` iterates all contexts but provides no distributed transaction guarantee. **Each module operation should be considered its own transaction.** Cross-module operations must use the Saga pattern (eventual consistency via events), not distributed transactions.

**Current pattern (incorrect):** `UnitOfWork.SaveChangesAsync()` saves ALL modules' changes in a single call without atomicity.
**Target pattern:** Each command handler saves only its own module's context. Cross-module operations use integration events.

---

## 9. TECHNICAL DEBT INVENTORY

### 9.1 Architectural Debt

| # | Item | Impact | Priority | Risk | Effort | Resolution |
|---|------|--------|----------|------|--------|------------|
| AD-1 | Subscriptions should be Product Inventory | Prevents TMF637 compliance; blocks service lifecycle | HIGH | HIGH | 6-8 weeks | Refactor: add Product aggregate, make Subscriptions thin layer |
| AD-2 | Provisioning→Workflow direct command coupling | Breaks module isolation; unsafe transactions | CRITICAL | HIGH | 2-3 days | Replace with integration event |
| AD-3 | UnitOfWork spans separate databases | No atomicity across modules | CRITICAL | HIGH | 1-2 weeks | Remove cross-DB UoW; add saga pattern |
| AD-4 | Missing Quote and Service Qualification modules | Blocks pre-order flow | HIGH | HIGH | 4-6 weeks | New modules (TMF648 + TMF645) |
| AD-5 | Missing SignalR/WebSocket bridge | No real-time frontend updates | MEDIUM | MEDIUM | 2-3 weeks | Add real-time event bridge |
| AD-6 | Over-granular aggregate roots in Audit/Network/Workflow | DDD inconsistency | LOW | LOW | 1 week | Demote to Entity |
| AD-7 | Entity with repository (Category, WebhookEvent) | DDD contradiction | MEDIUM | LOW | 2-3 days | Promote to AggregateRoot |
| AD-8 | No distributed saga for order→provision→subscription flow | No rollback on failure | HIGH | HIGH | 3-4 weeks | Implement saga pattern |

### 9.2 Backend Debt

| # | Item | Impact | Priority | Risk | Effort |
|---|------|--------|----------|------|--------|
| BD-1 | TransactionBehavior disabled in Program.cs | Inconsistent transaction handling | MEDIUM | MEDIUM | 1 day |
| BD-2 | TenantService.LoadTenantFromDatabase() is stub | No real tenant resolution | MEDIUM | HIGH | 1 week |
| BD-3 | PartyRole UpdateHandler doesn't apply field updates | Silent data loss bug | HIGH | HIGH | 1 day |
| BD-4 | OutboxProcessor uses `mandatory: false` | Messages can be silently dropped | MEDIUM | HIGH | 2 days |
| BD-5 | RabbitMQ consumer uses temporary auto-delete queue | Events lost on restart | MEDIUM | HIGH | 1 day |
| BD-6 | Integration events not persisted via outbox (direct publish) | Event loss on service crash | HIGH | CRITICAL | 1 week |
| BD-7 | All payment gateways are MOCK | Cannot process real payments | HIGH | CRITICAL | 4-6 weeks |
| BD-8 | All notification services are MOCK | Cannot send real notifications | HIGH | CRITICAL | 2-3 weeks |
| BD-9 | All provisioning is simulated with Task.Delay | Cannot provision services | HIGH | CRITICAL | 6-8 weeks |
| BD-10 | All network integration is absent | Cannot manage real network | HIGH | CRITICAL | 8-12 weeks |

### 9.3 Frontend Debt

| # | Item | Impact | Priority | Risk | Effort |
|---|------|--------|----------|------|--------|
| FD-1 | Generated client stale for 6 modules | Silent contract drift | HIGH | MEDIUM | 1 week |
| FD-2 | No EventManagement pages | Cannot manage subscriptions | HIGH | HIGH | 1-2 weeks |
| FD-3 | No real-time updates (polling only) | Stale data in UI | MEDIUM | MEDIUM | 2-3 weeks |
| FD-4 | Enum values not validated against backend | Silent drift | MEDIUM | MEDIUM | 1 week |
| FD-5 | Error handling not uniform (ProblemDetails parsing) | Poor UX on errors | MEDIUM | LOW | 1 week |

### 9.4 Security Debt

| # | Item | Impact | Priority | Risk | Effort |
|---|------|--------|----------|------|--------|
| SD-1 | JWT ValidateIssuer = false | Token issuer spoofing | CRITICAL | CRITICAL | 1 day |
| SD-2 | JWT ValidateAudience = false | Token audience confusion | CRITICAL | CRITICAL | 1 day |
| SD-3 | Credentials in source code (appsettings.json) | Credential leak | HIGH | CRITICAL | 1 week |
| SD-4 | API key AllowedIPs never validated | IP restriction bypass | HIGH | HIGH | 2 days |
| SD-5 | Rate limiting bypassed by JWT users | No rate control for web users | MEDIUM | HIGH | 2 days |
| SD-6 | Missing Content-Security-Policy header | XSS risk | MEDIUM | MEDIUM | 1 day |
| SD-7 | Missing HSTS header | MITM risk | MEDIUM | MEDIUM | 1 day |
| SD-8 | BackgroundServiceExceptionBehavior.Ignore | Silent failures | HIGH | HIGH | 1 day |
| SD-9 | SNMP community string plaintext in DB | Credential exposure | HIGH | HIGH | 1 day |
| SD-10 | Error 500 leaks ex.Message to client | Information disclosure | MEDIUM | MEDIUM | 1 day |

### 9.5 Testing Debt

| # | Item | Impact | Priority | Risk | Effort |
|---|------|--------|----------|------|--------|
| TD-1 | 17/22 modules lack integration tests | Unknown integration quality | HIGH | HIGH | 4-6 weeks |
| TD-2 | No contract tests (Pact/consumer-driven) | Contract drift undetected | HIGH | HIGH | 2-3 weeks |
| TD-3 | No performance/load tests integrated | Unknown production behavior | MEDIUM | HIGH | 2-3 weeks |
| TD-4 | No end-to-end tests covering commercial flows | Missing flow validation | HIGH | HIGH | 3-4 weeks |
| TD-5 | Test pass/fail status never verified (no `dotnet test` run) | Unknown if tests pass | HIGH | CRITICAL | 1 day |

### 9.6 Operational Debt

| # | Item | Impact | Priority | Risk | Effort |
|---|------|--------|----------|------|--------|
| OD-1 | No graceful shutdown handling | Connection drops on deploy | MEDIUM | MEDIUM | 1 week |
| OD-2 | No business metrics (customers, orders, revenue) | Blind to business health | MEDIUM | MEDIUM | 2 weeks |
| OD-3 | No queue depth metrics (RabbitMQ) | Blind to message backlog | MEDIUM | MEDIUM | 1 week |
| OD-4 | No database health check in /health | Blind to DB connectivity | HIGH | HIGH | 1 day |
| OD-5 | No RabbitMQ health check | Blind to message broker | MEDIUM | HIGH | 1 day |
| OD-6 | Missing Runtime instrumentation (GC/CPU/memory) | No runtime perf visibility | MEDIUM | MEDIUM | 1 day |

---

## 10. GAP ANALYSIS MATRIX

### 10.1 Consolidated Gaps by Category

| Gap Category | Count | Critical | High | Medium | Low |
|-------------|-------|----------|------|--------|-----|
| Missing Backend | 12 | 4 | 5 | 3 | 0 |
| Missing Frontend | 8 | 1 | 4 | 3 | 0 |
| Missing Database | 3 | 1 | 1 | 1 | 0 |
| TMF Compliance | 22 | 8 | 7 | 5 | 2 |
| Integration | 14 | 7 | 4 | 3 | 0 |
| Security | 10 | 3 | 4 | 3 | 0 |
| Performance | 4 | 0 | 2 | 2 | 0 |
| Testing | 5 | 1 | 3 | 1 | 0 |
| Documentation | 3 | 0 | 1 | 2 | 0 |

### 10.2 Critical Gaps (Must fix before production)

| Gap | Module | Risk | Evidence |
|-----|--------|------|----------|
| JWT validation skips issuer/audience | IAM/Host | Token forgery | Program.cs lines 114-117: `ValidateIssuer = false, ValidateAudience = false` |
| All payment gateways are MOCK | Payments | Can't take money | All 5 gateways return `Task.FromResult` with hardcoded success |
| All notification services are MOCK | Notifications | Can't contact customers | EmailService/SmsService log "[MOCK]" and return true |
| All provisioning is simulated | Provisioning | Can't deliver services | ProvisioningJobProcessor uses `Task.Delay(100)` |
| No network device integration | NetworkInventory | Can't manage network | No SNMP, CLI, TR-069, or any protocol implementation |
| Credentials in source code | All/Host | Credential leak | appsettings.json has postgres/redis/rabbitmq passwords |
| Subscriptions not TMF637 Product | Subscriptions | Wrong abstraction | No Product resource; Subscriptions act as Product Inventory |
| No Quote or Service Qualification | — | Missing pre-order flow | No TMF648 or TMF645 implementation |
| UnitOfWork has no transaction safety | SharedKernel | Data inconsistency | Cross-DB UoW without distributed transaction |
| Integration events not persisted via outbox | All | Event loss | `AddIntegrationEvent()` on AggregateRoot is never called |

### 10.3 Per-Module Gap Summary

| Module | Backend Gaps | Frontend Gaps | TMF Gaps | Integration Gaps | Security Gaps | Test Gaps |
|--------|-------------|--------------|----------|-----------------|---------------|-----------|
| IAM | PartyRole handler bug, no Organization | PartyRole pages | Major (TMF632) | None | JWT validation | No integration tests |
| CRM | No PATCH, no integration events | Minor | @baseType, ExternalIdentifier | No events | None | No integration tests |
| ProductCatalog | Missing Constraint, Attachment | Minor | Minor (@baseType) | No events | None | No integration tests |
| Orders | No CancelOrder task, no milestones | Minor | Minor | No events to Payments | None | Unit tests exist |
| Subscriptions | No Product resource | No Product pages | Major (TMF637) | No events to Inventory | None | No integration tests |
| ServiceCatalog | Missing ResourceSpecRef | Minor | Minor | No events | None | No integration tests |
| ServiceInventory | Missing ServiceChar, ServicePrice | Minor | Minor | No events | None | No integration tests |
| Provisioning | No ServiceOrder resource | Progress pages | Major (TMF641) | MOCK provisioning | None | No integration tests |
| Billing | No BillingAccount resource | Account pages | Major (TMF666) | No external tax | None | Integration tests exist |
| Invoices | Missing BillItem structure | Minor | Minor | PDF is template | None | Integration tests exist |
| Payments | All gateways MOCK | Gateway config | Partial | No real gateways | None | Integration tests exist |
| Collections | Missing dunning automation | Dunning pages | None | No external agency | None | Integration tests exist |
| Rating | Missing UsageSpec, Buckets | Usage dashboards | Partial | No real usage data | None | Integration tests exist |
| NetworkInventory | Missing Resource base | Capacity/alert pages | Major (TMF639) | NO network integration | SNMP plaintext | No integration tests |
| NumberInventory | Missing Resource sub-type | Port pages | Major (TMF639) | No number portability | None | No integration tests |
| Workflow | Missing parallel execution | Designer pages | Major (TMF701) | None | None | No integration tests |
| Ticketing | Missing TaskRelationship | Dashboard pages | Major (TMF702) | None | None | No integration tests |
| Notifications | All channels MOCK | Template pages | Major (TMF681) | No real email/SMS | None | No integration tests |
| Audit | Missing alert endpoints | Alert pages | None | None | None | No integration tests |
| Reporting | Widget DTO field alignment | Widget config | None | None | None | No integration tests |
| ApiGateway | No rate limit config UI | API key pages | None | None | IP whitelist bypass | No integration tests |
| EventManagement | No frontend at all | MISSING entirely | Major (TMF688) | No internal event link | None | No tests |

---

## 11. EXECUTION WAVES

### Wave 0: Foundation & Governance (Weeks 1-2)

**Objective:** Fix critical security issues, establish governance foundation, and prepare for execution.

**Dependencies:** None

**Deliverables:**
1. Fix JWT validation (enable issuer + audience validation)
2. Move credentials from appsettings.json to environment variables / user secrets
3. Fix BackgroundServiceExceptionBehavior to log+restart instead of ignore
4. Add Content-Security-Policy and HSTS headers
5. Fix API key AllowedIPs validation
6. Re-enable or remove TransactionBehavior explicitly
7. Add health checks for PostgreSQL and RabbitMQ
8. Run `dotnet test` and fix any failing tests
9. Add per-module integration test scaffolding
10. Establish Definition of Done (as defined below in Section 15)

**Acceptance Criteria:**
- JWT validates issuer and audience
- No credentials in source-controlled config
- Health checks report database and broker status
- All existing tests pass
- DoD documented and agreed

---

### Wave 1: Commercial Foundation (Weeks 3-6)

**Objective:** Fix architectural violations in core commercial flows. Establish proper module isolation and event-driven patterns.

**Dependencies:** Wave 0

**Capabilities:**
- Provisioning → Workflow coupling fix
- UnitOfWork multi-DB fix
- Category/WebhookEvent/Entitlement aggregate fix
- PartyRole handler bug fix
- Generated client regeneration (all modules)

**Deliverables:**
1. Replace Provisioning→Workflow direct command with integration event
2. Remove cross-DB UnitOfWork; make each module's UoW save only its own context
3. Add saga pattern for order→provisioning→subscription flow
4. Promote Category, WebhookEvent, SubscriptionEntitlement to AggregateRoot
5. Demote AuditPolicy, AuditAlertRule, TopologyMap, WorkflowMetric to Entity
6. Fix CRM dual aggregate root (Customer/Individual/Organization)
7. Add OLT → NetworkElement inheritance
8. Fix PartyRole UpdateHandler field application
9. Fix InvoiceCancelledDomainEvent location
10. Regenerate OpenAPI spec + generated client for ALL modules
11. Run field-level contract diff for ALL modules

**Acceptance Criteria:**
- No compile-time cross-module command dependencies remain
- Each module's UnitOfWork saves only its own context
- All aggregate roots have correct base classes
- All repository interfaces manage only aggregate roots
- Generated client matches OpenAPI spec exactly
- Contract diff shows 0 drift items for all modules

---

### Wave 2: Customer Journey & Fulfillment (Weeks 7-14)

**Objective:** Add ServiceOrder (TMF641) to Provisioning, fix Product Inventory (TMF637) in Subscriptions, implement Quote (TMF648) and Service Qualification (TMF645).

**Dependencies:** Wave 1

**Capabilities:**
- Service Order Management (TMF641)
- Product Inventory (TMF637)
- Quote Management (TMF648)
- Service Qualification (TMF645)
- Provisioning real integration layer

**Deliverables:**
1. **Provisioning TMF641:**
   - Add ServiceOrder aggregate root with TMF lifecycle
   - Add CancelServiceOrder task resource
   - Add RelatedParty, requested dates, milestones
   - Add ServiceOrderItem to Service relationship
   - Add informationRequired notifications
   - Keep existing Job/Task as execution engine beneath ServiceOrder

2. **Subscriptions → Product Inventory:**
   - Add Product aggregate root (TMF637)
   - Add ProductRelationship, ProductCharacteristic, ProductPrice
   - Make Subscription reference Product (not Offer directly)
   - Add RealizingService/RealizingResource links
   - Add Place, AgreementRef, BillingAccountRef
   - Add product lifecycle events
   - Update all queries/DTOs/mappings

3. **Quote Management (New Module - TMF648):**
   - Add Quote aggregate with QuoteItem
   - Quote lifecycle: Draft→InProgress→Accepted→Rejected→Expired
   - Product Offer references with pricing
   - Related parties, agreements
   - ValidUntil, effective dates
   - Frontend: quote creation wizard, quote list/detail

4. **Service Qualification (New Module - TMF645):**
   - Add CheckServiceQualification resource
   - Add QueryServiceQualification resource
   - Service eligibility checking against address/place
   - Alternate proposal support
   - Frontend: qualification request forms, results display

5. **Provisioning real integration layer:**
   - Add IProvisioningEngine interface
   - Implement for each service type (FTTH, ADSL, etc.)
   - Start with simulated/VNI implementations
   - Add provisioning result callbacks

**Acceptance Criteria:**
- ServiceOrder API fully TMF641 compliant (POST/GET/PATCH/DELETE)
- Product inventory replaces subscription as primary customer product record
- Subscriptions reference Product instances
- Quote API fully functional (POST/GET/PATCH)
- Service Qualification API fully functional (POST/GET)
- All new endpoints in OpenAPI spec
- Frontend pages exist for all new capabilities

---

### Wave 3: Revenue Management & Customer Experience (Weeks 15-22)

**Objective:** Fix BillingAccount (TMF666), Payment gateways (real), Notification services (real), Improve NetworkInventory TMF639, Event Management TMF688.

**Dependencies:** Wave 2

**Capabilities:**
- Billing Account Management (TMF666)
- Real Payment Processing
- Real Notification Delivery
- Resource Inventory (TMF639)
- Event Management (TMF688)
- Real-time Frontend Events

**Deliverables:**
1. **Billing TMF666:**
   - Add BillingAccount aggregate with lifecycle
   - Add AccountBalance, AccountTaxExemption
   - Add SettlementAccount, FinancialAccount
   - Add integration events for account lifecycle

2. **Payment Gateways (Real):**
   - Integrate at least one real gateway (Stripe API)
   - Add PaymentMethod management
   - Add payment tokenization support
   - Add webhook handling for gateway callbacks
   - Add payment reconciliation automation

3. **Notification Services (Real):**
   - Integrate SMTP email (MailKit/SendGrid)
   - Integrate SMS provider (Twilio/API)
   - Add push notification via Firebase
   - Add delivery tracking and retry

4. **NetworkInventory TMF639:**
   - Add Resource base entity
   - Make NetworkElement, OLT, TelephoneNumber extend Resource
   - Add AdministrativeState/OperationalState/UsageState
   - Add ResourceSpecification reference

5. **EventManagement TMF688:**
   - Connect to internal domain events via outbox
   - Add event subscription management UI
   - Add retry with exponential backoff
   - Add delivery audit log

6. **Real-Time Frontend Events:**
   - Add SignalR hub
   - Bridge key integration events to frontend
   - Order status changes, provisioning completion, notification delivery

**Acceptance Criteria:**
- BillingAccount API functional (POST/GET)
- At least one real payment gateway integrated
- Email and SMS sending functional with real providers
- Resource API TMF639 compliant
- Event subscriptions visible in frontend UI
- Real-time updates for order and provisioning status

---

### Wave 4: Production Hardening (Weeks 23-26)

**Objective:** Complete remaining TMF compliance, harden all modules, final production readiness.

**Dependencies:** Wave 3

**Capabilities:**
- Complete TMF alignment for all modules
- Comprehensive test coverage
- Performance optimization
- Security hardening
- Operational runbooks

**Deliverables:**
1. **Remaining TMF fixes:**
   - CRM TMF629: add PATCH, integration events, @baseType
   - ProductCatalog TMF620: add PriceType split, events, RelatedParty
   - Orders TMF622: add CancelOrder task, milestones, version tracking
   - ServiceCatalog/Inventory TMF633/638: add integration events
   - IAM TMF632/669: add Organization, Individual details
   - Audit/Reporting/ApiGateway: final contract alignment

2. **Testing:**
   - Integration tests for all 22 modules
   - Contract tests for all API endpoints
   - End-to-end tests covering commercial flows
   - Load tests for critical paths (order creation, bill generation)
   - Performance benchmarks

3. **Security Hardening:**
   - Remove all mock/stub code that could be called in production
   - Add rate limiting for JWT-authenticated users
   - Add input sanitization on all endpoints
   - Security audit report

4. **Operations:**
   - Graceful shutdown for all background services
   - Business metrics dashboard
   - Queue depth monitoring
   - Operational runbooks for all modules
   - Backup/recovery validation

5. **Documentation:**
   - API documentation complete
   - Deployment guide
   - Operations guide
   - Disaster recovery plan

**Acceptance Criteria:**
- All modules have TMF compliance score >70%
- All modules have contract alignment >90%
- Integration tests pass for all modules
- Load test: 1000 concurrent users, P50 < 100ms
- Security audit passes
- All runbooks validated
- Production Readiness score >90%

---

## 12. MASTER PRIORITIZED BACKLOG

### 12.1 Work Package Inventory

| ID | Work Package | Wave | Business Value | Priority | Risk | Complexity | Duration | Est. Effort (dev-weeks) |
|----|-------------|------|---------------|----------|------|------------|----------|------------------------|
| WP-001 | Fix JWT validation (issuer + audience) | W0 | CRITICAL | P0 | CRITICAL | S | 1 day | 0.2 |
| WP-002 | Move credentials to env vars/user secrets | W0 | CRITICAL | P0 | CRITICAL | S | 2 days | 0.4 |
| WP-003 | Fix BackgroundServiceExceptionBehavior | W0 | HIGH | P0 | HIGH | S | 1 day | 0.2 |
| WP-004 | Add security headers (CSP, HSTS) | W0 | MEDIUM | P0 | MEDIUM | S | 1 day | 0.2 |
| WP-005 | Fix API key AllowedIPs validation | W0 | HIGH | P0 | HIGH | S | 1 day | 0.2 |
| WP-006 | Add health checks (PostgreSQL, RabbitMQ) | W0 | HIGH | P0 | HIGH | S | 1 day | 0.2 |
| WP-007 | Run test suite + fix failures | W0 | HIGH | P0 | MEDIUM | M | 3 days | 0.6 |
| WP-008 | Add integration test scaffolding | W0 | HIGH | P1 | MEDIUM | M | 5 days | 1.0 |
| WP-009 | Fix Provisioning→Workflow coupling | W1 | HIGH | P0 | HIGH | S | 2 days | 0.4 |
| WP-010 | Fix cross-DB UnitOfWork | W1 | HIGH | P0 | HIGH | M | 1 week | 1.0 |
| WP-011 | Add saga for order→provisioning→subscription | W1 | HIGH | P0 (deferred) | HIGH | L | 3 weeks | 3.0 |
| WP-012 | Fix aggregate root violations | W1 | MEDIUM | P1 | MEDIUM | M | 1 week | 1.0 |
| WP-013 | Fix PartyRole handler bug | W1 | HIGH | P1 | HIGH | S | 1 day | 0.2 |
| WP-014 | Fix CRM dual aggregate root | W1 | MEDIUM | P1 | MEDIUM | M | 3 days | 0.6 |
| WP-015 | Add OLT→NetworkElement inheritance | W1 | MEDIUM | P2 | MEDIUM | M | 2 days | 0.4 |
| WP-016 | Regenerate OpenAPI + client | W1 | HIGH | P0 | HIGH | M | 1 week | 1.0 |
| WP-017 | Run contract diff for all modules | W1 | HIGH | P1 | MEDIUM | L | 1 week | 1.0 |
| WP-018 | Add ServiceOrder to Provisioning (TMF641) | W2 | HIGH | P0 | HIGH | XL | 4 weeks | 4.0 |
| WP-019 | Add Product to Subscriptions (TMF637) | W2 | HIGH | P0 | VERY HIGH | XXL | 6 weeks | 6.0 |
| WP-020 | Implement Quote Management (TMF648) | W2 | HIGH | P1 | HIGH | XL | 4 weeks | 4.0 |
| WP-021 | Implement Service Qualification (TMF645) | W2 | MEDIUM | P1 | HIGH | L | 3 weeks | 3.0 |
| WP-022 | Add provisioning real integration layer | W2 | HIGH | P0 | HIGH | XL | 4 weeks | 4.0 |
| WP-023 | Add BillingAccount (TMF666) | W3 | MEDIUM | P1 | MEDIUM | L | 3 weeks | 3.0 |
| WP-024 | Integrate real payment gateway (Stripe) | W3 | HIGH | P0 | HIGH | L | 3 weeks | 3.0 |
| WP-025 | Integrate real email (SMTP/MailKit) | W3 | MEDIUM | P1 | MEDIUM | M | 1 week | 1.0 |
| WP-026 | Integrate real SMS (Twilio) | W3 | MEDIUM | P1 | MEDIUM | M | 1 week | 1.0 |
| WP-027 | Add push notifications (Firebase) | W3 | LOW | P2 | MEDIUM | M | 1 week | 1.0 |
| WP-028 | Add Resource base entity (TMF639) | W3 | MEDIUM | P1 | MEDIUM | L | 2 weeks | 2.0 |
| WP-029 | Connect EventManagement to internal events | W3 | MEDIUM | P1 | MEDIUM | L | 2 weeks | 2.0 |
| WP-030 | Add EventManagement frontend UI | W3 | MEDIUM | P1 | MEDIUM | M | 1 week | 1.0 |
| WP-031 | Add SignalR real-time events bridge | W3 | MEDIUM | P1 | MEDIUM | L | 2 weeks | 2.0 |
| WP-032 | Remaining TMF compliance fixes | W4 | MEDIUM | P2 | MEDIUM | L | 3 weeks | 3.0 |
| WP-033 | Comprehensive integration tests | W4 | HIGH | P1 | MEDIUM | XL | 4 weeks | 4.0 |
| WP-034 | Contract tests (Pact) | W4 | MEDIUM | P2 | MEDIUM | L | 2 weeks | 2.0 |
| WP-035 | End-to-end tests | W4 | HIGH | P1 | MEDIUM | L | 2 weeks | 2.0 |
| WP-036 | Load/performance tests | W4 | MEDIUM | P2 | LOW | M | 1 week | 1.0 |
| WP-037 | Security audit + hardening | W4 | HIGH | P1 | HIGH | L | 2 weeks | 2.0 |
| WP-038 | Operations hardening | W4 | MEDIUM | P2 | MEDIUM | M | 1 week | 1.0 |
| WP-039 | Documentation completion | W4 | MEDIUM | P3 | LOW | M | 1 week | 1.0 |
| WP-040 | Production readiness certification | W4 | HIGH | P0 | HIGH | M | 1 week | 1.0 |
| WP-041 | Saga for order→provisioning→subscription (revisit) | W1 | HIGH | P1 (revisit) | HIGH | L | 3 weeks | 3.0 |

---

## 13. PRODUCTION READINESS MATRIX

### 13.1 Readiness Gates

A capability is PRODUCTION READY only when ALL gates pass:

| Gate | Description | Verification |
|------|-------------|-------------|
| **Backend** | All backend code implemented and unit tested | Code review, unit tests pass |
| **Frontend** | All frontend pages/forms/hooks implemented | UI review, component tests |
| **Database** | All migrations applied, no drift between model and DB | `dotnet ef migrations has-done` |
| **OpenAPI** | All endpoints documented in OpenAPI spec | OpenAPI spec completeness check |
| **Generated Client** | Frontend client matches OpenAPI spec | No drift in generated files |
| **TMF Alignment** | Meets TMF compliance target for the module | TMF checklist passed |
| **Integration** | All cross-module integrations work end-to-end | Integration tests pass |
| **Security** | AuthN/AuthZ correct, no known vulnerabilities | Security scan + review |
| **Performance** | Meets latency/throughput targets | Load test results |
| **Testing** | Unit + integration + contract tests all pass | Test suite green |
| **Documentation** | API docs, deployment guide, ops guide exist | Doc review |
| **Monitoring** | Logs, metrics, traces, alerts configured | OTel dashboards exist |
| **Backup/Recovery** | Backup strategy documented and tested | DR runbook validated |
| **CI/CD** | Automated build, test, deploy pipeline | CI green |

### 13.2 Current Readiness by Module

| Module | Backend | Frontend | DB | OpenAPI | Client | TMF | Integ | Security | Perf | Tests | Docs | Monitor | B/R | CI/CD | **Score** |
|--------|---------|----------|----|---------|--------|-----|-------|----------|------|-------|------|---------|-----|-------|-----------|
| IAM | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ❌ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 50% |
| CRM | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 64% |
| ProductCatalog | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 71% |
| Orders | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 79% |
| Subscriptions | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 57% |
| ServiceCatalog | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 64% |
| ServiceInventory | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 64% |
| Provisioning | ✅ | ⚠️ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 36% |
| Billing | ✅ | ⚠️ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 50% |
| Invoices | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 64% |
| Payments | ✅ | ⚠️ | ✅ | ✅ | ⚠️ | ⚠️ | ❌ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 43% |
| Collections | ✅ | ⚠️ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 57% |
| Rating | ✅ | ⚠️ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ❓ | ⚠️ | ✅ | ✅ | ❓ | ✅ | 57% |
| NetworkInventory | ✅ | ⚠️ | ✅ | ⚠️ | ⚠️ | ❌ | ❌ | ❌ | ❓ | ❌ | ⚠️ | ✅ | ❓ | ✅ | 29% |
| NumberInventory | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 57% |
| Workflow | ✅ | ⚠️ | ✅ | ✅ | ✅ | ❌ | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 50% |
| Ticketing | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 57% |
| Notifications | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ❌ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 50% |
| Audit | ✅ | ⚠️ | ✅ | ⚠️ | ⚠️ | — | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 43% |
| Reporting | ✅ | ⚠️ | ✅ | ✅ | ✅ | — | ⚠️ | ✅ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 50% |
| ApiGateway | ✅ | ⚠️ | ✅ | ✅ | ✅ | — | ⚠️ | ❌ | ❓ | ❌ | ✅ | ✅ | ❓ | ✅ | 43% |
| EventManagement | ✅ | ❌ | ✅ | ⚠️ | ⚠️ | ❌ | ⚠️ | ⚠️ | ❓ | ❌ | ⚠️ | ✅ | ❓ | ✅ | 21% |

**Overall Production Readiness: ~50%**

Legend: ✅=Complete, ⚠️=Partial, ❌=Missing, ❓=Unknown

---

## 14. RECOMMENDED EXECUTION ORDER

### 14.1 Priority Queue (strict ordering)

```
P0 (CRITICAL - must fix before any production usage)
├── WP-001: Fix JWT validation
├── WP-002: Move credentials to env vars
├── WP-003: Fix BackgroundServiceExceptionBehavior
├── WP-006: Add health checks
├── WP-009: Fix Provisioning→Workflow coupling
├── WP-010: Fix cross-DB UnitOfWork
├── WP-016: Regenerate OpenAPI + client
├── WP-018: Add ServiceOrder to Provisioning (TMF641)
├── WP-019: Add Product to Subscriptions (TMF637)
├── WP-022: Add provisioning real integration layer
├── WP-024: Integrate real payment gateway
├── WP-040: Production readiness certification

P1 (HIGH - required for commercial release)
├── WP-008: Integration test scaffolding
├── WP-011: Saga for order→provisioning→subscription
├── WP-012: Fix aggregate root violations
├── WP-013: Fix PartyRole handler bug
├── WP-017: Run contract diff
├── WP-020: Implement Quote Management (TMF648)
├── WP-021: Implement Service Qualification (TMF645)
├── WP-023: Add BillingAccount (TMF666)
├── WP-025: Integrate real email
├── WP-026: Integrate real SMS
├── WP-028: Add Resource base entity (TMF639)
├── WP-029: Connect EventManagement to internal events
├── WP-030: Add EventManagement frontend
├── WP-031: Add SignalR bridge
├── WP-033: Comprehensive integration tests
├── WP-035: End-to-end tests
├── WP-037: Security audit + hardening

P2 (MEDIUM - important but can be deferred)
├── WP-005: Fix API key AllowedIPs
├── WP-014: Fix CRM dual aggregate root
├── WP-015: Add OLT→NetworkElement inheritance
├── WP-027: Add push notifications
├── WP-032: Remaining TMF compliance
├── WP-034: Contract tests
├── WP-036: Load/performance tests
├── WP-038: Operations hardening

P3 (LOW - nice to have)
├── WP-004: Add security headers
├── WP-007: Run test suite + fix
├── WP-039: Documentation completion
```

### 14.2 Actual Execution Schedule

```
### 14.3 Actual Execution History

```
Week 1:  WP-001, WP-002, WP-003, WP-004, WP-005, WP-006, WP-007
Week 2:  WP-008, WP-009, WP-010, WP-012, WP-013
Week 3:  WP-011 (NOT EXECUTED), WP-014, WP-015, WP-016, WP-017
Week 4:  WP-018 (start)
Week 5:  WP-018 (continue)
Week 6:  WP-018 (continue), WP-019 (design + spec)
```

### 14.4 Revised Forward Schedule

```
Week 7:  WP-018 (completed), WP-019 (implementation — subagent-driven)
Week 8:  WP-019 (continue)
Week 9:  WP-019 (finish), WP-020 (start)
Week 10: WP-020 (continue), WP-021 (start)
Week 11: WP-020 (continue), WP-021 (continue)
Week 12: WP-011 (saga — revisit), WP-022 (start), WP-023 (start)
Week 13: WP-022 (continue), WP-023 (continue)
Week 14: WP-022 (continue), WP-024 (start)
Week 15: WP-024 (continue), WP-025, WP-026
Week 16: WP-024 (continue), WP-027, WP-028
Week 17: WP-028 (continue), WP-029
Week 18: WP-029 (continue), WP-030, WP-031
Week 19: WP-031 (continue), WP-032
Week 20: WP-032 (continue), WP-033 (start)
Week 21: WP-033 (continue), WP-034 (start)
Week 22: WP-033 (continue), WP-034 (continue)
Week 23: WP-035, WP-036
Week 24: WP-037, WP-038
Week 25: WP-039, WP-040
Week 26: WP-040 (continue), Final certification
```
```

---

## 15. DEFINITION OF REPOSITORY COMPLETION

The repository is considered PRODUCTION COMPLETE when ALL of the following conditions are met:

### 15.1 Functional Completeness

- [ ] All 22 modules have complete backend + frontend + database implementation
- [ ] EventManagement has frontend pages
- [ ] Quote Management (TMF648) implemented with frontend
- [ ] Service Qualification (TMF645) implemented with frontend
- [ ] Real-time frontend events via SignalR/WebSocket bridge

### 15.2 TMF Compliance

- [ ] ProductCatalog ≥ 90% TMF620 compliance
- [ ] Orders ≥ 90% TMF622 compliance
- [ ] CRM ≥ 80% TMF629 compliance
- [ ] IAM ≥ 80% TMF632 + TMF669 compliance
- [ ] ServiceCatalog ≥ 90% TMF633 compliance
- [ ] Subscriptions ≥ 80% TMF637 compliance (as Product Inventory)
- [ ] ServiceInventory ≥ 85% TMF638 compliance
- [ ] NetworkInventory ≥ 70% TMF639 compliance
- [ ] Provisioning ≥ 80% TMF641 compliance (with ServiceOrder)
- [ ] Quote ≥ 85% TMF648 compliance
- [ ] Service Qualification ≥ 85% TMF645 compliance
- [ ] Billing ≥ 75% TMF666 compliance
- [ ] Payments ≥ 85% TMF676 compliance
- [ ] Rating ≥ 75% TMF677 compliance
- [ ] Invoices ≥ 80% TMF678 compliance
- [ ] Notifications ≥ 70% TMF681 compliance
- [ ] EventManagement ≥ 75% TMF688 compliance
- [ ] Workflow ≥ 70% TMF701 compliance
- [ ] Ticketing ≥ 70% TMF702 compliance

### 15.3 Contract Alignment

- [ ] All modules have Field-Level Contract Alignment Score ≥ 90%
- [ ] OpenAPI spec is current and complete for all endpoints
- [ ] Generated client (dto.ts + types.ts) matches OpenAPI exactly
- [ ] All frontend hooks use generated types (no hand-typed payloads)
- [ ] Enum values match between backend and frontend for all modules
- [ ] Validation rules are duplicated correctly (FluentValidation ↔ Zod)
- [ ] Error responses follow consistent format (ProblemDetails) and are parsed by frontend

### 15.4 Production Readiness

- [ ] All external integrations replaced with real implementations (not MOCK):
  - [ ] At least one real payment gateway integrated and tested
  - [ ] Email sending via real SMTP provider
  - [ ] SMS sending via real provider
  - [ ] Provisioning engine with real service type implementations
- [ ] All security gates pass:
  - [ ] JWT validates issuer + audience
  - [ ] No credentials in source code
  - [ ] Rate limiting applies to ALL users (not just API key)
  - [ ] Content-Security-Policy, HSTS headers configured
  - [ ] All background services have exception handling with restart
  - [ ] No MOCK/stub code present in production paths
- [ ] All testing gates pass:
  - [ ] All 22 modules have integration tests passing
  - [ ] Contract tests pass for all API endpoints
  - [ ] End-to-end tests cover all 18 commercial service types
  - [ ] Load test: 1000 concurrent users, P50 < 100ms, P99 < 500ms
  - [ ] Performance benchmarks within target
- [ ] All operations gates pass:
  - [ ] Health checks: PostgreSQL, Redis, RabbitMQ, Keycloak
  - [ ] Metrics: business metrics (customers, orders, revenue, invoices) in Grafana
  - [ ] Logging: all modules structured-logging to OpenSearch
  - [ ] Tracing: OpenTelemetry traces for all modules
  - [ ] Alerts: queue depth, error rate, latency breach
  - [ ] Backup: automated backup verified with recovery test
  - [ ] Runbooks: deployment, operations, disaster recovery all validated

### 15.5 Architecture Compliance

- [ ] No compile-time cross-module command dependencies (event-driven only)
- [ ] Each module saves only its own database context
- [ ] Cross-module transactions use saga pattern
- [ ] All aggregate roots are correctly modeled (no entity-with-repository, no dual roots)
- [ ] Integration events use outbox pattern (not direct publish)
- [ ] RabbitMQ queues are durable (not auto-delete)
- [ ] OutboxProcessor uses `mandatory: true` with dead letter handling

---

## 16. FIRST IMPLEMENTATION PROMPT

### Wave 0, Package 1: Security Foundation Fixes

**Target Capabilities:** Authentication / Authorization / Security Infrastructure

**Objectives:**
1. Fix JWT token validation to verify issuer and audience
2. Remove hardcoded credentials from `appsettings.json`
3. Fix `BackgroundServiceExceptionBehavior` to properly handle failures
4. Add security headers (CSP, HSTS)
5. Fix API key AllowedIPs validation
6. Add health checks for PostgreSQL and RabbitMQ

**Repository Scope:**
- `/home/ubuntu/obss/src/Host/Obss.Host/Program.cs`
- `/home/ubuntu/obss/src/Host/Obss.Host/appsettings.json`
- `/home/ubuntu/obss/src/Host/Obss.Host/appsettings.Development.json`
- `/home/ubuntu/obss/src/Host/Obss.Host/Middleware/`
- `/home/ubuntu/obss/src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/Services/ApiKeyValidator.cs`
- `/home/ubuntu/obss/src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/Services/RateLimiter.cs`
- `/home/ubuntu/obss/src/Shared/Obss.SharedKernel/Infrastructure/DependencyInjection.cs`

**Backend Tasks:**

1. **JWT Validation Fix:**
   - In `Program.cs`: Set `ValidateIssuer = true` and `ValidateAudience = true`
   - Add `ValidIssuer` and `ValidAudience` to `TokenValidationParameters`
   - Configure IssuerSigningKeys from Keycloak JWKS endpoint
   - Add `RequireHttpsMetadata = true` for non-development environments

2. **Credential Management:**
   - Remove `Password` fields from `appsettings.json` connection strings
   - Add connection strings with `{ENV_VARIABLE_NAME}` placeholders
   - Create `appsettings.Production.json` with env-var-based config
   - Document required environment variables in `docs/operations/environment-variables.md`

3. **BackgroundServiceExceptionBehavior:**
   - Change from `Ignore` to `StopHost` (default)
   - Add retry logic in each background service (OutboxProcessor, RabbitMqConsumerService, etc.)
   - Add health check contribution from each background service

4. **Security Headers:**
   - Add `Content-Security-Policy: default-src 'self'` middleware
   - Add `Strict-Transport-Security: max-age=31536000; includeSubDomains` middleware
   - Add `Permissions-Policy: camera=(), microphone=(), geolocation=()` middleware
   - Make CSP configurable via `appsettings.json`

5. **API Key AllowedIPs:**
   - In `ApiKeyAuthMiddleware.cs`: Extract remote IP from `HttpContext.Connection.RemoteIpAddress`
   - Call `apiKey.HasAllowedIp(ip)` method
   - Return 401 if IP not in allowed list (when list is non-empty)

6. **Health Checks:**
   - Add `AddNpgsql()` health check for PostgreSQL connectivity
   - Add `AddRabbitMQ()` health check for message broker
   - Add per-module DbContext health checks (or at minimum one per database)
   - Update `/health/detailed` to show all checks

**Testing Tasks:**
- Add unit tests for each fix
- Verify all existing tests still pass with `dotnet test`

**Acceptance Criteria:**
- JWT tokens from different issuers are rejected
- JWT tokens for different audiences are rejected
- No database passwords in `appsettings.json`
- Background services restart automatically on failure
- All security headers present in HTTP responses
- API requests from non-allowed IPs are rejected
- `/health` endpoint shows database and message broker status
- `dotnet test` passes with zero failures

**Definition of Done:**
- [ ] Code reviewed and approved
- [ ] All tests pass
- [ ] Security review completed
- [ ] Changes documented
- [ ] Environment variables documented
- [ ] Migration no-impact confirmed

---

**END OF MASTER OSS/BSS CAPABILITY ROADMAP v2.0**

This document is the SINGLE SOURCE OF TRUTH for all remaining implementation sessions.
All future work must reference this roadmap for priorities, dependencies, and completion criteria.
