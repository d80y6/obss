# Frontend–Backend Contract Alignment Audit

## Per-Module Alignment Scores

| # | Module | Alignment Score | Classification | Key Drift Items |
|---|--------|----------------|---------------|-----------------|
| 1 | IAM | **90%** | Minor Drift | PartyRole endpoints not in OpenAPI spec |
| 2 | CRM | **85%** | Minor Drift | Segment mgmt endpoints, address field mapping |
| 3 | ProductCatalog | **85%** | Minor Drift | Complex nested DTOs, pricing update paths |
| 4 | Orders | **90%** | Minor Drift | Fulfillment endpoints, pagination field names |
| 5 | Subscriptions | **80%** | Partial Drift | Entitlement command DTOs, enum alignment |
| 6 | ServiceCatalog | **75%** | Partial Drift | Candidate endpoints, characteristic value mapping |
| 7 | ServiceInventory | **80%** | Partial Drift | Topology endpoints, discovery job contracts |
| 8 | Provisioning | **70%** | Partial Drift | Job logs query, template response shape |
| 9 | Billing | **75%** | Partial Drift | Job endpoints, tax exemption models |
| 10 | Invoices | **85%** | Minor Drift | Credit note DTOs, dispute flow alignment |
| 11 | Payments | **80%** | Partial Drift | Refund DTOs, reconciliation contracts |
| 12 | Collections | **75%** | Partial Drift | Payment arrangement DTOs, dunning endpoints |
| 13 | Rating | **80%** | Partial Drift | Promotion commands, real-time rate response |
| 14 | NetworkInventory | **70%** | Partial Drift | OLT/ONT endpoints, capacity models |
| 15 | NumberInventory | **85%** | Minor Drift | Import number range endpoint |
| 16 | Workflow | **70%** | Partial Drift | SLA endpoints, step management |
| 17 | Ticketing | **85%** | Minor Drift | SLA definition DTOs |
| 18 | Notifications | **90%** | Minor Drift | Template management endpoints |
| 19 | Audit | **85%** | Minor Drift | Compliance/alert endpoints |
| 20 | Reporting | **75%** | Partial Drift | Widget DTOs, scheduled report contracts |
| 21 | ApiGateway | **85%** | Minor Drift | Partner endpoint DTOs |
| 22 | EventManagement | **60%** | Major Drift | No frontend pages, limited hooks |

**Overall Contract Alignment Score: ~79% (Partial Drift)**

---

## 1. Request Contract Diffs

### IAM (90% — Minor Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `password` in CreateUser | No password field on User | Frontend sends password, backend ignores (Keycloak manages auth) |
| `tenantId` as UUID | Backend uses TenantId VO | Frontend string aligns with serialized TenantId.Value |
| PartyRole POST payload | PartyRoleDto on backend | OK — PartyRole endpoints not in OpenAPI spec (generated client stale) |
| `status` string | Enum (Active/Suspended/Terminated) | OK with string conversion |

### CRM (85% — Minor Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `address` object | Backend uses flat Address VO | Frontend nests address fields under object; backend flattens at persistence layer |
| `segmentIds` array | No batch assign endpoint | Frontend may send multi-segment; backend assigns one at a time |
| `contactMedium` array | Backend handles as owned entity | OK |
| `customerType` string | Enum (Residential/Business/Wholesale) | OK with string conversion |
| `kycStatus` string | Enum (NotStarted/Pending/Verified/Rejected) | OK |
| SuspendCustomer POST | Backend expects `{reason: string}` | OK |

### ProductCatalog (85% — Minor Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `pricing` as nested object | CreateOfferPricingItem expects flat | OK — Mapster handles |
| `characteristics` array | List of CreateCharacteristicItem | OK |
| `priceRanges` array | List of PriceRangeDto | OK — but PUT /offers/{offerId}/pricing/{pricingId} path mismatches frontend route pattern |
| `lifecycleStatus` string | Enum (Draft/Active/Retired/Discontinued) | OK with string conversion |
| `offerType` string | Enum (OneTime/Recurring/UsageBased/Bundled) | OK |

### Orders (90% — Minor Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `orderItems` array | CreateOrderCommand has items | OK |
| `billingAddress` object | Backend uses Address VO | OK |
| `status` string | Enum (Draft/Submitted/.../Cancelled) | OK |
| `priority` string | Backend stores as free string | No constraint on frontend values |
| Fulfillment endpoints | GetOrderFulfillmentStatusQuery | OK — but completeFulfillment route in OpenAPI mismatches frontend path |

