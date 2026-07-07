# Repository Inventory — OBSS Platform

## Overview

A .NET 9 modular monolith with 22 business modules plus shared kernel and building blocks.
Frontend: Next.js 16 (App Router, React 19, Tailwind CSS 4, TypeScript 5).

## Module Inventory (22 Modules)

| # | Module | Backend Layers | Frontend Pages | API Path Prefix | DB Schema | TMF API Mapping |
|---|--------|---------------|----------------|----------------|-----------|-----------------|
| 1 | **IAM** | Domain/App/Infra/Api | `admin/` (users, roles, tenants) | `/iam` | `iam` | TMF632 Party, TMF669 Party Role |
| 2 | **CRM** | Domain/App/Infra/Api | `customers/` | `/crm` | `crm` | TMF629 Customer |
| 3 | **ProductCatalog** | Domain/App/Infra/Api | `catalogs/`, `products/`, `product-specifications/`, `catalogs/offers/` | `/catalog` | `catalog` | TMF620 Product Catalog |
| 4 | **Orders** | Domain/App/Infra/Api | `orders/` | `/orders` | `orders` | TMF622 Product Ordering |
| 5 | **Subscriptions** | Domain/App/Infra/Api | `subscriptions/` | `/subscriptions` | `subscriptions` | TMF637 Product Inventory |
| 6 | **ServiceCatalog** | Domain/App/Infra/Api | `service-catalog/` | `/service-catalog` | `service_catalog` | TMF633 Service Catalog |
| 7 | **ServiceInventory** | Domain/App/Infra/Api | `service-inventory/` | `/service-inventory` | `service_inventory` | TMF638 Service Inventory |
| 8 | **Provisioning** | Domain/App/Infra/Api | `provisioning/` | `/provisioning` | `provisioning` | — (supports TMF641) |
| 9 | **Billing** | Domain/App/Infra/Api | `billing/` | `/billing` | `billing` | TMF666 Account Mgmt |
| 10 | **Invoices** | Domain/App/Infra/Api | `invoices/` | `/invoices` | `invoices` | TMF678 Customer Bill |
| 11 | **Payments** | Domain/App/Infra/Api | `payments/` | `/payments` | `payments` | TMF676 Payment Mgmt |
| 12 | **Collections** | Domain/App/Infra/Api | `collections/` | `/collections` | `collections` | TMF676 (subsidiary) |
| 13 | **Rating** | Domain/App/Infra/Api | `rating/` | `/rating` | `rating` | TMF677 Usage Consumption |
| 14 | **NetworkInventory** | Domain/App/Infra/Api | `network/` | `/network` | `network_inventory` | TMF639 Resource Inventory |
| 15 | **NumberInventory** | Domain/App/Infra/Api | `number-inventory/` | `/number-inventory` | `number_inventory` | TMF639 Resource Inventory |
| 16 | **Workflow** | Domain/App/Infra/Api | `workflow/` | `/workflow` | `workflow` | TMF701 Process Flow |
| 17 | **Ticketing** | Domain/App/Infra/Api | `tickets/` | `/ticketing` | `ticketing` | TMF702 Task Management |
| 18 | **Notifications** | Domain/App/Infra/Api | `notifications/` | `/notifications` | `notifications` | TMF681 Communication |
| 19 | **Audit** | Domain/App/Infra/Api | `audit/` | `/audit` | `audit` | — (supporting) |
| 20 | **Reporting** | Domain/App/Infra/Api | `reporting/` | `/reporting` | `reporting` | — (supporting) |
| 21 | **ApiGateway** | Domain/App/Infra/Api | `api-gateway/` | `/gateway` | `gateway` | — (infrastructure) |
| 22 | **EventManagement** | Domain/App/Infra/Api | — | `/events` | `event_management` | TMF688 Event Mgmt |

## Shared Infrastructure

| Component | Location | Purpose |
|-----------|----------|---------|
| **Obss.SharedKernel** | `src/Shared/Obss.SharedKernel/` | Entity, AggregateRoot, ValueObject, DomainEvent, IntegrationEvent, Repository, UnitOfWork, Outbox/Inbox, FieldSelector, Pagination |
| **Obss.ModuleRegistration** | `src/BuildingBlocks/Obss.ModuleRegistration/` | Auto-discovery module registration interface |
| **Obss.Host** | `src/Host/Obss.Host/` | Entry point: 22 DbContexts, MediatR, FluentValidation, Mapster, Keycloak auth, API key middleware, RabbitMQ outbox, OpenTelemetry |

## Frontend Structure

| Layer | Location | Purpose |
|-------|----------|---------|
| Generated Client | `src/api/generated/` | 94 DTO interfaces, ~240 command types (generated) |
| Hooks | `src/api/hooks/` | 177 React Query hooks (1 per API operation) |
| Forms | `src/forms/` | Form schemas + components |
| Pages | `src/app/*/` | One directory per module with list/detail/create/edit pages |
| Types | `src/types/` | Shared type definitions |
| Stores | `src/stores/` | Zustand stores (auth, UI state) |
| Services | `src/services/api.ts` | Base API service with auth headers |
| Components | `src/components/` | Shared UI components, layout (Sidebar, Header, AppShell) |

## Database

22 PostgreSQL schemas in a single database instance, each with independent EF Core migrations.

## Messaging

RabbitMQ via outbox pattern. Integration events flow between modules:
- Orders → Subscriptions (SubscriptionRequired)
- Orders → Provisioning (ProvisioningRequired)
- Orders → All (OrderSubmitted, OrderApproved)
- Various → Notifications via integration event handlers
