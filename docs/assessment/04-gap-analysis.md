# Gap Analysis

## Per-Module Gaps

### 1. IAM (TMF632 + TMF669)

**TMF Gaps:**
- No `Individual` full resource (no address, contact, characteristics, identifications)
- No Organization resource (only flat Tenant)
- No party merge/split operations
- No tax exemption certificate
- No ExternalIdentifier standard structure
- PartyRole UpdateHandler doesn't apply field updates (bug)
- PartyRole table may not be in migration snapshot

**Contract Drift:**
- PartyRole endpoints not in generated API client (missing from OpenAPI spec)
- Password sent by frontend but not stored (expected — Keycloak handles auth)
- No ChangePassword endpoint anywhere

**Missing Features:**
- No password management
- No user self-registration
- No user profile management UI
- No permission management UI
- No API key management for users
- No user session management

### 2. CRM (TMF629)

**TMF Gaps:**
- No CustomerAccount resource
- No AppliedCustomerBillingTaxRate
- No full payment method management
- No shopping cart
- No loyalty/bonus points
- No ExternalIdentifier structure
- @baseType/@schemaLocation not on all DTOs
- No customer integration events published

**Contract Drift:**
- Address field nesting between frontend and backend
- Segment batch assignment missing in backend (one-at-a-time only)
- No PATCH endpoint (partial updates via custom endpoint)

**Missing Features:**
- No customer merge
- No customer bulk import
- No customer activity timeline
- No customer communication history
- No customer consent management

### 3. ProductCatalog (TMF620)

**TMF Gaps:**
- No ProductOfferingPrice-priceType split per TMF model
- No agreement refs on product offerings
- No MarketSegment/Place associations
- No Constraint resource
- No Attachment support
- No ResourceSpecification ref on product spec
- No @baseType/@schemaLocation on all entities
- No ExternalIdentifier structure
- No RelatedParty on Catalog/Category
- No integration events for catalog changes

**Contract Drift:**
- Pricing update route pattern differs between frontend and backend expectations
- Characteristic value complex nested updates may not be fully aligned

**Missing Features:**
- No product specification version history
- No offer comparison tool
- No bundle pricing scenarios
- No catalog import/export

### 4. Orders (TMF622)

**TMF Gaps:**
- No CancelOrder task resource
- No OrderInformationRequired event
- No milestone tracking
- No agreement refs
- No BillingAccount ref
- No Note entities (flat text field)
- No Payment refs linked to actual Payments module
- No Version tracking
- No Channel as structured enum

**Contract Drift:**
- Fulfillment complete route in OpenAPI may differ from frontend expectation
- Error response format not standardized to TMF ProblemDetails everywhere

**Missing Features:**
- No order amendment flow
- No order split/merge
- No order SLA tracking
- No order approval routing
- No order hold/unhold

### 5. Subscriptions (TMF637)

**Major TMF Gaps:**
- Module is Subscription Management, NOT Product Inventory
- No Product resource as defined by TMF637
- No ProductRelationship
- No ProductCharacteristic (dynamic)
- No ProductPrice on instances
- No ProductTerm
- No BillingAccountRef
- No Place
- No AgreementRef
- No RealizingService/RealizingResource
- No Product lifecycle events matching TMF
- No provisioned product-to-service mapping

**Contract Drift:**
- Entitlement override command DTO field naming may differ
- Frontend enum for billing periods may mismatch backend

**Missing Features:**
- No product-to-service mapping UI
- No subscription history timeline
- No subscription preview/pricing calculator
- No subscription hold/resume scheduling

### 6. ServiceCatalog (TMF633)

**TMF Gaps:**
- No ResourceSpecification ref on service spec
- @valueType on characteristics not structured
- BaseCandidate not populated in handlers
- FeatureSpecification stored as jsonb blob
- @type for polymorphism not used
- No @baseType/@schemaLocation
- No ProductOffering refs on categories
- No version tracking
- No integration events

**Contract Drift:**
- M2M candidate-category assignment may not work correctly from frontend
- BaseCandidate field not exposed

**Missing Features:**
- No service specification versioning
- No service spec comparison
- No service catalog dependency graph UI

### 7. ServiceInventory (TMF638)

**TMF Gaps:**
- ServiceCharacteristic as jsonb blob, not structured
- ServiceRelationship not standardized (topology is separate)
- No SupportingService
- No ServicePrice
- No ServiceTerm
- No Feature structure
- Place as flat string
- No RelatedEntity polymorphism
- No integration events
- No Category ref type enforcement