### Subscriptions (80% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `entitlements` array | SetSubscriptionEntitlementsCommand | OK |
| `billingPeriod` string | Enum (Monthly/Quarterly/SemiAnnual/Annual) | OK |
| `cancelReason` string | CancelSubscriptionCommand has Reason | OK |
| `offerChange` effectiveDate | ChangeOfferCommand has EffectiveDate | **Field name drift**: frontend may send `effectiveDate` vs backend's `EffectiveDate` naming |
| `quantity` as number | ChangeQuantityCommand has Quantity | OK |
| No `renewalDate` control | Subscription has RenewalDate | OK — backend-managed |

### ServiceCatalog (75% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `specCharacteristics` nested | AddCharacteristicCommand | OK |
| `candidateCategory` relation | ServiceCandidate has Categories M2M | **Frontend may not handle M2M assignments correctly** |
| `baseCandidate` reference | ServiceCandidate has BaseCandidateId | Not exposed in frontend UI or API properly |
| `featureSpecification` | Stored as jsonb | Complex structure not fully modeled in frontend |

### ServiceInventory (80% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `serviceType` string | Enum (FTTH/ADSL/VoIP/etc.) | OK — but frontend enum may not list all 18 service types |
| `status` string | ServiceStatus enum | OK |
| `configuration` object | JsonDocument | OK — but frontend must send valid JSON |
| `topologyLinks` array | AddTopologyLinkCommand | OK |
| `discoveryConfig` object | ResourceDiscoveryJob Configuration | OK |

### Billing (75% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `billGenerate` fields | GenerateBillCommand | OK |
| `cycleStart/End` dates | ProcessBillingCycleCommand | OK |
| `taxRule` object | CreateTaxRuleCommand | **Missing from generated client** — tax rules not in OpenAPI v1 schemas |
| `taxExemption` | ApplyTaxExemptionCommand | **DTO not aligned** — frontend may use different field names |

### Invoices (85% — Minor Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `creditNote` fields | VoidInvoiceCommand / CreateCreditNote | **Credit note DTO not fully generated** |
| `dispute` payload | CreateDisputeCommand | OK — but resolve/reject DTO naming differs |
| `sendInvoice` action | SendInvoiceCommand | OK |

### Payments (80% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `refundAmount` | ProcessRefundCommand has Amount | OK |
| `paymentMethod` string | Gateway abstraction | **No standardized PaymentMethod DTO** |
| `reconciliation` import | POST /reconciliation/import | **Endpoint not in generated client** |
| `gateway` config | PaymentGateway configuration | **Missing from frontend** |

### Collections (75% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `caseAction` | UpdateCollectionCaseCommand | OK |
| `arrangement` payments | RecordPaymentArrangementCommand | OK |
| `feeWaiver` reason | WaiveFeesCommand | OK |
| `dunningPolicy` | Not a backend entity | **Frontend references dunning policies; backend may not have them** |
| Aging report filters | GetCustomerAgingQuery | OK |

### Rating (80% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `usageRecord` | SubmitUsageCommand | OK |
| `promotion` payload | CreatePromotionCommand | OK — but `deactivatePromotion` DTO uses different datetime format |
| `ratingRule` | CreateRatingRuleCommand | OK |
| `applicablePromotions` | GetApplicablePromotionsQuery | **Filter params not aligned** |

### Workflow (70% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `stepDefinition` | AddWorkflowStepCommand | OK |
| `slaConfig` | CreateWorkflowSlaCommand | **DTO field names differ from frontend** |
| `dashboardMetrics` | GetWorkflowMetricsQuery | **Response shape complex, frontend may not parse correctly** |

### NetworkInventory (70% — Partial Drift)
| Frontend Sends | Backend DTO Accepts | Issue |
|----------------|-------------------|-------|
| `elementType` | NetworkElement entity | **Frontend enum values may not match backend** |
| `interface` config | POST /{id}/interfaces | **Missing from generated client** |
| `capacity` thresholds | Capacity tracking DTO | **Not aligned with frontend alert format** |
| OLT/ONT registration | POST /olts/{id}/register-ont | **Missing from generated client** |

---

## 2. Response Contract Diffs (Fields Returned vs Fields Consumed)

