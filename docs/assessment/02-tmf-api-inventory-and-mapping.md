# TMF API Inventory & Module Mapping

## Applicable TM Forum APIs

| TMF API | Name | Version | Module(s) | Compliance Score | Status |
|---------|------|---------|-----------|-----------------|--------|
| TMF620 | Product Catalog Management | 5.x | ProductCatalog | **90%** | Minor Gaps |
| TMF622 | Product Ordering | 5.x | Orders | **85%** | Minor Gaps |
| TMF629 | Customer Management | 5.x | CRM | **75%** | Partial |
| TMF632 | Party Management | 5.x | IAM (User) | **40%** | Major Gaps |
| TMF633 | Service Catalog | 5.x | ServiceCatalog | **85%** | Minor Gaps |
| TMF637 | Product Inventory | 5.x | Subscriptions | **50%** | Major Gaps |
| TMF638 | Service Inventory | 5.x | ServiceInventory | **80%** | Partial |
| TMF639 | Resource Inventory | 5.x | NetworkInventory, NumberInventory | **45%** | Major Gaps |
| TMF641 | Service Ordering | 5.x | Provisioning | **30%** | Major Gaps |
| TMF645 | Service Qualification | 5.x | — | **0%** | Not Implemented |
| TMF648 | Quote Management | 5.x | — | **0%** | Not Implemented |
| TMF651 | Agreement | 5.x | CRM (partial) | **15%** | Major Gaps |
| TMF666 | Account Management | 5.x | Billing | **55%** | Partial |
| TMF669 | Party Role | 5.x | IAM (PartyRole) | **60%** | Partial |
| TMF676 | Payment Management | 5.x | Payments | **75%** | Partial |
| TMF677 | Usage Consumption | 5.x | Rating | **60%** | Partial |
| TMF678 | Customer Bill | 5.x | Invoices | **70%** | Partial |
| TMF681 | Communication Management | 5.x | Notifications | **50%** | Major Gaps |
| TMF688 | Event Management | 5.x | EventManagement | **45%** | Major Gaps |
| TMF701 | Process Flow Management | 5.x | Workflow | **35%** | Major Gaps |
| TMF702 | Task Management | 5.x | Ticketing | **40%** | Major Gaps |

## Detailed Module → TMF Mapping

### 1. IAM → TMF632 (Party Management) + TMF669 (Party Role) — **40% / 60%**

**What maps:**
- `User` entity maps to TMF632 `Individual` (has title, middleName, birthDate, nationalId, preferredLanguage, gender)
- `PartyRole` entity maps to TMF669 `PartyRole` (has partyId, roleId, status lifecycle, validFrom/validUntil)
- `Tenant` entity maps loosely to TMF632 `Organization`
- CRUD operations exist for all

**What's missing (TMF632):**
- TMF632 `Individual` has `fullName`, `givenName`, `familyName`, `geographicAddress`, `contactMedium`, `characteristic` — User has FirstName/LastName but no address, no contact mediums
- No `Organization` resource separate from Tenant (org legal details, registration, etc.)
- No `TaxExemptionCertificate` on party
- No `ExternalIdentifier` structure on Individual
- No `Disability` / `MaritalStatus` / `Nationality` / `CountryOfBirth` fields
- No related parties structure
- No `Skill` / `LanguageAbility` entities
- No party `Characteristic` (dynamic key-value pairs)
- No `IndividualIdentification` (identity documents are in CRM, not IAM)
- No `PartyName` structure — just flat FirstName/LastName
- No `Medium` (contact medium with characteristics) — CRM has ContactMedium but not linked to IAM User
- Party lifecycle events (individual created/updated/merged) — domain events exist but no integration events
- No merge/split party operations
- No field-level patch (PATCH) — only full PUT

