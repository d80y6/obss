# Telecom OSS/BSS Platform — Master Architecture Document

**Version:** 1.0  
**Status:** Approved  
**Last Updated:** 2026-06-20  
**Owner:** Chief OSS/BSS Architect  

---

## Table of Contents

1. [Vision](#1-vision)
2. [Goals](#2-goals)
3. [Architecture Principles](#3-architecture-principles)
4. [Bounded Contexts](#4-bounded-contexts)
5. [Module Dependency Map](#5-module-dependency-map)
6. [Integration Strategy](#6-integration-strategy)
7. [Security Architecture](#7-security-architecture)
8. [SaaS Architecture](#8-saas-architecture)
9. [Deployment Architecture](#9-deployment-architecture)
10. [Event Architecture](#10-event-architecture)

---

## 1. Vision

### 1.1 Platform Vision

To become the definitive open-standard Telecom OSS/BSS platform that enables Communication Service Providers (CSPs) of any scale — from emerging ISP startups to established Tier-2/3 operators — to rapidly design, launch, manage, and monetize connectivity and digital services with zero vendor lock-in, enterprise-grade reliability, and SaaS-level operational efficiency.

### 1.2 Strategic Goals

| Goal | Description |
|------|-------------|
| **Unified Platform** | Replace disjointed OSS/BSS point solutions with a single, cohesive, modular platform that covers the full service lifecycle from lead to cash to care. |
| **Multi-Tenant SaaS Ready** | Architect from day zero for multi-tenant operation, enabling the platform to be offered as a managed service to multiple CSPs with strict data isolation. |
| **Product Velocity** | Enable non-technical business users to define, price, package, and launch new products in hours, not weeks — using a flexible product catalog and policy-driven rating engine. |
| **Zero-Trust Security** | Implement defense-in-depth with RBAC, tenant isolation, API security, and full auditability as non-negotiable architectural primitives. |
| **Observability by Default** | Every component exposes health, metrics, traces, and structured logs without additional instrumentation effort from developers. |
| **Event-Driven Agility** | Loosely couple all business processes through an event backbone, enabling real-time reactions to subscriber actions, network events, and billing milestones. |
| **Modular Monolith Stability** | Deploy as a modular monolith until proven scaling bottlenecks demand microservice extraction — avoiding premature distribution while preserving clean module boundaries. |

---

## 2. Goals

### 2.1 Measurable Architecture Goals

| Domain | Goal | Metric | Target |
|--------|------|--------|--------|
| **Performance** | Order-to-active fulfillment latency | P95 time from order submission to service activation | < 30 seconds |
| **Performance** | Invoice generation throughput | Time to generate 100K invoices | < 5 minutes |
| **Performance** | API response time | P95 response for read APIs | < 200ms |
| **Performance** | API response time | P95 response for write APIs | < 500ms |
| **Availability** | Platform uptime | Monthly SLA | 99.9% (excluding planned maintenance) |
| **Availability** | Recovery time | RTO for any single module | < 15 minutes |
| **Availability** | Data loss | RPO | < 1 minute |
| **Scale** | Tenant capacity | Maximum tenants per single deployment | 500 |
| **Scale** | Subscriber capacity | Maximum subscribers per tenant | 1,000,000 |
| **Scale** | Transaction volume | Peak rating events per second | 10,000 TPS |
| **Security** | Audit coverage | All state-changing operations logged | 100% |
| **Security** | Tenant isolation | Cross-tenant data leakage incidents | Zero |
| **Development** | Module independence | Number of modules deployable independently | All 19 |
| **Quality** | Test coverage | Line coverage across all modules | > 80% |

---

## 3. Architecture Principles

### 3.1 Clean Architecture

**Statement:** Each bounded context shall enforce a strict dependency inversion layering: Domain → Application → Infrastructure → Presentation. Inner layers (Domain) must have zero knowledge of outer layers (Infrastructure, Presentation).

**Rationale:** The Telecom domain is complex and long-lived. Clean Architecture ensures that business rules are testable in isolation, frameworks are swappable, and the cost of change remains bounded as the system evolves over decades.

**Enforcement:**
- Domain layer projects reference zero NuGet packages except `MediatR` and `FluentValidation`
- Application layer depends only on Domain layer
- Infrastructure layer implements interfaces defined in Application/Domain
- Architecture tests (NetArchTest) in CI enforce dependency rules
- Violations fail the build

### 3.2 Domain-Driven Design

**Statement:** Each bounded context shall model its domain using tactical DDD patterns: Aggregates, Entities, Value Objects, Domain Events, and Repository interfaces. Ubiquitous Language shall be maintained within each context and documented in a context-specific glossary.

**Rationale:** Telecom OSS/BSS is rich with domain complexity — charging, rating, provisioning, inventory reconciliation. DDD enables the software model to faithfully represent the business model, making the system comprehensible to domain experts and developers alike.

**Enforcement:**
- Aggregates are the sole transactional consistency boundary
- Repositories are interfaces in Domain, implemented in Infrastructure
- Domain Events represent meaningful business occurrences
- Value Objects are immutable, structurally equatable, and replaceable

### 3.3 Modular Monolith First

**Statement:** All bounded contexts shall initially be deployed as a single process (modular monolith) with strictly separated assemblies/modules. Extraction to microservices is a deployment-time decision, not an architecture-time decision.

**Rationale:** Most distributed system failures are caused by distribution, not by scale. A modular monolith provides the benefits of clean separation without the costs of network latency, distributed transactions, eventual consistency surprises, and operational complexity. Microservices extraction is reserved for proven bottlenecks.

**Enforcement:**
- In-process communication via MediatR (commands, queries, events)
- Each module has its own database schema (logical or physical)
- Cross-module communication uses events only — no direct DB access
- Module boundaries are enforced by architecture tests
- A module can be extracted to a standalone service by hosting its ASP.NET Core pipeline independently

### 3.4 Event-Driven Architecture

**Statement:** All cross-module communication shall occur through asynchronous events published on a message broker. Synchronous coupling between modules is forbidden except for queries that must be real-time (handled through API Gateway-mediated REST or gRPC).

**Rationale:** Loose coupling enables independent deployability, fault isolation, and eventual consistency that maps naturally to Telecom processes (order fulfillment, billing cycles, provisioning workflows).

**Enforcement:**
- Commands within a module are synchronous MediatR calls
- Events between modules are asynchronous via RabbitMQ
- Event schemas are versioned (CloudEvents 1.0 format)
- Events are immutable — new versions add fields
- No module may call another module's database directly

### 3.5 CQRS

**Statement:** Every bounded context shall separate read models from write models. Commands mutate state; Queries return data. Command handlers and query handlers reside in separate Application-layer classes.

**Rationale:** Read and write workloads in OSS/BSS have fundamentally different characteristics. Invoices are write-heavy during cycle close but read-heavy during customer disputes. Service inventory reads far exceed writes. Separate models allow independent optimization, caching, and scaling.

**Enforcement:**
- Commands return `Result<T>` (never domain entities)
- Queries return dedicated DTOs/projection models
- Write side uses domain aggregates; read side uses raw SQL/Dapper for performance
- Query handlers do not call repositories; they call `IDbConnection` or dedicated read stores
- MediatR pipeline behaviors handle cross-cutting concerns (validation, audit, logging)

### 3.6 Outbox Pattern

**Statement:** All domain events that must be published to other modules shall be persisted to an Outbox table within the same database transaction as the aggregate change. A background process reads the Outbox and publishes events to the message broker.

**Rationale:** Guarantees exactly-once-at-least delivery semantics without distributed transactions. If the broker is down, events persist in the Outbox and will be published when the broker recovers.

**Enforcement:**
- EF Core `SaveChangesAsync` interceptor writes domain events to Outbox table
- Outbox process runs as a background service (`IHostedService`)
- Outbox records are deleted only after confirmed delivery
- Dead-lettered events are written to a DeadLetter table for manual inspection
- Monitoring alerts on Outbox backlog > 1000 messages

### 3.7 Inbox Pattern

**Statement:** Each module that consumes events from other modules shall implement an Inbox table. Incoming events are idempotently processed — the Inbox deduplicates by `EventId` and ensures exactly-once processing semantics.

**Rationale:** Message brokers guarantee at-least-once delivery. The Inbox pattern ensures idempotent processing even if the same event is delivered multiple times due to broker retries or consumer crashes.

**Enforcement:**
- Incoming event handler checks `EventId` against Inbox table
- If `EventId` exists, event is acknowledged and skipped
- If `EventId` is new, event is processed and `EventId` is recorded
- Processing is wrapped in a database transaction
- Failed events are retried (configurable retry policy, exponential backoff)

### 3.8 Multi-Tenant SaaS

**Statement:** The platform shall support multiple tenants (CSPs) in a single deployment with strict data isolation. Every database table shall include a `TenantId` column (shared schema approach), and every query shall be filtered by the current tenant context.

**Rationale:** Shared schema with tenant ID provides the best operational simplicity, lowest cost per tenant, and simplest schema migrations — while row-level security (RLS) in PostgreSQL provides defense-in-depth isolation.

**Enforcement:**
- `TenantId` is a required column on all tenant-scoped tables
- EF Core global query filters automatically append `TenantId` to all queries
- PostgreSQL Row-Level Security (RLS) policy enforces `tenant_id = current_setting('app.tenant_id')`
- Tenant context is extracted from JWT claims and propagated via `ITenantContext` scoped service
- Cross-tenant data access is architecturally impossible (verified by integration tests)
- Tenant provisioning and lifecycle management is handled by the IAM module

### 3.9 RBAC (Role-Based Access Control)

**Statement:** All authorization decisions shall be based on roles assigned to principals. Permissions are granular (down to API endpoint + HTTP method), grouped into roles, and assigned to users or API clients.

**Rationale:** Telecom operations involve dozens of distinct job functions (NOC engineer, billing analyst, sales agent, customer support, super-admin) each needing precisely scoped access.

**Enforcement:**
- Policies evaluated via `[Authorize]` attributes and policy-based authorization
- Permissions follow the format: `{module}:{resource}:{action}` (e.g., `billing:invoice:read`)
- Roles are configurable per tenant (tenant admins can define custom roles)
- Super-admin role exists only at the platform level (cross-tenant operations)

### 3.10 Security By Design

**Statement:** Security requirements shall be addressed at every layer of the architecture — network, transport, application, data — and shall not be retrofitted.

**Rationale:** OSS/BSS systems handle PII, billing data, network configurations, and payment instrument data. A breach is catastrophic. Security cannot be an afterthought.

**Enforcement:**
- All APIs require authentication except health/readiness endpoints
- All inter-service communication uses TLS 1.3
- Secrets are managed via a secrets vault (never in config files)
- Input validation is mandatory on all API endpoints (FluentValidation)
- SQL injection is prevented by EF Core parameterization and raw SQL via Dapper with parameters
- JWT tokens have short expiry (15 min access, 7 day refresh with rotation)
- API keys for machine-to-machine communication are hashed at rest

### 3.11 Observability By Design

**Statement:** Every component shall emit structured logs, metrics, and distributed traces without requiring per-developer instrumentation effort. Observability infrastructure is a platform concern, not an application concern.

**Rationale:** In a modular monolith with eventual consistency and asynchronous processing, understanding what happened when requires end-to-end visibility. Debugging billing discrepancies or provisioning failures without observability is impossible.

**Enforcement:**
- OpenTelemetry SDK is configured in the ASP.NET Core pipeline (not per-module)
- All modules share the same tracer, meter, and logger factory
- MediatR pipeline behavior creates activity spans for every command/query
- EF Core interceptor captures query duration and SQL for slow-query detection
- Metrics: request rate, error rate, latency (p50/p95/p99), outbox backlog, queue depth
- Logs are structured (serilog or Microsoft.Extensions.Logging with structured templates)
- Logging level is dynamically configurable per module via configuration

### 3.12 Auditability By Design

**Statement:** All state-changing operations shall be recorded in an append-only Audit Log. The audit log is immutable, tamper-evident, and time-bound for retention.

**Rationale:** Telecom billing disputes, provisioning errors, and privilege abuse investigations all require a definitive record of "who changed what, when, and what was the previous value."

**Enforcement:**
- Audit records are written via MediatR pipeline behavior (automatic for all commands)
- Each audit record contains: actor ID, tenant ID, timestamp, resource type, resource ID, action, before/after state (JSON), IP address, correlation ID
- Audit records are stored in a dedicated `audit` schema, append-only (no deletes, no updates)
- Retention policy: 7 years online, then archive to cold storage
- Audit logs are accessible only to platform super-admins and tenant auditors (RBAC-gated)

### 3.13 Avoid Premature Microservices

**Statement:** No bounded context shall be deployed as a separate service until there is evidence — from production monitoring and load testing — that the modular monolith cannot meet performance or team autonomy requirements.

**Rationale:** Conway's Law suggests organizations produce designs that mirror their communication structures. A three-team organization does not need fifteen microservices. Premature distribution multiplies complexity without corresponding benefit.

**Enforcement:**
- Extraction criteria: (a) sustained CPU/memory pressure isolated to one module, (b) independent deployability required for team velocity, (c) different scalability characteristics (e.g., rating needs 100x more instances than CRM)
- Until extraction criteria are met, all modules run in-process
- Module boundaries are maintained such that extraction requires zero code changes — only hosting configuration

### 3.14 Avoid Shared Databases Between Modules

**Statement:** Each bounded context owns its data exclusively. No other module may access another module's database tables directly — all cross-module data access must go through events or API Gateway-mediated queries.

**Rationale:** Shared databases create implicit coupling that destroys module independence. A schema change in one module breaks queries in another. Eventual consistency via events preserves autonomy.

**Enforcement:**
- Each module has its own EF Core DbContext and database schema
- Cross-module queries use the Query-by-API pattern: the requesting module calls the owning module's public API
- Architecture tests verify that no module references another module's DbContext or entity types
- Database migrations for each module are independent

---

## 4. Bounded Contexts

### 4.1 IAM (Identity and Access Management)

| Aspect | Detail |
|--------|--------|
| **Domain** | Identity, Authentication, Authorization, RBAC, Tenancy |
| **Purpose** | Manages users, roles, permissions, tenants, and authentication. Acts as the platform's identity provider. |
| **Tables** | `iam.tenants`, `iam.users`, `iam.roles`, `iam.permissions`, `iam.role_permissions`, `iam.user_roles`, `iam.refresh_tokens`, `iam.api_clients`, `iam.client_permissions`, `iam.tenant_settings` |
| **Aggregates** | `Tenant` (name, slug, status, settings), `User` (email, display name, status, tenant membership), `Role` (name, permissions collection), `ApiClient` (client id, hashed secret, scopes) |
| **Value Objects** | `TenantSlug`, `Email`, `PermissionName`, `PasswordHash`, `TokenId` |
| **Domain Events** | `UserRegistered`, `UserRoleAssigned`, `UserRoleRevoked`, `TenantProvisioned`, `TenantSuspended`, `TenantActivated`, `ApiClientCreated` |
| **Integration Events** | `TenantProvisionedEvent` (→ all modules), `TenantSuspendedEvent` (→ all modules), `UserCreatedEvent` (→ CRM) |
| **Dependencies** | None (base layer) |

### 4.2 CRM (Customer Relationship Management)

| Aspect | Detail |
|--------|--------|
| **Domain** | Leads, Contacts, Accounts, Customer Lifecycle |
| **Purpose** | Manages customer-facing data: leads, contacts, accounts, customer interactions, and customer lifecycle stages. |
| **Tables** | `crm.accounts`, `crm.contacts`, `crm.leads`, `crm.lead_sources`, `crm.interactions`, `crm.customer_segments`, `crm.address_book` |
| **Aggregates** | `Account` (account type, billing info, contacts collection), `Contact` (name, email, phone, address), `Lead` (source, status, assigned sales rep), `Interaction` (type, notes, channel) |
| **Value Objects** | `FullName`, `PhoneNumber`, `Address`, `TaxId` |
| **Domain Events** | `AccountCreated`, `AccountUpdated`, `LeadQualified`, `LeadConvertedToAccount`, `ContactMoved` |
| **Integration Events** | `AccountCreatedEvent` (→ Orders, Billing), `AccountUpdatedEvent` (→ Billing) |
| **Dependencies** | IAM (tenant context, user identity) |

### 4.3 ProductCatalog

| Aspect | Detail |
|--------|--------|
| **Domain** | Products, Offers, Price Plans, Discounts, Bundles |
| **Purpose** | Defines the service portfolio. Products are technical specifications; Offers are market-facing packages with pricing. |
| **Tables** | `catalog.products`, `catalog.product_specifications`, `catalog.offers`, `catalog.offer_prices`, `catalog.offer_discounts`, `catalog.charge_definitions`, `catalog.bundles`, `catalog.bundle_items`, `catalog.eligibility_rules` |
| **Aggregates** | `Product` (name, category, technical specs), `Offer` (prices, discounts, eligibility rules, validity period), `ChargeDefinition` (type: recurring/one-time/usage, amount, tax rule), `Bundle` (items, rules) |
| **Value Objects** | `Money`, `PriceModel` (flat/volume/tiered), `BillingCycle` (monthly/quarterly/annual/one-time), `RecurringPeriod`, `DiscountPercentage`, `DiscountAmount` |
| **Domain Events** | `ProductCreated`, `OfferCreated`, `OfferActivated`, `OfferDeactivated`, `PricePlanChanged`, `BundleModified` |
| **Integration Events** | `OfferActivatedEvent` (→ Orders, Subscriptions, Rating), `OfferDeactivatedEvent` (→ Orders, Subscriptions), `ChargeDefinitionChangedEvent` (→ Billing) |
| **Dependencies** | IAM (tenant context) |

### 4.4 Orders

| Aspect | Detail |
|--------|--------|
| **Domain** | Orders, Order Items, Order Workflows, Order Validation |
| **Purpose** | Captures customer requests for new services, changes, or disconnections. Orchestrates the order-to-active lifecycle. |
| **Tables** | `orders.orders`, `orders.order_items`, `orders.order_charges`, `orders.order_status_history`, `orders.order_validation_results` |
| **Aggregates** | `Order` (order type: new/change/disconnect/reconnect, status, customer, items collection), `OrderItem` (offer reference, quantity, charges collection, attributes) |
| **Value Objects** | `OrderStatus` (draft/pending/validated/in-progress/completed/rejected/cancelled), `OrderType`, `OrderItemAttribute` |
| **Domain Events** | `OrderSubmitted`, `OrderValidated`, `OrderRejected`, `OrderCompleted`, `OrderItemActivated`, `OrderCancelled` |
| **Integration Events** | `OrderSubmittedEvent` (→ Provisioning, Subscriptions, Billing), `OrderCompletedEvent` (→ Subscriptions, ServiceInventory, Billing) |
| **Dependencies** | IAM, CRM, ProductCatalog |

### 4.5 Subscriptions

| Aspect | Detail |
|--------|--------|
| **Domain** | Subscriptions, Subscription Lifecycle, Entitlements |
| **Purpose** | Tracks active service subscriptions for customers. A subscription is created when an order is fulfilled and represents the customer's current service entitlements. |
| **Tables** | `subscriptions.subscriptions`, `subscriptions.subscription_items`, `subscriptions.subscription_status_history`, `subscriptions.subscription_charges`, `subscriptions.subscription_amendments` |
| **Aggregates** | `Subscription` (customer, status, items collection, billing account, activation/suspension/termination dates), `SubscriptionItem` (offer reference, quantity, status, charges collection), `SubscriptionAmendment` (amendment type, previous state, new state, reason) |
| **Value Objects** | `SubscriptionStatus` (active/suspended/terminated/pending), `AmendmentType` (upgrade/downgrade/add/remove/suspend/resume) |
| **Domain Events** | `SubscriptionActivated`, `SubscriptionSuspended`, `SubscriptionResumed`, `SubscriptionTerminated`, `SubscriptionAmended`, `SubscriptionItemAdded`, `SubscriptionItemRemoved` |
| **Integration Events** | `SubscriptionActivatedEvent` (→ Billing, ServiceInventory, Rating), `SubscriptionSuspendedEvent` (→ Billing, Provisioning, Rating), `SubscriptionTerminatedEvent` (→ Billing, ServiceInventory, Provisioning), `SubscriptionAmendedEvent` (→ Billing, Rating) |
| **Dependencies** | IAM, ProductCatalog, Orders |

### 4.6 Rating

| Aspect | Detail |
|--------|--------|
| **Domain** | Usage Records, Rating Engine, Charge Calculation, Discount Application |
| **Purpose** | Processes raw usage data (CDRs, session logs) and calculates rated charges based on price plans, discounts, and allowances defined in the product catalog. |
| **Tables** | `rating.usage_records`, `rating.rated_charges`, `rating.discount_applications`, `rating.allowance_consumption`, `rating.rating_errors`, `rating.raw_cdrs` |
| **Aggregates** | `UsageRecord` (service type, quantity, units, timestamp, source), `RatedCharge` (calculated amount, tax amount, rating factors), `AllowanceBucket` (included units, consumed units, reset period) |
| **Value Objects** | `UsageQuantity`, `RatingFactor`, `ChargeAmount`, `AllowanceUnit`, `RatingResult` |
| **Domain Events** | `UsageRecordProcessed`, `RatingCompleted`, `AllowanceExhausted`, `ThresholdReached` |
| **Integration Events** | `RatingCompletedEvent` (→ Billing for invoice creation), `AllowanceExhaustedEvent` (→ Notifications for speed-throttle/alerts), `ThresholdReachedEvent` (→ Notifications) |
| **Dependencies** | IAM, ProductCatalog, Subscriptions |

### 4.7 Billing

| Aspect | Detail |
|--------|--------|
| **Domain** | Billing Accounts, Billing Cycles, Bill Runs, Invoice Generation, Debit/Credit Notes |
| **Purpose** | Manages the billing lifecycle: aggregates charges (recurring, one-time, usage) into invoices on a periodic basis, handles pro-ration, adjustments, and billing account management. |
| **Tables** | `billing.billing_accounts`, `billing.billing_cycles`, `billing.bill_runs`, `billing.bill_run_items`, `billing.charge_items`, `billing.adjustments`, `billing.debit_notes`, `billing.credit_notes`, `billing.pro_ration_rules` |
| **Aggregates** | `BillingAccount` (account number, payment terms, billing cycle, tax exemption status), `BillRun` (cycle reference, status, items collection, totals), `ChargeItem` (source: recurring/one-time/usage, amount, tax, period), `Adjustment` (type: manual/system, reason, amount, approval) |
| **Value Objects** | `BillingCycle` (monthly/quarterly/annual), `BillRunStatus` (draft/finalized/cancelled), `ChargePeriod`, `ProRationFactor`, `TaxAmount`, `Money` |
| **Domain Events** | `BillingAccountCreated`, `BillRunStarted`, `BillRunCompleted`, `ChargeItemPosted`, `AdjustmentApplied`, `CreditNoteIssued`, `DebitNoteIssued` |
| **Integration Events** | `BillRunCompletedEvent` (→ Invoices for generation), `ChargeItemPostedEvent` (→ Invoices), `BillingAccountCreatedEvent` (→ Payments) |
| **Dependencies** | IAM, CRM, ProductCatalog, Subscriptions, Rating |

### 4.8 Invoices

| Aspect | Detail |
|--------|--------|
| **Domain** | Invoice Documents, Invoice Items, Tax Calculation, Invoice Lifecycle |
| **Purpose** | Generates, formats, and manages invoice documents. Handles invoice presentation, delivery (email/postal/portal), customer disputes, and payment reconciliation. |
| **Tables** | `invoices.invoices`, `invoices.invoice_items`, `invoices.invoice_taxes`, `invoices.invoice_status_history`, `invoices.invoice_disputes`, `invoices.invoice_delivery_log`, `invoices.invoice_templates` |
| **Aggregates** | `Invoice` (invoice number, billing account, period, line items, totals, tax summary, status, due date), `InvoiceDispute` (reason, status, resolution, credited amount), `InvoiceTemplate` (layout, branding, delivery channels) |
| **Value Objects** | `InvoiceNumber`, `InvoiceStatus` (draft/final/paid/overdue/cancelled/credited), `TaxLine`, `PaymentTerm`, `InvoiceDocumentReference` |
| **Domain Events** | `InvoiceGenerated`, `InvoiceFinalized`, `InvoiceDelivered`, `InvoiceDisputed`, `InvoiceDisputeResolved`, `InvoiceCancelled`, `InvoiceCredited` |
| **Integration Events** | `InvoiceGeneratedEvent` (→ Payments, Notifications), `InvoiceFinalizedEvent` (→ Payments, Notifications, Reporting), `InvoiceDisputedEvent` (→ CRM, Collections), `InvoicePaymentReceivedEvent` (→ Collections) |
| **Dependencies** | IAM, CRM, Billing |

### 4.9 Payments

| Aspect | Detail |
|--------|--------|
| **Domain** | Payment Transactions, Payment Methods, Payment Gateways, Payment Reconciliation |
| **Purpose** | Processes incoming payments against invoices, manages payment methods (credit card, bank transfer, mobile money, wallet), handles payment gateway integration, and reconciles payments. |
| **Tables** | `payments.payment_transactions`, `payments.payment_methods`, `payments.payment_gateway_logs`, `payments.payment_reconciliation`, `payments.refunds`, `payments.wallet_transactions`, `payments.mandates` |
| **Aggregates** | `PaymentTransaction` (amount, currency, method, gateway reference, status, invoice references), `PaymentMethod` (type, token/identifier, expiry, is_default, customer), `Refund` (original transaction, amount, reason, status), `Wallet` (balance, transaction history) |
| **Value Objects** | `PaymentStatus` (pending/completed/failed/refunded/partially_refunded), `PaymentMethodType` (card/bank_transfer/mobile_money/wallet), `GatewayReference`, `Money` |
| **Domain Events** | `PaymentReceived`, `PaymentFailed`, `PaymentRefunded`, `PaymentMethodAdded`, `PaymentMethodRemoved`, `PaymentReconciled`, `WalletCredited`, `WalletDebited` |
| **Integration Events** | `PaymentReceivedEvent` (→ Invoices, Collections, Notifications), `PaymentFailedEvent` (→ Collections, Notifications), `PaymentReconciledEvent` (→ Reporting) |
| **Dependencies** | IAM, CRM, Invoices |

### 4.10 Collections

| Aspect | Detail |
|--------|--------|
| **Domain** | Dunning, Collection Cases, Payment Reminders, Credit Control, Debt Recovery |
| **Purpose** | Manages overdue invoice collection processes: automated dunning workflows, payment reminders (SMS/email), escalation rules, credit limit enforcement, and debt recovery case management. |
| **Tables** | `collections.collection_cases`, `collections.dunning_cycles`, `collections.dunning_actions`, `collections.promises_to_pay`, `collections.credit_limits`, `collections.credit_limit_logs`, `collections.escalation_rules`, `collections.collection_notes` |
| **Aggregates** | `CollectionCase` (billing account, total overdue, dunning cycle, status, assigned collector), `DunningCycle` (cycle level, actions collection, schedule), `PromiseToPay` (promised date, amount, status, reminder schedule), `CreditLimit` (customer segment, hard/soft limits, current exposure) |
| **Value Objects** | `DunningLevel` (1-5), `CollectionCaseStatus` (open/active/promised/escalated/resolved/written_off), `CreditLimitType` (hard/soft) |
| **Domain Events** | `CollectionCaseOpened`, `DunningLevelEscalated`, `PromiseToPayMade`, `PromiseToPayKept`, `PromiseToPayBroken`, `CreditLimitExceeded`, `CaseResolved`, `DebtWrittenOff` |
| **Integration Events** | `CollectionCaseOpenedEvent` (→ CRM, Notifications), `CreditLimitExceededEvent` (→ Orders, Provisioning for service restriction), `DebtWrittenOffEvent` (→ Reporting, CRM) |
| **Dependencies** | IAM, CRM, Invoices, Payments |

### 4.11 ServiceInventory

| Aspect | Detail |
|--------|--------|
| **Domain** | Customer Service Instances, Service Configurations, Service Characteristics |
| **Purpose** | Maintains a real-time inventory of all active services delivered to customers. Links subscription items to their actual service instances in the network. |
| **Tables** | `inventory.service_instances`, `inventory.service_characteristics`, `inventory.service_connections`, `inventory.service_endpoints`, `inventory.service_dependencies`, `inventory.service_topology` |
| **Aggregates** | `ServiceInstance` (service type, status, customer reference, subscription reference, provisioning status, characteristics collection), `ServiceConnection` (endpoint A, endpoint B, medium, bandwidth), `ServiceTopology` (parent-child relationships between services) |
| **Value Objects** | `ServiceStatus` (pending/active/suspended/terminated), `ServiceCharacteristic` (key-value pair), `ConnectionEndpoint`, `Bandwidth` |
| **Domain Events** | `ServiceActivated`, `ServiceSuspended`, `ServiceResumed`, `ServiceTerminated`, `ServiceModified`, `ServiceTopologyChanged` |
| **Integration Events** | `ServiceActivatedEvent` (→ NetworkInventory for resource assignment, Notifications), `ServiceTerminatedEvent` (→ NetworkInventory for resource release) |
| **Dependencies** | IAM, Subscriptions, NetworkInventory |

### 4.12 NetworkInventory

| Aspect | Detail |
|--------|--------|
| **Domain** | Network Resources, Topology, Capacity, Resource Allocation, Network Equipment |
| **Purpose** | Models the physical and logical network infrastructure: equipment, ports, circuits, IP pools, VLAN pools, bandwidth capacity. Tracks resource allocation to services. |
| **Tables** | `network.sites`, `network.devices`, `network.device_ports`, `network.connections`, `network.ip_pools`, `network.ip_allocations`, `network.vlan_pools`, `network.vlan_allocations`, `network.bandwidth_pools`, `network.capacity_reservations`, `network.network_topology` |
| **Aggregates** | `Device` (type, model, vendor, serial number, site, ports collection, status), `DevicePort` (port type, media, status, bandwidth, connected to), `IpPool` (CIDR range, total, allocated, reserved), `VlanPool` (range, allocations), `Connection` (type: fiber/circuit/wireless, endpoints, bandwidth, provider) |
| **Value Objects** | `IpAddress`, `CidrNotation`, `VlanId`, `PortSpeed`, `DeviceStatus` (online/offline/degraded/maintenance), `SiteLocation` |
| **Domain Events** | `DeviceAdded`, `DeviceStatusChanged`, `PortActivated`, `PortDeactivated`, `CapacityThresholdReached`, `IpAllocated`, `IpReleased`, `VlanAllocated`, `VlanReleased` |
| **Integration Events** | `CapacityThresholdReachedEvent` (→ Notifications, Provisioning), `ResourceAllocationFailedEvent` (→ Provisioning, Orders), `NetworkTopologyChangedEvent` (→ ServiceInventory) |
| **Dependencies** | IAM |

### 4.13 Provisioning

| Aspect | Detail |
|--------|--------|
| **Domain** | Service Activation, Configuration Deployment, Provisioning Workflows, Network Integration |
| **Purpose** | Automates the technical activation of services on network equipment. Translates order/subscription requests into device configurations, RADIUS updates, BNG policies, and network provisioning actions. |
| **Tables** | `provisioning.provisioning_jobs`, `provisioning.provisioning_steps`, `provisioning.provisioning_templates`, `provisioning.device_credentials`, `provisioning.provisioning_logs`, `provisioning.activation_schedules` |
| **Aggregates** | `ProvisioningJob` (order/service reference, status, steps collection, device references), `ProvisioningStep` (action: configure/activate/deactivate/test, device, configuration payload, status, retry count, result), `ProvisioningTemplate` (device type, configuration template, parameters) |
| **Value Objects** | `JobStatus` (pending/in_progress/completed/failed/rolled_back), `StepAction`, `ConfigurationPayload`, `DeviceCredentialReference` |
| **Domain Events** | `ProvisioningJobStarted`, `ProvisioningStepCompleted`, `ProvisioningJobCompleted`, `ProvisioningJobFailed`, `ProvisioningJobRolledBack` |
| **Integration Events** | `ProvisioningJobCompletedEvent` (→ Orders, Subscriptions, ServiceInventory, Notifications), `ProvisioningJobFailedEvent` (→ Orders, Workflow for escalation, Notifications) |
| **Dependencies** | IAM, Orders, Subscriptions, NetworkInventory, Workflow |

### 4.14 Workflow

| Aspect | Detail |
|--------|--------|
| **Domain** | Workflow Definitions, Workflow Instances, Activities, Transitions, Timers |
| **Purpose** | Provides a general-purpose workflow engine for orchestrating multi-step, multi-actor processes. Used by order fulfillment, provisioning, dunning, and other module workflows. |
| **Tables** | `workflow.workflow_definitions`, `workflow.workflow_instances`, `workflow.workflow_states`, `workflow.workflow_transitions`, `workflow.activity_definitions`, `workflow.activity_executions`, `workflow.workflow_timers`, `workflow.workflow_errors` |
| **Aggregates** | `WorkflowDefinition` (name, version, states collection, transitions collection, activities collection), `WorkflowInstance` (definition reference, current state, context data, variables, status), `ActivityExecution` (activity reference, input, output, status, started/completed timestamps) |
| **Value Objects** | `WorkflowStatus` (running/suspended/completed/failed/cancelled), `TransitionTrigger` (event/timer/manual), `ActivityType`, `WorkflowContext` |
| **Domain Events** | `WorkflowStarted`, `WorkflowStateChanged`, `WorkflowActivityCompleted`, `WorkflowCompleted`, `WorkflowFailed`, `WorkflowTimerExpired` |
| **Integration Events** | `WorkflowCompletedEvent` (→ calling module with result), `WorkflowFailedEvent` (→ calling module, Notifications) |
| **Dependencies** | IAM |

### 4.15 Ticketing

| Aspect | Detail |
|--------|--------|
| **Domain** | Tickets, Ticket Categories, Ticket Lifecycle, SLA, Ticket Assignment |
| **Purpose** | Provides a unified ticketing system for customer support, NOC incidents, and internal tasks. Tracks issues from creation through resolution with SLA management. |
| **Tables** | `ticketing.tickets`, `ticketing.ticket_messages`, `ticketing.ticket_attachments`, `ticketing.ticket_categories`, `ticketing.ticket_priorities`, `ticketing.ticket_slas`, `ticketing.ticket_assignments`, `ticketing.ticket_status_history`, `ticketing.sla_violations` |
| **Aggregates** | `Ticket` (title, description, category, priority, status, customer reference, assignee, SLA, messages collection), `TicketSla` (response time, resolution time, escalation policy), `SlaViolation` (ticket, violated metric, breached at, escalated to) |
| **Value Objects** | `TicketStatus` (open/in_progress/waiting_on_customer/resolved/closed), `TicketPriority` (critical/high/medium/low), `SlaTimer`, `TicketUrgency` |
| **Domain Events** | `TicketCreated`, `TicketAssigned`, `TicketStatusChanged`, `TicketPriorityChanged`, `TicketResolved`, `TicketClosed`, `SlaWarningTriggered`, `SlaBreached` |
| **Integration Events** | `TicketCreatedEvent` (→ Notifications), `SlaBreachedEvent` (→ Notifications, Workflow for escalation), `TicketResolvedEvent` (→ CRM) |
| **Dependencies** | IAM, CRM |

### 4.16 Notifications

| Aspect | Detail |
|--------|--------|
| **Domain** | Notification Messages, Templates, Delivery Channels, Delivery Logs |
| **Purpose** | Provides centralized notification delivery across multiple channels: email, SMS, push notifications, in-app notifications, webhooks. Manages templates and delivery preferences. |
| **Tables** | `notifications.notification_templates`, `notifications.notification_messages`, `notifications.notification_delivery_log`, `notifications.notification_preferences`, `notifications.webhook_endpoints`, `notifications.webhook_delivery_log` |
| **Aggregates** | `NotificationMessage` (template reference, channel, recipient, rendered content, status), `NotificationTemplate` (name, channel, subject/body templates, variables), `WebhookEndpoint` (url, events subscribed, secret, retry policy) |
| **Value Objects** | `NotificationChannel` (email/sms/push/in_app/webhook), `DeliveryStatus` (pending/delivered/failed/bounced), `TemplateVariable`, `WebhookEventType` |
| **Domain Events** | `NotificationSent`, `NotificationDelivered`, `NotificationFailed`, `WebhookDelivered`, `WebhookFailed` |
| **Integration Events** | None consumed directly (called via application service by other modules). Consumes from all modules to send notifications. |
| **Dependencies** | IAM |

### 4.17 Reporting

| Aspect | Detail |
|--------|--------|
| **Domain** | Reports, Dashboards, Data Marts, Scheduled Reports, Report Definitions |
| **Purpose** | Provides business intelligence and operational reporting. Generates scheduled and ad-hoc reports from materialized data sources across modules. |
| **Tables** | `reporting.report_definitions`, `reporting.scheduled_reports`, `reporting.report_executions`, `reporting.report_outputs`, `reporting.dashboard_definitions`, `reporting.data_marts`, `reporting.report_snapshots`, `reporting.report_subscriptions` |
| **Aggregates** | `ReportDefinition` (name, type, data source query, parameters, output format), `ReportExecution` (definition, parameters, status, output reference, executed at), `DashboardDefinition` (layout, widgets, data source references) |
| **Value Objects** | `ReportFormat` (pdf/csv/excel/html/json), `ReportStatus` (pending/running/completed/failed), `DashboardWidget` (type, data source, refresh interval) |
| **Domain Events** | `ReportGenerated`, `ScheduledReportDelivered`, `DashboardRefreshed` |
| **Integration Events** | Consumes events from all modules for data mart materialization. Publishes `ReportGeneratedEvent` (→ Notifications). |
| **Dependencies** | IAM |

### 4.18 Audit

| Aspect | Detail |
|--------|--------|
| **Domain** | Audit Logs, Audit Trails, Retention Policies, Tamper Evidence |
| **Purpose** | Provides append-only audit logging for all state-changing operations across the platform. Writes are automatic via MediatR pipeline behavior. |
| **Tables** | `audit.audit_logs`, `audit.audit_retention_policies`, `audit.audit_archives` |
| **Aggregates** | `AuditLogEntry` (actor, tenant, timestamp, resource type, resource ID, action, before/after JSON, IP, correlation ID, module name) |
| **Value Objects** | `AuditAction` (create/update/delete/archive/restore), `ResourceIdentifier`, `CorrelationId`, `SerializedSnapshot` |
| **Domain Events** | None (Audit does not publish events — it is the event sink for other modules) |
| **Integration Events** | Consumes no events. Called synchronously via MediatR pipeline behavior. |
| **Dependencies** | IAM |

### 4.19 APIGateway

| Aspect | Detail |
|--------|--------|
| **Domain** | API Routing, Rate Limiting, API Key Management, Request/Response Transformation, API Documentation |
| **Purpose** | Central entry point for all external (BSS portal, customer portal, partner API) and internal (cross-module query) HTTP traffic. Handles authentication, authorization, rate limiting, and routing to module endpoints. |
| **Tables** | `gateway.api_routes`, `gateway.rate_limit_rules`, `gateway.api_usage_logs`, `gateway.api_key_usage` |
| **Aggregates** | `ApiRoute` (path, method, target module, authentication required, authorization policies), `RateLimitRule` (route/method/tenant/client, limit, window, burst) |
| **Value Objects** | `RoutePattern`, `RateLimitWindow`, `HttpMethod`, `ApiUsageMetric` |
| **Domain Events** | None |
| **Integration Events** | None. Routes all external traffic to internal modules. |
| **Dependencies** | IAM (token validation, API key validation) |

---

## 5. Module Dependency Map

### 5.1 Dependency Graph

The following directed graph shows dependencies between bounded contexts. An arrow from A → B means "A depends on B."

```
IAM (foundation — no dependencies)
  ↑
  ├──→ CRM, ProductCatalog, NetworkInventory, Workflow, Notifications, Reporting, Audit, APIGateway
  │
CRM
  ↑
  ├──→ Orders, Ticketing, Collections
  │
ProductCatalog
  ↑
  ├──→ Orders, Subscriptions, Rating, Billing
  │
Orders
  ↑
  ├──→ Provisioning
  │
Subscriptions ←── Orders
  ↑
  ├──→ Billing, Rating, ServiceInventory, Provisioning
  │
Rating
  ↑
  ├──→ Billing
  │
Billing ←── Subscriptions, Rating
  ↑
  ├──→ Invoices
  │
Invoices
  ↑
  ├──→ Payments, Collections
  │
Payments
  ↑
  ├──→ Collections
  │
ServiceInventory ←── Subscriptions
  ↑
  ├──→ NetworkInventory
  │
NetworkInventory ←── ServiceInventory (reverse dependency for status checks)
  ↑
  └── Provisioning
  │
Provisioning ←── Orders, Subscriptions, NetworkInventory
  ↑
  └── Workflow
  │
Workflow ←── Provisioning (and any module needing orchestration)
  ↑
  └── (no upward dependencies — called by others)
  │
Ticketing ←── CRM
  ↑
  └── (no upward dependencies)
  │
Notifications ←── (consumes from all, depends on no business module)
  ↑
  └── IAM only
  │
Reporting ←── (consumes from all, depends on no business module)
  ↑
  └── IAM only
  │
Audit ←── (called by all, depends on no business module)
  ↑
  └── IAM only
  │
APIGateway ←── IAM only (routes to all others without depending on them)
```

### 5.2 Dependency Matrix

| Module | Depends On | Depended On By |
|--------|-----------|----------------|
| IAM | — | All modules |
| CRM | IAM | Orders, Ticketing, Collections |
| ProductCatalog | IAM | Orders, Subscriptions, Rating, Billing |
| Orders | IAM, CRM, ProductCatalog | Provisioning |
| Subscriptions | IAM, ProductCatalog, Orders | Billing, Rating, ServiceInventory, Provisioning |
| Rating | IAM, ProductCatalog, Subscriptions | Billing |
| Billing | IAM, CRM, ProductCatalog, Subscriptions, Rating | Invoices |
| Invoices | IAM, CRM, Billing | Payments, Collections |
| Payments | IAM, CRM, Invoices | Collections |
| Collections | IAM, CRM, Invoices, Payments | — |
| ServiceInventory | IAM, Subscriptions, NetworkInventory | — |
| NetworkInventory | IAM | ServiceInventory, Provisioning |
| Provisioning | IAM, Orders, Subscriptions, NetworkInventory, Workflow | — |
| Workflow | IAM | Provisioning |
| Ticketing | IAM, CRM | — |
| Notifications | IAM | — (called by all) |
| Reporting | IAM | — (reads from all) |
| Audit | IAM | — (called by all) |
| APIGateway | IAM | — |

### 5.3 Dependency Rules

1. **No circular dependencies** — The dependency graph is a DAG. CI enforces this with architecture tests.
2. **No cross-module database access** — Modules can only read/write their own schemas.
3. **Cross-module queries go through API Gateway** — A module needing data from another module calls that module's public API endpoint.
4. **Cross-module state changes go through events** — A module needing to trigger state changes in another module publishes an event.
5. **IAM is the universal dependency** — Every module depends on IAM for tenant context, user identity, and authorization checks.

---

## 6. Integration Strategy

### 6.1 Communication Patterns

| Pattern | Mechanism | Use Case | Example |
|---------|-----------|----------|---------|
| In-process Command | MediatR `IRequest<T>` | State change within a module | `SubmitOrderCommand` |
| In-process Query | MediatR `IRequest<T>` | Data retrieval within a module | `GetCustomerBillingSummaryQuery` |
| In-process Event | MediatR `INotification` | Internal reaction within a module | `OrderSubmittedEvent` → send email notification |
| Integration Event | RabbitMQ via Outbox/Inbox | Cross-module state notification | `SubscriptionActivatedEvent` → Billing starts recurring charges |
| Synchronous Query API | HTTP REST (through API Gateway) | Cross-module data retrieval | Orders module queries CRM for customer details |
| Command API | HTTP REST (through API Gateway) | External/partner-initiated operations | Customer portal submits a trouble ticket |

### 6.2 Module Communication Flows

#### 6.2.1 Order-to-Active Flow

```
Customer → APIGateway → Orders (SubmitOrder)
  → Orders validates against ProductCatalog (in-process query)
  → Orders validates customer via CRM (API query through Gateway)
  → Orders creates Order aggregate
  → Orders publishes OrderSubmittedEvent (Outbox)
  → (Multiple consumers receive OrderSubmittedEvent)
    → Subscriptions: creates PendingSubscription
    → Provisioning: creates ProvisioningJob
    → Workflow: starts FulfillmentWorkflow
    → Billing: creates pending BillingAccount charges
  → ProvisioningJob executes against NetworkInventory
  → ProvisioningJob completes → publishes ProvisioningCompletedEvent
  → Subscriptions: activates Subscription → publishes SubscriptionActivatedEvent
  → ServiceInventory: creates ServiceInstance → publishes ServiceActivatedEvent
  → Billing: activates recurring charges
  → Notifications: sends activation confirmation
  → Orders: marks Order as Completed
```

#### 6.2.2 Billing Cycle Flow

```
 Scheduler → Billing (StartBillRunCommand)
  → Billing creates BillRun aggregate
  → Billing publishes BillRunStartedEvent
  → Rating: ensures all usage is rated (if needed)
  → Billing collects charges from:
    → Subscription recurring charges (in-process)
    → Rating completed charges (event consumed earlier)
  → Billing posts all ChargeItems → publishes ChargeItemPostedEvent
  → Billing completes BillRun → publishes BillRunCompletedEvent
  → Invoices: receives BillRunCompletedEvent
  → Invoices generates Invoice documents → publishes InvoiceGeneratedEvent
  → Payments: receives InvoiceGeneratedEvent → marks invoice for collection
  → Notifications: sends invoice notification to customer
  → Reporting: updates billing data marts
```

#### 6.2.3 Dunning/Collections Flow

```
  Scheduler → Collections (EvaluateOverdueInvoicesCommand)
  → Collections queries Invoices for overdue invoices (API query)
  → Collections opens CollectionCase → publishes CollectionCaseOpenedEvent
  → Workflow: starts DunningWorkflow for the case
  → DunningWorkflow sends reminders (via Notifications)
  → If payment received: PaymentReceivedEvent → Collections resolves case
  → If dunning level escalates: publishes DunningLevelEscalatedEvent
  → At max dunning level: workflow triggers service suspension
    → Publishes CreditLimitExceededEvent
    → Provisioning: suspends service
```

### 6.3 API Gateway Routing

The API Gateway (YARP-based reverse proxy in .NET 9) routes external requests to module endpoints:

```
POST /api/v1/{tenant}/orders          → Orders module
GET  /api/v1/{tenant}/invoices        → Invoices module
POST /api/v1/{tenant}/tickets         → Ticketing module
GET  /api/v1/admin/tenants            → IAM module (super-admin only)
POST /api/v1/{tenant}/webhooks/payments → Payments module (external gateway callback)
```

The Gateway:
1. Extracts tenant context from JWT or API key
2. Validates authentication and authorization
3. Enforces rate limits
4. Logs API usage (→ Reporting data marts)
5. Routes request to appropriate module endpoint

---

## 7. Security Architecture

### 7.1 Authentication

| Mechanism | Use Case | Details |
|-----------|----------|---------|
| **OpenID Connect / OAuth 2.0** | Human users (admin portal, customer portal) | Keycloak as identity provider. Authorization code flow with PKCE. JWT access tokens (RS256, 15 min TTL). Refresh tokens (7 day TTL, rotation). |
| **API Keys** | Machine-to-machine (partner API, internal automation) | HMAC-sha256 hashed API keys stored in IAM. Key presented in `X-API-Key` header. Rate-limited and permission-scoped. |
| **Client Certificates** | Network equipment integration | mTLS between provisioning module and network devices. Certificate pinned. |

### 7.2 Authorization

#### 7.2.1 RBAC Model

```
Platform-level roles:
  - super-admin: full cross-tenant access (platform operations)

Tenant-level roles (configurable per tenant):
  - tenant-admin: full access within tenant
  - billing-admin: billing, invoices, payments, collections
  - billing-viewer: read-only billing data
  - network-engineer: network inventory, provisioning
  - support-agent: ticketing, CRM (read)
  - sales-agent: CRM, orders
  - customer: own data only (portal access)
```

#### 7.2.2 Permission Format

`{module}:{resource}:{action}`

Examples:
- `crm:account:read`
- `crm:account:write`
- `billing:invoice:read`
- `billing:invoice:finalize`
- `provisioning:job:create`
- `network:device:configure`

#### 7.2.3 Policy Enforcement

```csharp
// Policy registered at startup
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BillingAdmin", policy =>
        policy.RequireClaim("permission", "billing:invoice:read")
              .RequireClaim("permission", "billing:invoice:finalize"));
});

// API endpoint enforcement
[Authorize(Policy = "BillingAdmin")]
[Permission("billing:invoice:read")]
public async Task<IActionResult> GetInvoice(Guid id) { ... }
```

### 7.3 Tenant Isolation

1. **JWT Claim:** `tenant_id` is embedded in the JWT token at authentication time
2. **Scoped Service:** `ITenantContext` is populated from JWT claims in the API Gateway
3. **EF Core Filter:** Global query filter applied to all entities:
   ```csharp
   modelBuilder.Entity<Invoice>().HasQueryFilter(i => i.TenantId == _tenantContext.TenantId);
   ```
4. **PostgreSQL RLS:** Defense-in-depth at database level:
   ```sql
   ALTER TABLE invoices.invoices ENABLE ROW LEVEL SECURITY;
   CREATE POLICY tenant_isolation ON invoices.invoices
     USING (tenant_id = current_setting('app.tenant_id')::uuid);
   ```
5. **Integration Tests:** Every module's integration tests verify that cross-tenant access returns empty results.

### 7.4 API Security

| Measure | Implementation |
|---------|---------------|
| TLS | All external and inter-service traffic over TLS 1.3 |
| Rate Limiting | Per-tenant, per-API-key, per-endpoint rate limits (fixed window + token bucket) |
| Request Validation | FluentValidation on all command/query models |
| CORS | Strict origin policy — only configured portal domains |
| Content Security | Antiforgery tokens for state-changing requests from browser |
| Security Headers | HSTS, X-Content-Type-Options, X-Frame-Options, CSP |
| Payload Size Limits | 10 MB max request body; 1 MB for non-file uploads |
| API Versioning | URL-based versioning (`/api/v1/...`), minimum 2 versions supported |

### 7.5 Secrets Management

| Secret Type | Storage | Access |
|-------------|---------|--------|
| Database passwords | Environment variables (Docker secrets) | Mounted at container startup |
| JWT signing keys | Keycloak-managed (RS256 key pair) | Never stored in app code |
| API keys (external services) | Vault or encrypted config | Decrypted at startup by infrastructure layer |
| Payment gateway credentials | Vault | Scoped to Payments module only |
| Network device credentials | Vault | Scoped to Provisioning module only |
| RabbitMQ credentials | Docker secrets | Mounted at container startup |
| MinIO credentials | Docker secrets | Mounted at container startup |
| OpenSearch credentials | Docker secrets | Mounted at container startup |

### 7.6 Audit Logging Security

- Audit logs are append-only (no UPDATE, no DELETE — enforced by database triggers)
- Audit schema is grant-protected — only the Audit module has write access
- Audit logs are never purged (retention policy is archival, not deletion)
- Before/after state snapshots are stored as JSON — sensitive fields (passwords, tokens, payment PANs) are redacted before serialization
- Tamper evidence: SHA-256 hash chain linking consecutive audit entries

---

## 8. SaaS Architecture

### 8.1 Multi-Tenant Data Isolation Strategy

**Decision: Shared Schema with TenantId (with RLS)**

| Approach | Selected? | Rationale |
|----------|-----------|-----------|
| Shared schema + TenantId column | **YES** | Best operational simplicity. Single schema per module. Easy migrations. Low cost per tenant. RLS provides defense-in-depth. |
| Schema-per-tenant | No | Operational complexity. N schemas x M modules. Migration nightmare at scale. Only justified for extreme compliance requirements (not in scope). |
| Database-per-tenant | No | Connection pool explosion. Backup/recovery complexity. Only for largest Tier-1 tenants with dedicated infrastructure requirements. |
| Hybrid (Shared + Schema) | Future option | Large tenants can be migrated to dedicated schema if needed. The TenantId column approach supports this migration path. |

**Data isolation layers (defense in depth):**

1. **Application layer:** `ITenantContext` + EF Core global query filters
2. **Database layer:** PostgreSQL RLS policies on every tenant-scoped table
3. **API Gateway layer:** Route validation — tenant slug in URL must match JWT tenant claim
4. **Integration layer:** Events carry `TenantId` — event handlers filter by tenant
5. **Reporting layer:** Data marts include `TenantId` — reports are tenant-scoped by default

### 8.2 Tenant Provisioning Lifecycle

```
1. Platform admin creates tenant via IAM Admin API
2. IAM provisions tenant:
   a. Creates tenant record in iam.tenants
   b. Creates tenant schema (idempotent CREATE SCHEMA IF NOT EXISTS)
   c. Runs tenant-specific migrations (new tenant seeding data)
   d. Configures RLS policies for tenant
   e. Creates initial tenant admin user
   f. Configures tenant settings (branding, domains, defaults)
3. IAM publishes TenantProvisionedEvent
4. All modules consume TenantProvisionedEvent:
   a. Each module creates tenant-specific seed data if needed
   b. Each module configures tenant defaults
5. Tenant is active and ready for use
```

### 8.3 Tenant States

| State | Description | Allowed Operations |
|-------|-------------|-------------------|
| `provisioning` | Being set up | No operations |
| `active` | Fully operational | All operations |
| `suspended` | Payment/compliance hold | Read-only, no state changes |
| `disabled` | Administratively disabled | No operations (API returns 403) |
| `deleted` | Scheduled for purging | No operations (grace period before data deletion) |

### 8.4 Tenant Configuration

Per-tenant configuration stored in `iam.tenant_settings`:

```json
{
  "tenant_id": "uuid",
  "settings": {
    "branding": {
      "company_name": "Example ISP",
      "logo_url": "https://cdn.example.com/logo.png",
      "primary_color": "#0055AA",
      "support_email": "support@example.com",
      "support_phone": "+1-555-0123"
    },
    "billing": {
      "default_currency": "USD",
      "tax_calculation": "automatic",
      "invoice_due_days": 30,
      "billing_cycle": "monthly",
      "invoice_prefix": "INV-2024-",
      "next_invoice_number": 1001
    },
    "features": {
      "enable_advanced_rating": true,
      "enable_collections": true,
      "enable_wallet": false,
      "enable_partner_api": true
    },
    "notifications": {
      "default_language": "en",
      "email_from": "billing@example.com",
      "sms_enabled": true,
      "email_enabled": true
    }
  }
}
```

### 8.5 Tenant Provisioning Capacity

- A single deployment supports up to 500 tenants
- Each tenant supports up to 1,000,000 subscribers
- Tenant provisioning time: < 5 seconds from API call to active
- Tenant suspension/activation: near-instant (configuration change + RLS toggle)
- Tenant data deletion: 30-day grace period + async purge

---

## 9. Deployment Architecture

### 9.1 Docker Compose Topology

```yaml
services:
  # ==================== DATA LAYER ====================
  postgres:
    image: postgres:16
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./init/postgres:/docker-entrypoint-initdb.d
    environment:
      POSTGRES_DB: obss
      POSTGRES_PASSWORD_FILE: /run/secrets/db_password
    secrets:
      - db_password
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U obss"]
      interval: 5s
      timeout: 5s
      retries: 5
    deploy:
      resources:
        limits:
          cpus: '4'
          memory: 8G

  redis:
    image: redis:7-alpine
    volumes:
      - redis-data:/data
    command: redis-server --requirepass ${REDIS_PASSWORD}
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 1G

  rabbitmq:
    image: rabbitmq:3.13-management
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER}
      RABBITMQ_DEFAULT_PASS_FILE: /run/secrets/rabbitmq_password
    secrets:
      - rabbitmq_password
    ports:
      - "15672:15672"  # Management UI (internal only)
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "check_port_connectivity"]
      interval: 10s
      timeout: 5s
      retries: 5
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G

  minio:
    image: minio/minio:latest
    volumes:
      - minio-data:/data
    environment:
      MINIO_ROOT_USER: ${MINIO_ROOT_USER}
      MINIO_ROOT_PASSWORD_FILE: /run/secrets/minio_password
    secrets:
      - minio_password
    command: server /data --console-address ":9001"
    ports:
      - "9000:9000"   # S3 API
      - "9001:9001"   # Console
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 10s
      timeout: 5s
      retries: 5

  opensearch:
    image: opensearchproject/opensearch:2.14
    volumes:
      - opensearch-data:/usr/share/opensearch/data
    environment:
      discovery.type: single-node
      OPENSEARCH_INITIAL_ADMIN_PASSWORD: ${OPENSEARCH_INITIAL_PASSWORD}
      DISABLE_SECURITY_PLUGIN: "false"
      plugins.security.disabled: "false"
    ports:
      - "9200:9200"   # REST API (internal)
    healthcheck:
      test: ["CMD", "curl", "-f", "-k", "https://localhost:9200/_cluster/health"]
      interval: 10s
      timeout: 5s
      retries: 5
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 4G

  keycloak:
    image: quay.io/keycloak/keycloak:24.0
    volumes:
      - keycloak-data:/opt/keycloak/data
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/obss?currentSchema=keycloak
      KC_DB_USERNAME: ${KEYCLOAK_DB_USER}
      KC_DB_PASSWORD_FILE: /run/secrets/keycloak_db_password
      KC_HOSTNAME: auth.obss.local
      KC_HTTP_ENABLED: "true"
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD_FILE: /run/secrets/keycloak_admin_password
    secrets:
      - keycloak_db_password
      - keycloak_admin_password
    command: start-dev  # Production: use "start" with proper TLS config
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
      interval: 10s
      timeout: 5s
      retries: 10
    depends_on:
      postgres:
        condition: service_healthy
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G

  # ==================== APPLICATION LAYER ====================
  obss-api:
    image: obss/api:latest
    build:
      context: ./src
      dockerfile: src/Obss.Api/Dockerfile
    volumes:
      - obss-logs:/var/log/obss
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__Postgres: "Host=postgres;Database=obss;Username=${DB_USER};Password=${DB_PASSWORD}"
      ConnectionStrings__Redis: "redis:6379,password=${REDIS_PASSWORD}"
      ConnectionStrings__RabbitMQ: "amqp://${RABBITMQ_USER}:${RABBITMQ_PASSWORD}@rabbitmq:5672/"
      ConnectionStrings__MinIO: "Endpoint=http://minio:9000;AccessKey=${MINIO_ROOT_USER};SecretKey=${MINIO_PASSWORD}"
      ConnectionStrings__OpenSearch: "https://admin:${OPENSEARCH_INITIAL_PASSWORD}@opensearch:9200"
      OpenTelemetry__OtlpEndpoint: http://otel-collector:4317
      Keycloak__Authority: http://keycloak:8080/realms/obss
      Keycloak__Audience: obss-api
      Kestrel__Endpoints__Https__Url: https://+:8443
      Kestrel__Endpoints__Https__Certificate__Path: /certs/tls.crt
      Kestrel__Endpoints__Https__Certificate__KeyPath: /certs/tls.key
    secrets:
      - db_password
      - rabbitmq_password
      - minio_password
    ports:
      - "8080:8080"   # HTTP API
      - "8443:8443"   # HTTPS API
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 10s
      timeout: 5s
      retries: 3
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      minio:
        condition: service_healthy
      keycloak:
        condition: service_healthy
    deploy:
      resources:
        limits:
          cpus: '4'
          memory: 4G
      replicas: 2  # Horizontal scaling

  # Outbox/Inbox background processors run as part of the API process
  # (IHostedService within the same container)

  # ==================== OBSERVABILITY LAYER ====================
  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    volumes:
      - ./config/otel-collector.yaml:/etc/otelcol-contrib/config.yaml
    ports:
      - "4317:4317"   # OTLP gRPC
      - "4318:4318"   # OTLP HTTP
      - "8888:8888"   # Prometheus metrics
    depends_on:
      - opensearch
      - prometheus
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 1G

  prometheus:
    image: prom/prometheus:latest
    volumes:
      - ./config/prometheus.yaml:/etc/prometheus/prometheus.yaml
      - prometheus-data:/prometheus
    ports:
      - "9090:9090"
    command:
      - '--config.file=/etc/prometheus/prometheus.yaml'
      - '--storage.tsdb.path=/prometheus'
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 2G

  grafana:
    image: grafana/grafana:latest
    volumes:
      - ./config/grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./config/grafana/datasources:/etc/grafana/provisioning/datasources
      - grafana-data:/var/lib/grafana
    environment:
      GF_SECURITY_ADMIN_PASSWORD_FILE: /run/secrets/grafana_admin_password
      GF_INSTALL_PLUGINS: grafana-piechart-panel
    secrets:
      - grafana_admin_password
    ports:
      - "3000:3000"
    depends_on:
      - prometheus
      - opensearch
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 1G

  # ==================== FRONTEND LAYER ====================
  obss-admin-portal:
    image: obss/admin-portal:latest
    build:
      context: ./frontend
      dockerfile: apps/admin-portal/Dockerfile
    environment:
      NODE_ENV: production
      NEXT_PUBLIC_API_URL: https://api.obss.local
      NEXT_PUBLIC_KEYCLOAK_URL: https://auth.obss.local
      NEXT_PUBLIC_KEYCLOAK_REALM: obss
      NEXT_PUBLIC_KEYCLOAK_CLIENT_ID: obss-admin-portal
    ports:
      - "3001:3000"
    depends_on:
      - obss-api
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 1G
      replicas: 2

  obss-customer-portal:
    image: obss/customer-portal:latest
    build:
      context: ./frontend
      dockerfile: apps/customer-portal/Dockerfile
    environment:
      NODE_ENV: production
      NEXT_PUBLIC_API_URL: https://api.obss.local
      NEXT_PUBLIC_KEYCLOAK_URL: https://auth.obss.local
      NEXT_PUBLIC_KEYCLOAK_REALM: obss
      NEXT_PUBLIC_KEYCLOAK_CLIENT_ID: obss-customer-portal
    ports:
      - "3002:3000"
    depends_on:
      - obss-api
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 1G
      replicas: 2

volumes:
  postgres-data:
  redis-data:
  rabbitmq-data:
  minio-data:
  opensearch-data:
  keycloak-data:
  prometheus-data:
  grafana-data:
  obss-logs:

secrets:
  db_password:
    file: ./secrets/db_password.txt
  rabbitmq_password:
    file: ./secrets/rabbitmq_password.txt
  minio_password:
    file: ./secrets/minio_password.txt
  keycloak_db_password:
    file: ./secrets/keycloak_db_password.txt
  keycloak_admin_password:
    file: ./secrets/keycloak_admin_password.txt
  grafana_admin_password:
    file: ./secrets/grafana_admin_password.txt
```

### 9.2 Service Topology Diagram

```
Internet
   │
   ▼
┌─────────────────────────────────────────────────────────┐
│                    Reverse Proxy (Nginx/Caddy)           │
│  api.obss.local:443  auth.obss.local:443  portal:443    │
└────────┬──────────────────────┬─────────────────────────┘
         │                      │
         ▼                      ▼
┌─────────────────┐   ┌─────────────────────┐
│   obss-api:8080  │   │  Keycloak:8080      │
│  (.NET 9 API)   │   │  (Identity Provider)│
│  ┌───────────┐  │   └─────────────────────┘
│  │ IAM       │  │
│  │ CRM       │  │         ┌─────────────────┐
│  │ Catalog   │  │         │   obss-admin     │
│  │ Orders    │  │         │   portal:3001    │
│  │ Subs      │  │         │   (Next.js)      │
│  │ Rating    │  │         └─────────────────┘
│  │ Billing   │  │
│  │ Invoices  │  │         ┌─────────────────┐
│  │ Payments  │  │         │   obss-customer  │
│  │ Collect   │  │         │   portal:3002    │
│  │ ServiceInv│  │         │   (Next.js)      │
│  │ NetInv    │  │         └─────────────────┘
│  │ Provision │  │
│  │ Workflow  │  │     ┌─────────────────────────────┐
│  │ Ticketing │  │     │   Observability Stack        │
│  │ Notify    │  │     │  ┌──────────┐ ┌──────────┐  │
│  │ Reporting │  │     │  │Prometheus│ │ Grafana  │  │
│  │ Audit     │  │     │  └────┬─────┘ └────┬─────┘  │
│  │ Gateway   │  │     │       │            │        │
│  └─────┬─────┘  │     │  ┌────▼────────────▼─────┐  │
│        │        │     │  │  OTEL Collector        │  │
└────────┼────────┘     │  └────┬───────────────────┘  │
         │              │       │                      │
         ▼              └───────┼──────────────────────┘
┌────────────────────┐         │
│  PostgreSQL 16     │         ▼
│  ┌────┐ ┌────┐    │    ┌──────────┐
│  │obss│ │keyc│    │    │OpenSearch│
│  │schema││loak│   │    └──────────┘
│  └────┘ └────┘    │
└────────────────────┘
         │
         ▼
┌────────────────────┐
│  RabbitMQ          │
│  (Event Bus)       │
└────────────────────┘
         │
         ▼
┌────────────────────┐
│  Redis             │
│  (Cache/Sessions)  │
└────────────────────┘
         │
         ▼
┌────────────────────┐
│  MinIO             │
│  (Object Store)    │
│  - Invoice PDFs    │
│  - Ticket attache  │
│  - Report exports  │
│  - Audit archives  │
└────────────────────┘
```

### 9.3 Scaling Strategy

| Component | Horizontal Scaling | Notes |
|-----------|-------------------|-------|
| `obss-api` | Yes (replicas) | Stateless. Scale based on CPU/memory. Requires connection pool tuning for PostgreSQL. |
| `obss-admin-portal` | Yes (replicas) | Next.js can be scaled. Sessions handled client-side. |
| `obss-customer-portal` | Yes (replicas) | Same as admin portal. |
| PostgreSQL | Read replicas | Primary for writes. Read replicas for reporting queries. |
| RabbitMQ | Clustered | High-availability queue mirroring for critical event types. |
| Redis | Sentinel/Cluster | Cache cluster for high availability. |
| MinIO | Distributed mode | Erasure coding for data durability. |
| OpenSearch | Clustered | Multi-node cluster for log/observability data. |

### 9.4 Environment Strategy

| Environment | Purpose | Infrastructure |
|-------------|---------|---------------|
| `development` | Local developer machines | Docker Compose, single instance |
| `testing` | CI/CD automated tests | Docker Compose in ephemeral CI |
| `staging` | Pre-production validation | Full Docker Compose with production-like data |
| `production` | Live customer traffic | Docker Compose on VMs or orchestrated via Nomad/K8s (future) |

---

## 10. Event Architecture

### 10.1 Event Schema

All events follow the **CloudEvents 1.0** specification with an OSS/BSS-specific extension context:

```json
{
  "specversion": "1.0",
  "id": "event-00000000-0000-0000-0000-000000000000",
  "source": "/obss/billing",
  "type": "com.obss.billing.billrun.completed.v1",
  "datacontenttype": "application/json",
  "tenantid": "tenant-00000000-0000-0000-0000-000000000000",
  "correlationid": "correlation-00000000-0000-0000-0000-000000000000",
  "subject": "billrun/00000000-0000-0000-0000-000000000000",
  "time": "2026-06-20T10:30:00Z",
  "data": {
    "billRunId": "br-00000000-0000-0000-0000-000000000000",
    "billingCycleId": "bc-00000000-0000-0000-0000-000000000000",
    "tenantId": "tenant-00000000-0000-0000-0000-000000000000",
    "totalAmount": 125000.00,
    "currency": "USD",
    "invoiceCount": 5432,
    "completedAt": "2026-06-20T10:30:00Z"
  }
}
```

### 10.2 Complete Event Catalog

| Event | Source Module | Published When | Consumed By |
|-------|-------------|----------------|-------------|
| `tenant.provisioned.v1` | IAM | Tenant created and ready | All modules |
| `tenant.suspended.v1` | IAM | Tenant suspended | All modules |
| `tenant.activated.v1` | IAM | Tenant reactivated | All modules |
| `user.created.v1` | IAM | New user registered | CRM |
| `account.created.v1` | CRM | Customer account created | Orders, Billing |
| `account.updated.v1` | CRM | Account details modified | Billing |
| `lead.converted.v1` | CRM | Lead converted to account | Orders |
| `offer.activated.v1` | ProductCatalog | Offer made available for sale | Orders, Subscriptions, Rating |
| `offer.deactivated.v1` | ProductCatalog | Offer retired | Orders, Subscriptions |
| `charge.definition.changed.v1` | ProductCatalog | Price plan modified | Billing |
| `order.submitted.v1` | Orders | Customer order submitted | Subscriptions, Provisioning, Billing |
| `order.completed.v1` | Orders | Order fully fulfilled | Subscriptions, ServiceInventory, Invoices |
| `order.rejected.v1` | Orders | Order validation failed | CRM |
| `subscription.activated.v1` | Subscriptions | Service subscription activated | Billing, ServiceInventory, Rating, Notifications |
| `subscription.suspended.v1` | Subscriptions | Subscription suspended | Billing, Provisioning, Rating, Notifications |
| `subscription.resumed.v1` | Subscriptions | Subscription reactivated | Billing, Provisioning, Rating |
| `subscription.terminated.v1` | Subscriptions | Subscription terminated | Billing, ServiceInventory, Provisioning, Rating |
| `subscription.amended.v1` | Subscriptions | Plan/service changed | Billing, Rating |
| `rating.completed.v1` | Rating | Usage rating cycle completed | Billing |
| `allowance.exhausted.v1` | Rating | Data/minutes allowance depleted | Notifications |
| `threshold.reached.v1` | Rating | Usage threshold crossed | Notifications |
| `billrun.completed.v1` | Billing | Billing cycle closed | Invoices |
| `charge.posted.v1` | Billing | Individual charge item posted | Invoices |
| `billing.account.created.v1` | Billing | New billing account created | Payments |
| `invoice.generated.v1` | Invoices | Invoice document created | Payments, Notifications |
| `invoice.finalized.v1` | Invoices | Invoice finalized (no more changes) | Payments, Notifications, Reporting |
| `invoice.disputed.v1` | Invoices | Customer disputed invoice | CRM, Collections |
| `invoice.payment.received.v1` | Invoices | Full/partial payment applied | Collections |
| `payment.received.v1` | Payments | Payment transaction completed | Invoices, Collections, Notifications |
| `payment.failed.v1` | Payments | Payment transaction failed | Collections, Notifications |
| `payment.refunded.v1` | Payments | Refund processed | Invoices |
| `payment.reconciled.v1` | Payments | Payment matched to invoice | Reporting |
| `collection.case.opened.v1` | Collections | Dunning case initiated | CRM, Notifications |
| `dunning.escalated.v1` | Collections | Dunning level increased | Notifications, Workflow |
| `credit.limit.exceeded.v1` | Collections | Customer exceeded credit limit | Orders, Provisioning |
| `debt.written.off.v1` | Collections | Uncollectible debt written off | Reporting, CRM |
| `service.activated.v1` | ServiceInventory | Customer service live | NetworkInventory, Notifications |
| `service.suspended.v1` | ServiceInventory | Service temporarily disabled | NetworkInventory |
| `service.terminated.v1` | ServiceInventory | Service permanently removed | NetworkInventory |
| `capacity.threshold.reached.v1` | NetworkInventory | Network resource nearing capacity | Notifications, Provisioning |
| `resource.allocation.failed.v1` | NetworkInventory | No network resources available | Provisioning, Orders |
| `provisioning.job.completed.v1` | Provisioning | Device/service configured | Orders, Subscriptions, ServiceInventory, Notifications |
| `provisioning.job.failed.v1` | Provisioning | Configuration failed | Orders, Workflow, Notifications |
| `workflow.completed.v1` | Workflow | Workflow process finished | Calling module |
| `workflow.failed.v1` | Workflow | Workflow terminated in error | Calling module, Notifications |
| `ticket.created.v1` | Ticketing | Support ticket opened | Notifications |
| `ticket.resolved.v1` | Ticketing | Support ticket resolved | CRM |
| `sla.breached.v1` | Ticketing | SLA response/resolution missed | Notifications, Workflow |
| `report.generated.v1` | Reporting | Scheduled report produced | Notifications |

### 10.3 Event Flow Diagrams

#### 10.3.1 Order Fulfillment Event Flow

```
Order Submitted
  │
  ▼
┌─────────────────────────────────────────────────┐
│ OrderSubmittedEvent                              │
│ ┌──────────────┐ ┌───────────┐ ┌──────────────┐ │
│ │ Subscriptions│ │Provisioning│ │   Billing    │ │
│ │ Create       │ │ Create Job │ │ Create Pending│ │
│ │ PendingSub   │ │            │ │ Charges      │ │
│ └──────┬───────┘ └─────┬─────┘ └──────┬───────┘ │
│        │               │              │          │
│        ▼               ▼              ▼          │
│    (awaiting      ProvisioningJob              │
│     activation)    CompletedEvent               │
│                      │                          │
│                      ▼                          │
│              ┌──────────────────┐               │
│              │SubscriptionActi- │               │
│              │vatedEvent        │               │
│              └────┬──────┬─────┘               │
│                   │      │                     │
│                   ▼      ▼                     │
│           ┌────────┐ ┌─────────┐               │
│           │Service │ │Billing  │               │
│           │Inventory│ │Activate │               │
│           │Create   │ │Charges  │               │
│           │Instance │ │         │               │
│           └─────────┘ └─────────┘               │
│                   │                             │
│                   ▼                             │
│           ┌────────────┐                        │
│           │ Order       │                        │
│           │ Completed   │                        │
│           │ Event       │                        │
│           └────────────┘                        │
└─────────────────────────────────────────────────┘
```

#### 10.3.2 Billing Cycle Event Flow

```
Monthly Billing Cycle Trigger (Scheduler)
  │
  ▼
BillRunStartedEvent
  │
  ▼
┌──────────────────────────────────────────────────┐
│ RatingCompletedEvent (if async rating needed)    │
│   │                                              │
│   ▼                                              │
│ BillRun collects:                                │
│   - Recurring charges from Subscriptions         │
│   - Rated usage from Rating                      │
│   - One-time charges from Orders                 │
│   │                                              │
│   ▼                                              │
│ BillRunCompletedEvent                            │
│   │                                              │
│   ▼                                              │
│ InvoiceGenerationEvent                           │
│   │                                              │
│   ▼                                              │
│ ┌─────────────────────────────┐                  │
│ │ For each billing account    │                  │
│ │  - Generate Invoice         │                  │
│ │  - Post InvoiceGeneratedEvent│                 │
│ │  - Notify customer          │                  │
│ └─────────────────────────────┘                  │
│   │                                              │
│   ▼                                              │
│ Payments: mark invoice for collection            │
│ Reporting: update billing data marts             │
└──────────────────────────────────────────────────┘
```

#### 10.3.3 Dunning/Collections Event Flow

```
Overdue Invoice Detected
  │
  ▼
CollectionCaseOpenedEvent (Level 1 Dunning)
  │
  ▼
┌────────────────────────────────────────┐
│ Dunning Workflow Started               │
│  │                                     │
│  ├── Day 1: Payment Reminder (SMS/Email)│
│  ├── Day 7: Follow-up (Email)          │
│  ├── Day 15: Late Notice (Email/SMS)   │
│  ├── Day 21: Final Notice (Email/SMS)  │
│  │                                     │
│  ├── If Payment Received at any point: │
│  │   PaymentReceivedEvent              │
│  │   → CollectionCaseResolvedEvent     │
│  │   → Case Closed                     │
│  │                                     │
│  └── If No Payment by Day 30:          │
│      DunningEscalatedEvent (Level 2)   │
│      → Human collector assigned        │
│      → Phone calls initiated           │
│      → Promise-to-Pay negotiation      │
│        │                               │
│        ├── Promise Made → PromiseToPay │
│        │   MadeEvent                   │
│        │   ├── Promise Kept → Case Resolved│
│        │   └── Promise Broken → Level 3│
│        │                               │
│        └── Level 3: Service Restriction│
│            → CreditLimitExceededEvent  │
│            → Provisioning suspends svc │
│            → Level 4: Legal/Escalation │
│            → Level 5: Write-off        │
│              → DebtWrittenOffEvent     │
└────────────────────────────────────────┘
```

### 10.4 Outbox Pattern Implementation

```
┌────────────────────────────────────────────────────────────┐
│                    COMMAND HANDLER                          │
│  1. Begin transaction                                      │
│  2. Modify aggregate(s)                                    │
│  3. Collect domain events from aggregate                    │
│  4. Save aggregate to database                              │
│  5. Write domain events to outbox table (same transaction)  │
│  6. Commit transaction                                      │
│                                                            │
│  ┌──────────────────────────────┐                           │
│  │ obss.outbox_messages         │                           │
│  │ id              │ uuid   PK │                           │
│  │ tenant_id       │ uuid      │                           │
│  │ aggregate_type  │ varchar   │                           │
│  │ aggregate_id    │ varchar   │                           │
│  │ event_type      │ varchar   │                           │
│  │ event_data      │ jsonb     │                           │
│  │ created_at      │ timestamp │                           │
│  │ processed_at    │ timestamp │ nullable                   │
│  │ retry_count     │ int       │ default 0                 │
│  │ error           │ text      │ nullable                   │
│  └──────────────────────────────┘                           │
└────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────┐
│              OUTBOX BACKGROUND PROCESSOR                   │
│  (IHostedService, runs every 1 second)                     │
│                                                            │
│  1. SELECT * FROM obss.outbox_messages                     │
│     WHERE processed_at IS NULL                             │
│     AND retry_count < 10                                   │
│     ORDER BY created_at ASC                                │
│     LIMIT 100                                              │
│     FOR UPDATE SKIP LOCKED                                 │
│                                                            │
│  2. For each message:                                      │
│     a. Serialize event to CloudEvents format               │
│     b. Publish to RabbitMQ exchange (topic)                │
│     c. On success: UPDATE processed_at = NOW()             │
│     d. On failure: UPDATE retry_count += 1,                │
│                    error = exception message               │
│     e. After 10 retries: move to dead_letter table; alert  │
│                                                            │
│  3. Monitoring:                                            │
│     - Metric: outbox_pending_messages (gauge)             │
│     - Metric: outbox_publish_duration (histogram)         │
│     - Metric: outbox_retry_count (counter)                │
│     - Alert: outbox backlog > 1000 for > 5 minutes        │
└────────────────────────────────────────────────────────────┘
```

### 10.5 Inbox Pattern Implementation

```
┌────────────────────────────────────────────────────────────┐
│              RABBITMQ CONSUMER (Event Handler)              │
│  1. Receive message from queue                              │
│  2. Deserialize CloudEvents envelope                        │
│  3. Check event.type for known event types                  │
│  4. Extract TenantId from envelope                          │
│  5. Check Inbox for duplicate EventId                       │
│                                                            │
│  ┌───────────────────────────────┐                          │
│  │ obss.inbox_messages           │                          │
│  │ id               │ uuid   PK  │                          │
│  │ event_id         │ uuid   UQ  │                          │
│  │ tenant_id        │ uuid       │                          │
│  │ event_type       │ varchar    │                          │
│  │ event_data       │ jsonb      │                          │
│  │ received_at      │ timestamp  │                          │
│  │ processed_at     │ timestamp  │ nullable                  │
│  │ retry_count      │ int        │ default 0                │
│  │ error            │ text       │ nullable                  │
│  └───────────────────────────────┘                          │
│                                                            │
│  6. If event_id EXISTS in inbox:                            │
│     → ACK message (already processed)                       │
│     → Done (idempotent)                                     │
│                                                            │
│  7. If event_id NOT FOUND:                                  │
│     a. Insert inbox record (received_at = NOW())            │
│     b. BEGIN transaction                                    │
│     c. Execute business logic (command handler)             │
│     d. UPDATE inbox SET processed_at = NOW()                │
│     e. COMMIT transaction                                   │
│     f. ACK RabbitMQ message                                 │
│                                                            │
│  8. On failure:                                             │
│     a. UPDATE retry_count += 1, error = exception          │
│     b. NACK message (re-queue with delay)                   │
│     c. After 10 retries: move to dead letter queue          │
│     d. Alert on dead letter                                 │
└────────────────────────────────────────────────────────────┘
```

### 10.6 Dead Letter Queue Strategy

| Queue | Dead Letter Exchange | Handling |
|-------|---------------------|----------|
| `obss.billing.*` | `obss.dlx.billing` | Retry 10x with exponential backoff (1s, 2s, 4s, 8s, ...), then DLQ |
| `obss.provisioning.*` | `obss.dlx.provisioning` | Retry 5x with longer backoff (5s, 10s, 20s, 40s, 80s), then DLQ |
| `obss.notifications.*` | `obss.dlx.notifications` | Retry 3x, then DLQ (notifications can be ephemeral) |

DLQ messages are:
1. Written to `obss.dead_letter_log` table (for auditing)
2. Alert raised to operations team
3. Available for replay from admin UI (after fixing the underlying issue)

### 10.7 RabbitMQ Topology

```
Exchanges:
  ┌──────────────────────┐
  │ obss.events (topic)  │ → All platform events
  └──────────┬───────────┘
             │
             ├── routing key: "iam.tenant.*"        → iam-queue
             ├── routing key: "crm.account.*"       → crm-queue
             ├── routing key: "orders.*"            → orders-queue
             ├── routing key: "subscriptions.*"     → subscriptions-queue
             ├── routing key: "rating.*"            → rating-queue
             ├── routing key: "billing.*"           → billing-queue
             ├── routing key: "invoices.*"          → invoices-queue
             ├── routing key: "payments.*"          → payments-queue
             ├── routing key: "collections.*"       → collections-queue
             ├── routing key: "service.*"           → service-inv-queue
             ├── routing key: "network.*"           → network-inv-queue
             ├── routing key: "provisioning.*"      → provisioning-queue
             ├── routing key: "workflow.*"          → workflow-queue
             ├── routing key: "ticketing.*"         → ticketing-queue
             ├── routing key: "notifications.*"     → notifications-queue
             └── routing key: "reporting.*"         → reporting-queue

Dead Letter Exchanges:
  ┌─────────────────────────┐
  │ obss.dlx (topic)        │ → All dead lettered events
  └─────────────────────────┘

  Each module's queue is bound to obss.dlx with routing key matching
  their module name for selective replay.
```

### 10.8 Event Versioning

- Events are versioned in the `type` field: `com.obss.billing.billrun.completed.v1`
- When schema changes are needed, a new version is created: `com.obss.billing.billrun.completed.v2`
- Old versions continue to be published for at least one major release cycle
- Consumers declare which versions they support
- Backward-compatible changes (adding fields) do not require a new version — consumers ignore unknown fields

---

## Appendix A: Module Assembly Structure

Each bounded context follows this consistent assembly structure:

```
Obss.Modules.{ModuleName}/
├── Domain/
│   ├── Aggregates/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── DomainEvents/
│   ├── Repositories/          (interfaces only)
│   ├── Specifications/
│   └── Exceptions/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── EventHandlers/         (internal domain event handlers)
│   ├── IntegrationEventHandlers/  (cross-module event handlers, via Inbox)
│   ├── Services/              (application service interfaces + implementations)
│   ├── DTOs/
│   ├── Mappings/
│   └── Validators/
├── Infrastructure/
│   ├── Persistence/
│   │   ├── DbContext.cs
│   │   ├── Configurations/    (EF Core entity type configurations)
│   │   ├── Repositories/      (implementations)
│   │   ├── Migrations/
│   │   └── Interceptors/
│   ├── Messaging/
│   │   ├── OutboxBackgroundService.cs
│   │   ├── EventPublisher.cs
│   │   └── EventHandlers/     (RabbitMQ consumer registration)
│   ├── Services/              (infrastructure service implementations)
│   └── DependencyInjection.cs (ServiceCollection extension methods)
├── Api/
│   ├── Controllers/
│   ├── Endpoints/             (if using minimal API)
│   ├── Middleware/
│   └── ModuleRegistration.cs
└── Tests/
    ├── UnitTests/
    ├── IntegrationTests/
    └── ArchitectureTests/
```

## Appendix B: Technology Stack Reference

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| Runtime | .NET | 9.0 | Application runtime |
| Web Framework | ASP.NET Core | 9.0 | HTTP API, minimal APIs, middleware |
| ORM | Entity Framework Core | 9.0 | Data access, migrations, query filters |
| CQRS Framework | MediatR | 12.x | In-process commands, queries, events |
| Validation | FluentValidation | 11.x | Input validation |
| Mapping | Mapster | 7.x | Entity ↔ DTO mapping |
| UI Framework | Next.js | 14.x | React-based frontend |
| UI Components | ShadCN UI | latest | Radix-based component library |
| Styling | Tailwind CSS | 3.x | Utility-first CSS |
| Forms | React Hook Form | 7.x | Form management |
| Server State | TanStack Query | 5.x | Server state management |
| Client State | Zustand | 4.x | Lightweight client state |
| Database | PostgreSQL | 16 | Primary database |
| Cache | Redis | 7 | Distributed cache, sessions, rate limiting |
| Message Broker | RabbitMQ | 3.13 | Event bus, async messaging |
| Object Store | MinIO | latest | Invoice PDFs, attachments, report exports |
| Search/Observability | OpenSearch | 2.14 | Log aggregation, audit search, reporting |
| Identity | Keycloak | 24.0 | OIDC provider, user federation, SSO |
| Tracing | OpenTelemetry | 1.x | Distributed tracing, metrics, logs |
| Metrics | Prometheus | latest | Metrics collection and alerting |
| Dashboards | Grafana | latest | Visualization and dashboards |
| Container Runtime | Docker | latest | Containerization |
| Orchestration | Docker Compose | latest | Local and production deployment |

## Appendix C: Data Ownership Map

| Schema | Owned By | Tables |
|--------|----------|--------|
| `iam` | IAM Module | tenants, users, roles, permissions, role_permissions, user_roles, refresh_tokens, api_clients, client_permissions, tenant_settings |
| `crm` | CRM Module | accounts, contacts, leads, lead_sources, interactions, customer_segments, address_book |
| `catalog` | ProductCatalog Module | products, product_specifications, offers, offer_prices, offer_discounts, charge_definitions, bundles, bundle_items, eligibility_rules |
| `orders` | Orders Module | orders, order_items, order_charges, order_status_history, order_validation_results |
| `subscriptions` | Subscriptions Module | subscriptions, subscription_items, subscription_status_history, subscription_charges, subscription_amendments |
| `rating` | Rating Module | usage_records, rated_charges, discount_applications, allowance_consumption, rating_errors, raw_cdrs |
| `billing` | Billing Module | billing_accounts, billing_cycles, bill_runs, bill_run_items, charge_items, adjustments, debit_notes, credit_notes, pro_ration_rules |
| `invoices` | Invoices Module | invoices, invoice_items, invoice_taxes, invoice_status_history, invoice_disputes, invoice_delivery_log, invoice_templates |
| `payments` | Payments Module | payment_transactions, payment_methods, payment_gateway_logs, payment_reconciliation, refunds, wallet_transactions, mandates |
| `collections` | Collections Module | collection_cases, dunning_cycles, dunning_actions, promises_to_pay, credit_limits, credit_limit_logs, escalation_rules, collection_notes |
| `inventory` | ServiceInventory Module | service_instances, service_characteristics, service_connections, service_endpoints, service_dependencies, service_topology |
| `network` | NetworkInventory Module | sites, devices, device_ports, connections, ip_pools, ip_allocations, vlan_pools, vlan_allocations, bandwidth_pools, capacity_reservations, network_topology |
| `provisioning` | Provisioning Module | provisioning_jobs, provisioning_steps, provisioning_templates, device_credentials, provisioning_logs, activation_schedules |
| `workflow` | Workflow Module | workflow_definitions, workflow_instances, workflow_states, workflow_transitions, activity_definitions, activity_executions, workflow_timers, workflow_errors |
| `ticketing` | Ticketing Module | tickets, ticket_messages, ticket_attachments, ticket_categories, ticket_priorities, ticket_slas, ticket_assignments, ticket_status_history, sla_violations |
| `notifications` | Notifications Module | notification_templates, notification_messages, notification_delivery_log, notification_preferences, webhook_endpoints, webhook_delivery_log |
| `reporting` | Reporting Module | report_definitions, scheduled_reports, report_executions, report_outputs, dashboard_definitions, data_marts, report_snapshots, report_subscriptions |
| `audit` | Audit Module | audit_logs, audit_retention_policies, audit_archives |
| `gateway` | APIGateway Module | api_routes, rate_limit_rules, api_usage_logs, api_key_usage |
| `keycloak` | IAM (Keycloak) | Keycloak-managed tables (user federation, sessions, etc.) |

---

*This architecture document is a living artifact. All architectural decisions must be recorded as Architecture Decision Records (ADRs) in `/docs/architecture/decisions/`. Any deviation from the principles and patterns defined herein requires a ratified ADR approved by the Chief Architect.*