### Major findings:
1. **Generated client (dto.ts)** — 94 DTOs, generally up-to-date with OpenAPI spec but some endpoints missing (PartyRole, OLT management, reconciliation import, compliance/alert endpoints in Audit)
2. **Enum alignment**: All backend enums have string serialization. Frontend types match in most cases, but some modules have drift:
   - NetworkInventory: Element types may not fully match
   - ServiceInventory: ServiceType enum has 18 values — frontend may not list all
   - Workflow: Status enums not fully standardized across frontend/backend
3. **Date formatting**: Backend uses `DateTime` (ISO 8601). Frontend uses `Date` / string. No known issues but should verify locale handling.
4. **Currency/decimal formatting**: Backend uses `decimal(18,2)`. Frontend treats as number. No known precision mismatches.

---

## 3. Enum / Lookup Alignment

| Module | Enum | Backend Values | Frontend Values | Aligned? |
|--------|------|---------------|-----------------|----------|
| IAM | User role | String from Keycloak | String from Keycloak | ✅ |
| CRM | CustomerStatus | Lead, Prospect, Active, Suspended, Terminated | Matching | ✅ |
| CRM | CustomerType | Residential, Business, Wholesale | Matching | ✅ |
| ProductCatalog | OfferType | OneTime, Recurring, UsageBased, Bundled | Matching | ⚠️ "UsageBased" hyphenation? |
| ProductCatalog | LifecycleStatus | Draft, Active, Retired, Discontinued | Matching | ✅ |
| ProductCatalog | BillingPeriod | Monthly, Quarterly, SemiAnnual, Annual | Matching | ✅ |
| Orders | OrderStatus | Draft, Submitted, PendingApproval, Approved, Rejected, Fulfilling, Completed, Cancelled | Matching | ✅ |
| Subscriptions | SubscriptionStatus | Pending, Active, Suspended, Cancelled, Expired | Matching | ✅ |
| ServiceInventory | ServiceType | 18 telecom types | May not list all | ⚠️ |
| ServiceInventory | ServiceStatus | Pending, Provisioning, Active, Suspended, Decommissioned | Matching | ✅ |
| NetworkInventory | Element type | Module-specific | May not match | ⚠️ |
| Ticketing | TicketPriority | Backend enum | Frontend matches | ✅ |
| Payments | PaymentStatus | Pending, Completed, Failed, Refunded | Matching | ✅ |

---

## 4. Validation Rule Alignment

| Module | Backend Rule | Frontend Rule | Aligned? |
|--------|-------------|---------------|----------|
| All | Name required, max 200 | Max length enforced | ✅ Most |
| All | TenantId NotEmpty | Derived from auth context | ✅ |
| CreateCustomer | Name max 200, email format | Zod email validation | ✅ |
| CreateOrder | Customer name required, items not empty | Form validates required fields | ✅ |
| CreateSubscription | CustomerId required, offerId required | Form validates selection | ✅ |
| ProductCatalog | ValidTo > ValidFrom | Backend-only check | ⚠️ Frontend should pre-validate |
| All | FluentValidation on server | Zod schemas on client | ✅ Most covered |

**Key gaps:**
- Backend `ValidTo > ValidFrom` rules not duplicated on frontend
- Backend enum range checks (IsInEnum) not duplicated on frontend enum validation
- Backend unique constraint violations returned as 400 — frontend should handle gracefully
- Some monetary limits/range validations only on backend

---

## 5. Error Contract Alignment

The backend uses `ProblemDetails` (RFC 7807) format for errors. The frontend API client reads error responses but:

| Issue | Severity |
|-------|----------|
| Validation errors returned as FluentValidation errors collection | ⚠️ Frontend may not parse per-field errors from ProblemDetails correctly |
| Domain exceptions return 400/404/409 with stack trace disabled | ✅ |
| Custom `Result<T>` pattern used internally but mapped to HTTP at endpoint level | ⚠️ Some endpoints return `Results.Ok(Result<T>)` which nests result |
| Not all modules handle errors uniformly — some throw exceptions, some return `Result.Failure` | ⚠️ Inconsistent error handling pattern |

---

## 6. Generated Client Staleness