**What's missing (TMF669):**
- `PartyRole` entity exists with basic CRUD
- No `PartyRole` valid for (validFrom/validUntil on the entity but handler doesn't apply them in UpdatePartyRoleHandler)
- No `PartyRole` characteristics
- No `PartyRole` related party
- No `PartyRole` engagement history
- No integration events for party role changes

### 2. CRM → TMF629 (Customer Management) — **75%**

**What maps:**
- `Customer` aggregate root with BillingAccountRef, ContactMedium, RelatedParty, Characteristic, CreditProfile
- `Individual` and `Organization` as separate aggregates
- `Agreement` aggregate with agreement type, parties
- `CustomerSegment` with criteria-based rules
- Status lifecycle: Lead → Prospect → Active → Suspended → Terminated
- Contact mediums (Email, Sms, Phone, Postal, Social, Web, Chat)
- Notification hubs (Email, Sms, Push, InApp, Webhook)
- CRUD operations, search with filtering

**What's missing:**
- No `CustomerAccount` resource (billing account refs exist but no standalone account management)
- No `AppliedCustomerBillingTaxRate`
- No `CustomerAgreement` (Agreement exists as separate aggregate, not linked to Customer directly — only via AgreementRef VO)
- No `PaymentMethod` on Customer (PaymentMethodRef exists but no full payment method management)
- No shopping cart
- No loyalty/bonus points
- No customer `Engagement` events
- TMF `@baseType`, `@schemaLocation`, `@type` not consistently applied on all DTOs
- No `ExternalIdentifier` standard structure
- No `BillingAccount` full resource — only refs
- No PATCH for customer — partial updates go through `PartialUpdateCustomer` but not standard TMF PATCH
- No `Customer` characteristic dynamic fields on the DTO level
- Customer events: domain events exist but no integration events published for cross-module consumption

### 3. ProductCatalog → TMF620 (Product Catalog) — **90%**

**What maps:**
- `ProductSpecification` with characteristics, characteristicValues, relationships (dependency/substitution/exclusion/optional)
- `Product` offering with category, productSpec ref, lifecycle
- `Category` with hierarchy (parent category, sort order, lifecycle)
- `Catalog` with catalogType, lifecycle
- `ProductOffering` (Offer) with pricing models (flat/tiered/volume), discounts (percentage/fixed/freePeriod), terms (minimum contract/renewal/cancellation), bundled offerings
- `PriceRange` for tiered pricing
- Product options with configuration rules (Required/Allowed/Excluded/Dependent)
- Lifecycle state machine: Draft → Active → Retired → Discontinued
- TMF `@type` on some entities
- Composite pattern for bundles
- Full CRUD for all resources
- Field selection (?fields=) via FieldSelector
- TMF-standard pagination

**What's missing:**
- No `ProductOfferingPrice` relationship to `ProductOfferingTerm` (pricings and terms are separate)
- `ProductSpecification` doesn't use TMF standard `ProductSpecificationCharacteristic` value type mapping for min/max as separate fields
- No `ProductOffering` characteristic/feature groups
- No `ProductOfferingPrice` `priceType` (recurring/one-time/usage) on the TMF Pricing model split
- No `Agreement` refs on product offerings
- No `ProductOffering` `MarketSegment` / `Place` associations
- No `Constraint` resource for business rules
- No `ProductSpecification` `Attachment` support
- No `ProductSpecification` `ResourceSpecification` ref on product spec
- No bundle `Pricing` relationship to ProductOfferingPrice
- `@baseType`, `@schemaLocation` not present on all entities/DTOs
- No `ExternalIdentifier` standard structure
- No TMF-standard `RelatedParty` on Catalog/Category
- No `Category` `ProductOffering` refs on category (only product refs)
- No events for spec/offer lifecycle changes published as integration events

### 4. Orders → TMF622 (Product Ordering) — **85%**

**What maps:**
- Order with order items, billing/shipping addresses, related parties
- State machine: Draft → Submitted → PendingApproval → Approved → Rejected → Fulfilling → Completed / Cancelled
- Order items with product/offer refs, quantity, pricing
- Fulfillment tracking
- TMF fields: `href`, `@type`, `@baseType`, `@schemaLocation`, `externalId`, `quoteId`, `notificationContact`, `priority`, `requestedStartDate`, `requestedCompletionDate`, `expectedCompletionDate`
- Order item actions, service type
- Related parties
- Payment tracking per order
- Domain events: submitted, approved, cancelled, completed
- Integration events published to Subscriptions + Provisioning modules
- Order validation before submission

**What's missing:**
- No `CancelOrder` task resource (cancellation uses simple POST action)
- No `OrderInformationRequired` event pattern
- No order milestone/milestone tracking
- No order `Agreement` refs
- No order `BillingAccount` ref (only customer billing address)
- No order `Note` entities (only flat text field)
- No order `Payment` refs linked to actual Payment module
- No `Pricing` details on order items (only unit price + recurring price)
- No `Product` object on order items (only ProductId)
- No TMF-standard `Error` response for validation failures — custom error format
- No `Order` `Version` tracking for updates
- No order `Channel` as structured enum (stored as free string)
- Approval workflow is synchronous, no async approval routing
- No `Order` batch operations
- No `Order` characterstics / dynamic fields

### 5. Subscriptions → TMF637 (Product Inventory) — **50%**

**What maps:**
- Subscription with lifecycle (Pending → Active → Suspended → Cancelled → Expired)
- Subscription offerings (offer change, quantity change)
- Entitlements with usage tracking and limits
- Related parties on subscription
- Billing period on subscription
- Add-ons

**What's missing (major):**
- TMF637 is about `Product` in inventory, not `Subscription`. Subscriptions module acts as a hybrid Product Inventory + Subscription Management
- No `Product` resource as defined by TMF637 (product procured by customer)
- No `ProductRelationship` between products
- No `ProductCharacteristic` dynamic fields
- No `ProductPrice` on product instances
- No `ProductTerm` / agreement terms on product
- No `BillingAccountRef` linked to products
- No `Place` on product instances
- No `AgreementRef` on subscriptions/products
- No `Product` specification ref directly — subscription references Offer/Product
- No `Product` lifecycle events (created, modified, deleted) — subscription events exist but don't match TMF637 event pattern
- No `Product` status matching TMF (the subscription status is different)
- Entitlements are business-specific, not TMF concept
- No `RealizingService` / `RealizingResource` on product (links to ServiceInventory)
- No structure for the product-to-service relationship

### 6. ServiceCatalog → TMF633 (Service Catalog) — **85%**

**What maps:**
- `ServiceSpecification` with characteristics, values, relationships
- `ServiceCategory` with hierarchy, candidates
- `ServiceCandidate` linking specs to categories
- Lifecycle status: Draft → Active → Retired
- CRUD for all resources
- Pagination with TMF format

**What's missing:**
- No `ResourceSpecification` ref on service spec
- No `ServiceSpecCharacteristic` `@valueType` mapping (uses flat string for ValueType)
- No `ServiceCandidate` `baseCandidate` relationship properly mapped (field exists but not populated in handlers)
- No `FeatureSpecification` structure on candidates (stored as jsonb blob, not structured)
- No `ServiceSpecRelationship` `@type` for polymorphism
- No `@baseType` / `@schemaLocation` on DTOs
- No `ServiceCategory` `ProductOffering` refs
- No `ServiceCategory` `Version` tracking
- No integration events for catalog changes
- No `ServiceSpecification` `Version` on lastUpdate mechanism

### 7. ServiceInventory → TMF638 (Service Inventory) — **80%**

**What maps:**
- `Service` with status lifecycle (Pending → Provisioning → Active → Suspended → Decommissioned)
- TMF fields: `href`, `atType`, `atBaseType`, `atSchemaLocation`, `name`, `description`, `externalId`, `place`, `category`, `startDate`, `completionDate`, `relatedParty`
- `ServiceResource` allocated to services
- `ServiceTopology` with upstream/downstream links
- Service characteristics stored in configuration (jsonb)
- Domain events for state changes
- Discovery jobs for resource matching

**What's missing:**
- No `ServiceCharacteristic` dynamic fields (stored as jsonb blob in Configuration)
- No `ServiceRelationship` standardized structure (topology is separate)
- No `SupportingService` on service (only supporting resources)
- No `ServicePrice` on services
- No `ServiceTerm` / agreement terms
- No `Service` `Feature` structure
- No `Service` `Place` structured (stored as flat Location string)
- No `Service` `RelatedEntity` polymorphism
- No integration events for service state changes
- No `Service` `Category` ref type enforcement
- No `Service` versioning

### 8. Provisioning → TMF641 (Service Ordering) — **30%**

**What maps:**
- Provisioning jobs with tasks and templates
- Job lifecycle: Pending → Queued → InProgress → Completed / Failed → RolledBack
- Integration with Orders (listens for ProvisioningRequired)
- Integration with ServiceInventory (activates services on completion)

**What's missing (major):**
- TMF641 is about `ServiceOrder`, not `ProvisioningJob` — the concepts are related but don't align
- No `ServiceOrder` resource with service order items
- No `ServiceOrder` state machine matching TMF (acknowledged, inProgress, held, completed, cancelled, etc.)
- No `CancelServiceOrder` task resource
- No `ServiceOrder` `RelatedParty`
- No `ServiceOrder` `RequestedCompletionDate` / `RequestedStartDate`
- No `ServiceOrder` milestone tracking
- No `ServiceOrder` information required notifications
- No `ServiceOrder` `OrderItem` relationship to Service
- No `ServiceOrder` note/notification
- No integration events from Provisioning back to Orders (provisioning events exist but only internal)
- No `ServiceOrder` characteristic
- No `ServiceOrder` agreement ref

### 9. Billing → TMF666 (Account Management) — **55%**

**What maps:**
- Bill generation with billing cycles
- Tax rules and exemptions
- Billing jobs tracking
- Calculate tax on bills

**What's missing:**
- No `BillingAccount` resource as defined by TMF666 (billing account with status, type, characteristics, etc.)
- No `SettlementAccount` / `SettlementAccountRelationship`
- No `AccountBalance` / `Balance` structure
- No `AccountTaxExemption`
- No `BillingAccount` `RelatedParty`
- No `BillingAccount` `FinancialAccount` ref
- No `BillPresentationMedia`
- No `AccountRelationship` between billing/settlement/financial accounts
- No `Account` characteristic
- No `Account` lifecycle events
- No `PaymentPlan` on accounts
- No `AccountRef` integration with CRM for proper billing account management
- Billing cycles are basic — no calendar-based cycle support
- No collection thresholds / dunning configuration in Billing

### 10. Invoices → TMF678 (Customer Bill) — **70%**

**What maps:**
- Invoice creation from bills
- Invoice lifecycle (draft → finalized → sent → paid → cancelled)
- PDF generation
- Credit notes
- Invoice disputes with resolve/reject flow
- Invoice summaries
- Overdue detection

**What's missing:**
- TMF678 is about `CustomerBill` — invoices map well but naming differs
- No `AppliedCustomerBillingTaxRate` on invoice lines
- No `FinancialAccount` ref
- No `BillingAccount` ref structure (uses flat customer reference)
- No `BillItem` structure (invoices have line items but no standardized TMF BillItem)
- No `BillFormat` / `BillPresentationMedia`
- No `BillStructure` for hierarchical billing
- No `Payment` refs on invoices (payments handled by Payments module separately)
- No `Invoice` characteristic
- No `@baseType` / `@schemaLocation` on DTOs
- No integration events for invoice lifecycle

### 11. Payments → TMF676 (Payment Management) — **75%**

**What maps:**
- Payment recording and processing
- Refunds
- Payment gateways
- Payment reconciliation
- Payment methods

**What's missing:**
- No `PaymentMethod` management resource (gateways exist but not standardized PaymentMethod CRUD)
- No `Refund` as standalone resource (refunds exist as operation on payment, not as separate resource)
- No `PaymentTransaction` structured tracking
- No `PaymentCard` / `BankAccount` detailed types
- No `BillingAccount` ref on payments
- No `PartyAccount` ref
- No `Payment` `Characteristic`
- No `Payment` `RelatedParty`
- No integration events for payment lifecycle
- No `PaymentPlan` / installment support
- No external payment gateway integration via standardized interface

### 12. Collections → — **Not TMF-mapped**

Collections module handles collection cases, payment arrangements, aging reports, and fee waiving. This is business logic not directly defined by a specific TMF API. It relates to account management (TMF666) and payment management (TMF676) subsidiary flows.

### 13. Rating → TMF677 (Usage Consumption) — **60%**

**What maps:**
- Usage submission and real-time rating
- Rating rules
- Promotions and discount application
- Usage consumption by subscription
- Unrated record management

**What's missing:**
- No `UsageConsumptionReport` resource
- No `UsageSpecification` for defining what/how usage is measured
- No `MonetaryAmount` standardization on usage records
- No `UsageCharacteristic` on usage records
- No `Product` reference on usage (subscription ref only)
- No `RelatedParty` on usage records
- No `Bucket` management for bundled usage
- No usage balance/remaining tracking
- No threshold/alarm on usage consumption
- No integration events for usage milestone reached

### 14. NetworkInventory → TMF639 (Resource Inventory) — **50%**

**What maps:**
- Network elements (like TMF `Resource`)
- Network topology with links
- Subnets, VLANs, OLTs/ONTs
- Capacity tracking and alerts
- IP address management

**What's missing:**
- No `Resource` entity matching TMF639 standard (has `NetworkElement` which is a Resource sub-type but not structured as TMF Resource)
- No `ResourceRelationship` standardized
- No `ResourceCharacteristic` dynamic fields
- No `ResourceSpecification` ref on network elements
- No `ResourceStatus` lifecycle matching TMF (available, reserved, allocated, etc.)
- No `Resource` `Category` / `Place` / `RelatedParty`
- No `Resource` `Attachment`
- No `Resource` `Feature`
- No `ResourcePrice` on elements
- No `AdministrativeState` / `OperationalState` / `UsageState` on all resources
- No integration events for resource lifecycle
- No `Resource` versioning

### 15. NumberInventory → TMF639 (Resource Inventory) — **40%**

**What maps:**
- `TelephoneNumber` as a specialized Resource
- Number lifecycle: Available → Reserved → Assigned/Ported → Suspended → Disconnected
- Number types (Geographic, Mobile, TollFree, etc.)
- CRUD for numbers

**What's missing:**
- TelephoneNumber does NOT map to TMF639 `Resource` structure — it's a domain-specific entity
- No `Resource` base type/interface
- No `Resource` standard fields (href, @type, @baseType, etc.)
- No `ResourceSpecification` ref
- No `ResourceCharacteristic`
- No `ResourceRelationship`
- No `RelatedParty` on numbers
- No `AdministrativeState` / `OperationalState` / `UsageState`
- No `Place` / `Category` on numbers
- No `Attachments`
- NumberInventory operates as standalone — not linked to TMF639 Resource lifecycle
- No integration events for number allocation

### 16. Workflow → TMF701 (Process Flow) — **35%**

**What maps:**
- Workflow definitions with steps
- Workflow instances
- Task execution
- SLA tracking per workflow
- Dashboard and metrics

**What's missing:**
- No TMF701 `Process` matching
- No `ProcessFlowSpecification` with required/optional steps, parallel execution
- No `ProcessFlowInstance` state machine matching TMF
- No `FlowGraph` / `FlowEdge` structure
- Step execution is sequential, no parallel branches
- No `ProcessFlow` characteristic
- No `RelatedParty` on workflows
- No `Process` timing/milestone tracking
- No TMF-standard events for process state changes
- No `ProcessFlow` `Version`
- No `InputParameter` / `OutputParameter` structured on steps

### 17. Ticketing → TMF702 (Task Management) — **40%**

**What maps:**
- Ticket creation, assignment, escalation
- Comments on tickets
- SLA tracking on tickets
- SLA breach detection
- Status lifecycle

**What's missing:**
- No `Task` resource matching TMF702 specification
- No `TaskCharacteristic` dynamic fields
- No `RelatedParty` on tickets
- No `TaskRelationship` between tickets (parent/child/subtask)
- No `TaskCategory` / `TaskType` hierarchies
- No `Note` / `Attachment` standardization on tasks
- No `Project` ref on tickets
- No `TaskOrder` / priority matrix
- No integration events for task lifecycle
- No `TaskName` / `TaskDescription` matching TMF
- No `@baseType` / `@schemaLocation`
- SLA is basic — no calendar-based SLA, no escalation matrix

### 18. Notifications → TMF681 (Communication Management) — **50%**

**What maps:**
- Notification creation and sending
- Multiple channels (email, SMS, push, in-app)
- Notification preferences per user
- Read/unread tracking
- Templates
- Bulk read operations

**What's missing:**
- No `CommunicationMessage` resource matching TMF681
- No `Communication` request/response pattern
- No `CommunicationCharacteristic`
- No `CommunicationCategory` / `CommunicationType`
- No `RelatedParty` on communications
- No `Attachment` on communications
- No `CommunicationFlow` for multi-step communications
- No delivery status tracking / retry logic
- No `Sender` / `Receiver` structured entities
- No `CommunicationTemplate` management matching TMF
- No @baseType / @schemaLocation
- No integration events

### 19. EventManagement → TMF688 (Event Management) — **45%**

**What maps:**
- Event subscriptions (webhook subscriptions)
- Webhook event dispatching
- Event querying

**What's missing:**
- No TMF688 `EventSubscription` matching (subscription with `query`/`filter` - EventManagement has basic filters)
- No `Event` hub registration / notification listener pattern
- No standard event payload format matching TMF hub pattern
- No `EventSubscription` `sink` URL validation
- No `EventSubscription` status (active/suspended)
- No `EventSubscription` eventType discovery
- No retry strategy / exponential backoff
- No event delivery guarantees (at-least-once, exactly-once)
- No `Event` filtering by resource/id
- No `Event` hub events emitted for subscription creation/deletion
- No integration with the system's own domain events (EventManagement manages external webhooks, not internal events)

### 20. Audit / Reporting / ApiGateway

These modules are supporting/infrastructure and don't directly map to TMF API specifications.

- **Audit**: TMF-compatible audit fields but no specific TMF API
- **Reporting**: Internal reporting, no TMF equivalent
- **ApiGateway**: Infrastructure gateway management, no TMF equivalent