**Contract Drift:**
- ServiceType enum frontend may not list all 18 values
- Configuration jsonb format not validated on frontend

**Missing Features:**
- No service dependency visualization
- No service health monitoring
- No service impact analysis

### 8. Provisioning

**TMF Gaps (TMF641):**
- No ServiceOrder resource at all
- No ServiceOrder lifecycle matching TMF
- No CancelServiceOrder
- No RelatedParty
- No requested dates
- No milestone tracking
- No information required notifications
- No order item-to-service relationship
- No notes/notifications
- No characteristics
- No agreement refs
- No integration events back to Orders (provisioning events are internal)

**Contract Drift:**
- Job logs query uses raw SQL — not in generated client properly
- Template response shape may differ from frontend expectation

**Missing Features:**
- No provisioning progress tracking UI
- No provisioning error details
- No manual provisioning task handling
- No provisioning rollback visualization

### 9. Billing (TMF666)

**TMF Gaps:**
- No BillingAccount resource
- No SettlementAccount
- No AccountBalance structure
- No AccountTaxExemption
- No RelatedParty on accounts
- No FinancialAccount ref
- No BillPresentationMedia
- No AccountRelationship
- No Account characteristics
- No Account lifecycle events
- No PaymentPlan

**Contract Drift:**
- Tax rules/exemptions DTOs not in generated client
- Job endpoints response shape may differ from frontend

**Missing Features:**
- No billing account management
- No billing account hierarchy
- No billing statement PDF viewer
- No billing cycle configuration UI
- No billing calendar visualization

### 10. Invoices (TMF678)

**TMF Gaps:**
- Resource named Invoice vs TMF CustomerBill
- No AppliedCustomerBillingTaxRate on lines
- No FinancialAccount ref
- No BillingAccount ref structure
- No BillItem structure
- No BillFormat/BillPresentationMedia
- No BillStructure for hierarchical billing
- No Payment refs
- No characteristic
- No @baseType/@schemaLocation

**Contract Drift:**
- Credit note DTO not fully generated
- Dispute resolve/reject DTO naming misalignment

**Missing Features:**
- No invoice PDF preview in browser
- No invoice email sending (endpoint exists but no UI trigger)
- No invoice payment allocation UI
- No recurring invoice templates

### 11. Payments (TMF676)

**TMF Gaps:**
- No PaymentMethod management resource
- No Refund as standalone resource
- No PaymentTransaction structured tracking
- No PaymentCard/BankAccount types
- No BillingAccount ref
- No PartyAccount ref
- No Payment Characteristic
- No Payment RelatedParty
- No integration events
- No PaymentPlan

**Contract Drift:**
- Reconciliation import endpoint missing from generated client
- PaymentMethod not standardized as DTO
- Gateway configuration not in frontend UI

**Missing Features:**
- No payment method management UI
- No payment history with filtering
- No payment receipt generation
- No auto payment configuration
- No payment gateway dashboard

### 12. Collections

**TMF Gaps:**
No direct TMF mapping. Related to TMF666/TMF676 subsidiary flows.

**Contract Drift:**
- Dunning policy referenced by frontend but may not exist in backend
- Case action DTO names may differ

**Missing Features:**
- No collection workflow automation
- No collection metrics/dashboard
- No dunning letter generation
- No third-party collection agency integration UI

### 13. Rating (TMF677)

**TMF Gaps:**
- No UsageConsumptionReport resource
- No UsageSpecification
- No MonetaryAmount standardization
- No UsageCharacteristic
- No Product reference (subscription ref only)
- No RelatedParty
- No Bucket management
- No usage balance tracking
- No threshold/alarm
- No integration events

**Contract Drift:**
- Promotion deactivation DTO datetime format differs
- ApplicablePromotions filter params not aligned
- Real-time rate response shape may not match frontend expectation

**Missing Features:**
- No usage dashboard/monitoring
- No usage threshold configuration
- No rating rule simulation
- No promotion effectiveness dashboard

### 14. NetworkInventory (TMF639)

**TMF Gaps:**
- No Resource entity matching TMF639
- No ResourceRelationship standardized
- No ResourceCharacteristic
- No ResourceSpecification ref
- No ResourceStatus lifecycle
- No Category/Place/RelatedParty
- No Attachment
- No Feature
- No ResourcePrice
- No AdministrativeState/OperationalState/UsageState
- No integration events
- No versioning

