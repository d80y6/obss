# WP-019: Add Product Inventory (TMF637) to Subscriptions Module

**Goal:** Add a TMF637-compliant `Product` aggregate root to the Subscriptions module, making it Product Inventory. `Subscription` remains as-is (lifecycle/entitlements) and references `Product` via its existing `ProductId` FK.

**Architecture:** New `Product` aggregate root alongside existing `Subscription`. No structural changes to `Subscription` — only the creation path changes to create `Product` first, then `Subscription` references it. TMF637 fields live on `Product`.

## New Domain Entities

### Product (AggregateRoot<Guid>)
- `Id`, `TenantId`, `CustomerId`, `Name`, `Description`, `ProductSpecificationId` (ref ProductCatalog), `ProductOfferingId` (ref ProductCatalog)
- `Status` (enum: Created, Active, Suspended, Cancelled, Terminated)
- `BillingAccountRef` (value object: `BillingAccountRef { AccountId, Href }`)
- `Place` (owned value object: `Place { Id, Role, Name, Street, City, State, Zip, Country }`)
- `AgreementRef` (owned value object: `AgreementRef { AgreementId, Name, Href }`)
- Collections: `ProductRelationship[]`, `ProductCharacteristic[]`, `ProductPrice[]`, `ProductTerm[]`, `RealizingService[]`, `RealizingResource[]`

### ProductRelationship (Entity<Guid>)
- `Id`, `ProductId`, `RelatedProductId`, `Type` (enum: ReliesOn, IsReliedOnBy, Bundled, SubProduct)

### ProductCharacteristic (Entity<Guid>)
- `Id`, `ProductId`, `Name`, `Value`, `ValueType`

### ProductPrice (Entity<Guid>)
- `Id`, `ProductId`, `PriceType`, `Name`, `Amount`, `Currency`, `RecurringPeriod`, `RecurringPeriodUnit`

### ProductTerm (Entity<Guid>)
- `Id`, `ProductId`, `Name`, `Description`, `Duration`, `DurationUnit`, `StartDate`, `EndDate`

### RealizingService (Entity<Guid>)
- `Id`, `ProductId`, `ServiceId`, `ServiceType`, `Status`

### RealizingResource (Entity<Guid>)
- `Id`, `ProductId`, `ResourceId`, `ResourceType`, `Status`

## Changes to Subscription

### Subscription.ProductId
- Already exists — unchanged behavior
- `CreateSubscriptionCommand` handler will create `Product` first, pass `Product.Id` to `Subscription.Create`

### SubscriptionService
- Already links to ServiceInventory — this becomes the `RealizingService` link on `Product` too
- Eventually `SubscriptionService` can be removed in favor of `Product.RealizingServices`

## Domain Events
- `ProductCreatedDomainEvent` (ProductId, CustomerId, ProductOfferingId)
- `ProductActivatedDomainEvent`
- `ProductSuspendedDomainEvent`
- `ProductCancelledDomainEvent`
- `ProductTerminatedDomainEvent`
- `ProductModifiedDomainEvent`

## Enums
- `ProductStatus` (in Domain): Created, Active, Suspended, Cancelled, Terminated
- `ProductRelationshipType` (in Domain): ReliesOn, IsReliedOn, Bundled, SubProduct
- `PriceType` (in Domain): OneTime, Recurring, Usage
- `DurationUnit` (in Domain): Days, Months, Years

## Value Objects
- `BillingAccountRef` (record)
- `Place` (record)
- `AgreementRef` (record)

## Application Layer

### Repository
- `IProductRepository` — `GetByIdAsync`, `GetListAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `CountAsync`

### DTOs
- `ProductDto` — flat dto with all child collections as lists
- `ProductRelationshipDto`, `ProductCharacteristicDto`, `ProductPriceDto`, `ProductTermDto`, `RealizingServiceDto`, `RealizingResourceDto`

### Commands
- `CreateProductCommand` — name, description, specId, offeringId, billingAccount, place, agreement
- `UpdateProductCommand` — update details
- `ActivateProductCommand` — state transition
- `SuspendProductCommand`
- `CancelProductCommand`

### Queries
- `GetProductByIdQuery`
- `GetProductsQuery` — filterable by status, customerId
- `GetProductsByCustomerQuery`

### Integration Events (outbound)
- `ProductCreatedIntegrationEvent`
- `ProductStateChangedIntegrationEvent`

### Event Handler (inbound)
- `ProvisioningJobCompletedEventHandler` — update Product realizing service status

## Infrastructure

### EF Configuration
- `ProductConfiguration` — table `products`, owned Place/AgreementRef/BillingAccountRef, HasMany for all child collections
- `ProductRelationshipConfiguration`
- `ProductCharacteristicConfiguration`
- `ProductPriceConfiguration`
- `ProductTermConfiguration`
- `RealizingServiceConfiguration`
- `RealizingResourceConfiguration`

### Repository
- `ProductRepository`

### Migration
- `AddProductInventoryTables` — creates `products`, `product_relationships`, `product_characteristics`, `product_prices`, `product_terms`, `realizing_services`, `realizing_resources` tables

## API Endpoints (under `/api/v1/subscriptions`)
- `POST /products` — create
- `GET /products/{id}` — by ID
- `GET /products` — list (paginated)
- `PATCH /products/{id}` — update
- `POST /products/{id}/activate`
- `POST /products/{id}/suspend`
- `POST /products/{id}/cancel`
- `GET /customers/{customerId}/products` — by customer

## Order Flow Update
- `CreateSubscriptionCommand` handler → creates `Product` → creates `Subscription` referencing Product.Id
- `SubscriptionRequiredIntegrationEventHandler` → creates `Product` then `Subscription`
- Existing `CreateSubscription` endpoint remains unchanged (now creates Product first)

## Scope Includes

### Frontend
- New "Products" page under Subscriptions section showing product inventory list/detail
- Customer product list (view-only, no CRUD)
- Product-to-service mapping display on product detail page
- Read-only, no product management UI (managed via API/internal flows)

### Data Migration
- Migration script to create Product records from all existing `Subscription` records
- One Product per existing Subscription, copying relevant fields
- Script lives in `scripts/migrations/subscriptions-to-products.sql`

### UI for Product-to-Service Mapping
- Product detail page shows `RealizingService` and `RealizingResource` lists
- Read-only display linked from ServiceInventory data

## Not In Scope
- Product management CRUD UI (backend API only for management)
- Subscription replacement — Subscription module still exists, Product supplements it