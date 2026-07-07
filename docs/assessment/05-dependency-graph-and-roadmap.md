# Dependency Graph & Implementation Roadmap

## Module Dependency Graph

```
Level 0 (Foundation)
├── IAM (Keycloak auth, users, tenants, party roles)
├── Audit (logging infrastructure)
├── EventManagement (webhook infrastructure)
└── ApiGateway (API routing, keys, partners)

Level 1 (Core Catalog)
├── ProductCatalog (products, offers, specs, categories)
└── ServiceCatalog (service specs, categories, candidates)

Level 2 (Customer Foundation)
├── CRM (customers, individuals, orgs, agreements, segments)
└── NumberInventory (telephone numbers)

Level 3 (Ordering)
├── Orders (orders, fulfillment)
├── ServiceInventory (services, topology)
└── NetworkInventory (network elements, topology)

Level 4 (Fulfillment)
├── Subscriptions (subscriptions, entitlements)
├── Provisioning (provisioning jobs, tasks)
└── Workflow (workflow definitions, instances)

Level 5 (Usage & Rating)
└── Rating (usage rating, promotions)

Level 6 (Financial)
├── Billing (bill generation, tax rules)
├── Invoices (invoices, credit notes, disputes)
├── Payments (payments, refunds, reconciliation)
└── Collections (collection cases, arrangements)

Level 7 (Cross-Cutting)
├── Notifications (email, SMS, push, in-app)
├── Ticketing (tickets, SLAs)
├── Reporting (reports, dashboards)
└── Notifications (depends on every module for events)
```

## Combined Risk Assessment

Each module scored on two axes:
- **TMF Gap Severity**: High (0-40%), Medium (41-70%), Low (71-100%)
- **Contract Drift Severity**: Critical (<60%), High (60-75%), Medium (76-85%), Low (>85%)

| Module | TMF Score | TMF Severity | Contract Score | Drift Severity | Combined Priority |
|--------|-----------|-------------|----------------|----------------|-------------------|
| Subscriptions | 50% | High | 80% | Medium | **P1** |
| Provisioning | 30% | High | 70% | High | **P1** |
| NetworkInventory | 50% | High | 70% | High | **P1** |
| NumberInventory | 40% | High | 85% | Low | P2 |
| IAM | 40% | High | 90% | Low | P2 |
| Workflow | 35% | High | 70% | High | **P1** |
| Ticketing | 40% | High | 85% | Low | P2 |
| EventManagement | 45% | High | 60% | Critical | **P1** |
| Notifications | 50% | High | 90% | Low | P2 |
| Billing | 55% | Medium | 75% | Medium | P2 |
| CRM | 75% | Low | 85% | Low | P3 |
| ProductCatalog | 90% | Low | 85% | Low | P3 |
| Orders | 85% | Low | 90% | Low | P3 |
| ServiceCatalog | 85% | Low | 75% | Medium | P3 |
| ServiceInventory | 80% | Low | 80% | Medium | P3 |
| Payments | 75% | Low | 80% | Medium | P3 |
| Invoices | 70% | Medium | 85% | Low | P3 |
| Collections | — | Medium | 75% | Medium | P3 |
| Rating | 60% | Medium | 80% | Medium | P3 |
| Reporting | — | Low | 75% | Medium | P4 |
| Audit | — | Low | 85% | Low | P4 |
| ApiGateway | — | Low | 85% | Low | P4 |

## Implementation Roadmap

### Priority 1: Fix Critical Gaps (Must fix — actively producing runtime bugs or blocking flows)

These modules have either severe TMF gaps or critical contract drift that breaks end-to-end flows:

**1a. Provisioning (TMF641 alignment + Contract Fix)**
- Effort: 3-4 weeks
- Risk: High (core fulfillment path)
- Dependencies: Orders, Workflow, ServiceInventory
- Actions:
  - Implement ServiceOrder resource matching TMF641
  - Add ServiceOrder lifecycle (acknowledged → inProgress → completed)
  - Add CancelServiceOrder task resource
  - Fix integration events to publish back to Orders
  - Add RelatedParty, requested dates, milestones
  - Align job logs endpoint with generated client
  - Fix template response DTOs