**Contract Drift:**
- OLT/ONT endpoints missing from generated client
- Interface/config endpoints missing from generated client
- Capacity alert DTOs may not match between frontend/backend
- Element type enums may not be aligned

**Missing Features:**
- No network topology visualization
- No network element configuration UI
- No capacity planning UI
- No network alarm integration
- No network inventory import/export

### 15. NumberInventory (TMF639)

**TMF Gaps:**
- TelephoneNumber doesn't map to TMF639 Resource structure
- No Resource base type
- No Resource standard fields
- No ResourceSpecification ref
- No ResourceCharacteristic
- No ResourceRelationship
- No RelatedParty
- No AdministrativeState/OperationalState/UsageState
- No Place/Category
- No Attachments
- No integration events

**Contract Drift:**
- Import number range endpoint may not be in generated client
- Port operations (port-in/port-out) may not be exposed in frontend

**Missing Features:**
- No number range management UI
- No number reservation workflow
- No port-in/port-out tracking UI
- No number pool visualization

### 16. Workflow (TMF701)

**TMF Gaps:**
- No ProcessFlowSpecification with required/optional steps
- No parallel execution
- No FlowGraph/FlowEdge structure
- No Process characteristics
- No RelatedParty
- No timing/milestone tracking
- No TMF-standard events
- No Version
- No InputParameter/OutputParameter

**Contract Drift:**
- SLA DTO field names may not match frontend expectations
- Dashboard metrics response shape complex — frontend may have parsing issues
- Step ordering/management DTOs may not align

**Missing Features:**
- No workflow designer/visual editor (UI is basic CRUD)
- No workflow simulation
- No workflow versioning
- No workflow SLA calendar support

### 17. Ticketing (TMF702)

**TMF Gaps:**
- No Task resource matching TMF702
- No TaskCharacteristic
- No RelatedParty
- No TaskRelationship (parent/child)
- No Category/Type hierarchy
- No Note/Attachment standardization
- No Project ref
- No priority matrix
- No integration events
- No @baseType/@schemaLocation
- No calendar-based SLA

**Contract Drift:**
- SLA definition DTOs may not fully align
- Escalation matrix not exposed in frontend

**Missing Features:**
- No ticket template/quick-create
- No ticket dashboard with KPIs
- No ticket auto-assignment rules
- No ticket SLA breach notifications
- No customer portal for ticket submission

### 18. Notifications (TMF681)

**TMF Gaps:**
- No CommunicationMessage resource
- No Communication request/response pattern
- No CommunicationCharacteristic
- No Category/Type
- No RelatedParty
- No Attachment
- No CommunicationFlow
- No delivery status tracking
- No Sender/Receiver structured
- No template management matching TMF

**Contract Drift:**
- Template management endpoints fully aligned — minor OK

**Missing Features:**
- No notification preferences UI per notification type
- No notification history search
- No push notification device registration
- No notification delivery analytics
- No bulk notification campaign UI

### 19. EventManagement (TMF688)

**TMF Gaps:**
- No EventSubscription with query/filter matching TMF
- No event hub registration pattern
- No standard event payload format
- No sink URL validation
- No subscription status management
- No event type discovery
- No retry strategy
- No delivery guarantees
- No event filtering by resource/id
- No integration with internal domain events

**Contract Drift:**
- No frontend pages exist for event management
- Limited API hooks in frontend
- 60% alignment is generous — severe drift

**Missing Features:**
- No event subscription management UI
- No event dashboard/monitoring
- No event replay capability
- No event delivery audit log

### 20. Audit (Supporting)

**Contract Drift:**
- Compliance/alert/policy endpoints not in generated client
- Alert rule endpoints may not match frontend expectations

**Missing Features:**
No significant feature gaps for a supporting module.

### 21. Reporting (Supporting)

**Contract Drift:**
- Widget DTO field names may not align
- Scheduled report response shape may differ

**Missing Features:**
- No report builder UI (drag-drop)
- No report scheduling calendar
- No report delivery configuration
- No dashboard custom layout

### 22. ApiGateway (Infrastructure)

**Contract Drift:**
- Minor alignment issues with partner endpoint DTOs

**Missing Features:**
- No API route testing UI
- No API key usage analytics
- No partner SLA monitoring
- No rate limiting configuration UI