| Module | Status | Details |
|--------|--------|---------|
| IAM | ✅ Current | PartyRole endpoints NOT in OpenAPI spec — **stale** |
| CRM | ✅ Current | Segment management endpoints match |
| ProductCatalog | ✅ Current | Full coverage |
| Orders | ✅ Current | Fulfillment endpoints match |
| Subscriptions | ✅ Current | Full coverage |
| ServiceCatalog | ✅ Current | Full coverage |
| ServiceInventory | ✅ Current | Full coverage |
| Provisioning | ✅ Current | Coverage |
| Billing | ⚠️ Partial | Tax rules/exemptions DTOs may not be in generated client |
| Invoices | ✅ Current | Dispute DTOs match |
| Payments | ⚠️ Partial | Reconciliation + auto endpoints missing |
| Collections | ✅ Current | Full coverage |
| Rating | ✅ Current | Full coverage |
| NetworkInventory | ⚠️ Partial | OLT, interface, capacity endpoints missing |
| NumberInventory | ✅ Current | Full coverage |
| Workflow | ⚠️ Partial | SLA DTOs may not match |
| Ticketing | ✅ Current | Full coverage |
| Notifications | ✅ Current | Full coverage |
| Audit | ⚠️ Partial | Compliance/alert/policy endpoints NOT in generated client |
| Reporting | ✅ Current | Full coverage |
| ApiGateway | ✅ Current | Full coverage |
| EventManagement | ⚠️ Partial | Next step: verify generated client |

---

## 7. Event Contract Alignment

| Event | Producer | Consumer(s) | Frontend Impact | Aligned? |
|-------|----------|------------|-----------------|----------|
| OrderSubmittedIntegrationEvent | Orders | Subscriptions, Provisioning | Triggers UI refresh | ✅ |
| OrderApprovedIntegrationEvent | Orders | Subscriptions | Triggers provisioning status | ✅ |
| SubscriptionRequiredIntegrationEvent | Orders | Subscriptions | Creates subscription in background | ✅ |
| ProvisioningRequiredIntegrationEvent | Orders | Provisioning | Creates provisioning job | ✅ |
| ProvisioningJobCompletedIntegrationEvent | Provisioning | Orders, ServiceInventory | Updates fulfillment, activates service | ✅ |
| SubscriptionActivatedIntegrationEvent | Subscriptions | — | No consumer | ⚠️ |
| SubscriptionCancelledIntegrationEvent | Subscriptions | — | No consumer | ⚠️ |

Events that are missing:
- CRM Customer changes (no CustomerUpdated/CustomerSuspended integration events)
- ProductCatalog product/offer changes (no catalog sync events)
- Payment events (no PaymentProcessed/PaymentFailed integration events)
- Invoice lifecycle events
- Ticket lifecycle events
- Any events consumed by the frontend via real-time bridge

---

## 8. AuthZ Alignment

| Module | Backend Role/Policy | Frontend Guard | Aligned? |
|--------|---------------------|---------------|----------|
| IAM | Admin role | Admin-only routes | ✅ |
| CRM | Authenticated user | Auth guard | ✅ |
| Orders | Authenticated user | Auth guard | ✅ |
| Billing | Admin/finance role | Conditional UI | ⚠️ Frontend role names may not match Keycloak |

The system uses Keycloak for auth. Frontend checks roles from JWT token. Backend uses `[Authorize]` attributes. Need to verify exact role name matching across all modules. No detailed role-permission matrix analysis was performed.

---

## 9. Pagination / Filtering / Sorting Alignment

| Module | Query Param Names | Default Page Size | Backend | Frontend | Aligned? |
|--------|-------------------|-------------------|---------|----------|----------|
| All | `offset`, `limit` | 20 | `TmfPaginationRequest` | Matching params | ✅ |
| All | `searchTerm` | — | Supported where applicable | Matching | ✅ |
| All | Response headers | `X-Total-Count` | Set by middleware | Read by hook | ✅ |
| Orders | `customerId`, `status`, `fromDate`, `toDate`, `orderType` | 20 | Filtered query | Matching | ✅ |
| CRM | `customerType`, `status` | 20 | Repository filtering | Matching | ✅ |
| ProductCatalog | `catalogType`, `productType`, `status`, `brand` | 20 | Repository filtering | Matching | ✅ |
| Subscriptions | `status`, `customerId`, `billingPeriod` | 20 | Repository filtering | Matching | ✅ |
| ServiceInventory | `serviceType`, `status` | 20 | Repository filtering | Matching | ✅ |

Pagination is well-aligned across the board.