- Expected TMF compliance after: 70%
- Expected Contract Alignment: 90%

**1b. Subscriptions → Product Inventory refactor (TMF637)**
- Effort: 6-8 weeks
- Risk: Very high (re-architecture)
- Dependencies: ProductCatalog, Orders, ServiceInventory, Rating, Billing
- Actions:
  - Add Product resource matching TMF637
  - Add ProductRelationship, ProductCharacteristic, ProductPrice
  - Make Subscription reference Product (not Offer directly)
  - Add RealizingService/RealizingResource links
  - Add Place, AgreementRef, BillingAccountRef on Product
  - Add product lifecycle events
  - Add provisioned product-to-service mapping UI
  - Align entitlement command DTOs
- Expected TMF compliance after: 80%
- Expected Contract Alignment: 90%

**1c. NetworkInventory → TMF639 Resource alignment**
- Effort: 4-5 weeks
- Risk: Medium
- Dependencies: None (standalone)
- Actions:
  - Add Resource base entity matching TMF639
  - Align NetworkElement as Resource sub-type
  - Add ResourceRelationship, ResourceCharacteristic
  - Add AdministrativeState/OperationalState/UsageState
  - Add ResourceSpecification ref
  - Add Place/Category/RelatedParty on resources
  - Fix generated client for OLT, interface, capacity endpoints
  - Align element type enums
  - Add integration events for resource lifecycle
- Expected TMF compliance after: 80%
- Expected Contract Alignment: 90%

**1d. EventManagement (TMF688 alignment)**
- Effort: 2-3 weeks
- Risk: Medium
- Dependencies: None (standalone)
- Actions:
  - Add EventSubscription with query/filter matching TMF688
  - Add event hub registration pattern
  - Implement standard event payload format
  - Add sink URL validation
  - Add subscription status management
  - Implement retry with exponential backoff
  - Connect to internal domain events (bridge to outbox)
  - Add frontend event management UI
  - Add subscription CRUD hooks + pages
- Expected TMF compliance after: 80%
- Expected Contract Alignment: 90%

**1e. Workflow (TMF701 Process Flow + Contract Fix)**
- Effort: 3-4 weeks
- Risk: Medium
- Dependencies: None (standalone)
- Actions:
  - Add ProcessFlowSpecification with required/optional steps
  - Support parallel execution branches
  - Add FlowGraph/FlowEdge structure
  - Add InputParameter/OutputParameter on steps
  - Align SLA DTOs with frontend
  - Fix workflow metrics/dashboard response shape
  - Add @baseType/@schemaLocation
- Expected TMF compliance after: 75%
- Expected Contract Alignment: 90%

### Priority 2: Close Major TMF Gaps

**2a. IAM (TMF632 + TMF669)**
- Effort: 3-4 weeks
- Risk: Medium
- Dependencies: None
- Actions:
  - Add full Individual resource (address, contact mediums, identifications)
  - Add Organization resource (legal details, registration)
  - Add ExternalIdentifier structure
  - Add party merge/split operations
  - Fix PartyRole UpdateHandler field application bug
  - Add PartyRole to migration snapshot
  - Add PartyRole endpoints to generated client
  - Add user profile management UI
  - Add permission management UI

**2b. NumberInventory (TMF639 alignment)**
- Effort: 2-3 weeks
- Risk: Low
- Dependencies: NetworkInventory (shared Resource base)
- Actions:
  - Align TelephoneNumber as TMF639 Resource sub-type
  - Add Resource base fields (href, @type, etc.)
  - Add ResourceSpecification ref
  - Add AdministrativeState/OperationalState/UsageState
  - Add integration events for number lifecycle
  - Add port operations to frontend UI

**2c. Ticketing (TMF702 alignment)**
- Effort: 2-3 weeks
- Risk: Low
- Dependencies: IAM
- Actions:
  - Add TaskCharacteristic dynamic fields
  - Add RelatedParty on tickets
  - Add TaskRelationship (parent/child)
  - Add Note/Attachment standardization
  - Add integration events
  - Add @baseType/@schemaLocation
  - Add calendar-based SLA calculation

**2d. Notifications (TMF681 alignment)**
- Effort: 2-3 weeks
- Risk: Low
- Dependencies: None
- Actions:
  - Add CommunicationMessage resource
  - Add delivery status tracking with retry
  - Add Sender/Receiver structured entities
  - Add notification delivery analytics
  - Add notification preferences per type
  - Add push notification device registration

### Priority 3: Close Medium Gaps & Improve Contract Alignment

**3a. Billing (TMF666 alignment)**
- Effort: 4-5 weeks (depends on Payment + Invoice alignment)
- Risk: Medium
- Dependencies: CRM (for account refs)
- Actions:
  - Add BillingAccount resource with lifecycle
  - Add AccountBalance structure
  - Add AccountTaxExemption
  - Add RelatedParty on accounts
  - Add BillPresentationMedia
  - Add integration events
  - Align tax rule/exemption DTOs in generated client

**3b. Rating (TMF677 alignment)**
- Effort: 2-3 weeks
- Risk: Low
- Dependencies: ProductCatalog, Subscriptions
- Actions:
  - Add UsageSpecification
  - Add UsageCharacteristic on records
  - Add Product reference
  - Add bucket/balance management
  - Add usage threshold/alarm
  - Add integration events for usage milestones

**3c. Collections (process alignment)**
- Effort: 1-2 weeks
- Risk: Low
- Dependencies: Billing, Invoices, Payments
- Actions:
  - Align dunning policy frontend/backend
  - Fix case action DTO naming
  - Add collection workflow automation

**3d. ServiceCatalog + ServiceInventory (minor TMF + contract fixes)**
- Effort: 2-3 weeks each
- Risk: Low
- Dependencies: None
- Actions:
  - Add @baseType/@schemaLocation
  - Add integration events
  - Align ServiceType enum frontend/backend (18 values)
  - Add ResourceSpecification ref to service specs

### Priority 4: Polish & Supporting Modules

**4a. ProductCatalog (minor fixes)**
- **ProductOfferingPrice model alignment** (add priceType split)
- Add integration events
- Add RelatedParty on catalog/category

**4b. CRM (minor fixes)**
- Add PATCH endpoint
- Add customer integration events
- Add @baseType/@schemaLocation

**4c. Orders (minor fixes)**
- Add CancelOrder task resource
- Add milestone tracking
- Add version tracking

**4d. Audit, Reporting, ApiGateway**
- Fix generated client gaps
- Add missing endpoint DTOs
- Align widget/schedule DTOs

## Combined Roadmap Summary

```
Month 1: P1 (Critical fixes)
├── Weeks 1-2: Provisioning TMF641 alignment
├── Weeks 2-4: EventManagement TMF688 + frontend
├── Weeks 3-5: Workflow TMF701 + contract fixes
└── Weeks 4-8: NetworkInventory TMF639 alignment

Month 2: P1 + P2 start
├── Weeks 5-7: NumberInventory TMF639 alignment
├── Weeks 6-8: IAM TMF632 + TMF669 + PartyRole fixes
├── Weeks 7-9: Ticketing TMF702 alignment
└── Weeks 8-10: Notifications TMF681

Month 3: P2 finish + P3 start
├── Weeks 9-11: Billing TMF666 alignment
├── Weeks 10-12: Rating TMF677 alignment
├── Weeks 11-13: ServiceCatalog + ServiceInventory fixes
└── Weeks 12-14: Collections alignment

Month 4-5: P3 finish + P4 + Subscriptions refactor
├── Weeks 13-16: ProductCatalog + CRM + Orders minor fixes
├── Weeks 16-22: Subscriptions → Product Inventory refactor (P1 holdover)
└── Weeks 20-24: Audit, Reporting, ApiGateway polish + All generated client refresh
```

**Total estimated effort: 20-24 weeks (5-6 months)** with a team of 2-3 backend + 1-2 frontend developers.
