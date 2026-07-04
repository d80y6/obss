// Auto-generated response DTO types
// Generated from OpenAPI command schemas + backend DTO knowledge

export type BillingPeriod = 'Monthly' | 'Quarterly' | 'SemiAnnual' | 'Annual';
export type OfferType = 'OneTime' | 'Recurring' | 'UsageBased' | 'Bundled';
export type PricingType = 'Flat' | 'Tiered' | 'Volume';
export type RuleType = 'Required' | 'Allowed' | 'Excluded' | 'Dependent';
export type OptionType = 'Choice' | 'Quantity' | 'Toggle';
export type ProductType = 'Physical' | 'Digital' | 'Service';
export type DiscountType = 'Percentage' | 'Fixed' | 'FreePeriod';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';
export type SlaLevel = 'Basic' | 'Standard' | 'Premium';
export type LifecycleStatus = 'Draft' | 'Active' | 'Retired' | 'Discontinued';

// Number Inventory
export type NumberType = 'Geographic' | 'Mobile' | 'TollFree' | 'National' | 'Premium' | 'SharedCost';
export type NumberStatus = 'Available' | 'Reserved' | 'Assigned' | 'Ported' | 'Suspended' | 'Disconnected';

export interface TelephoneNumberDto {
  id: string;
  tenantId: string;
  number: string;
  numberType: string;
  status: string;
  customerId: string | null;
  subscriptionId: string | null;
  assignedAt: string | null;
  reservedAt: string | null;
  cost: number;
  currency: string;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

// CRM
export interface AccountRefDto {
  billingAccountId: string;
  name: string;
  accountType: string;
  role: string;
  href: string | null;
}

export interface AgreementRefDto {
  agreementId: string;
  name: string;
  agreementType: string;
  role: string;
  href: string | null;
}

export interface PaymentMethodRefDto {
  paymentMethodId: string;
  name: string;
  href: string | null;
}

export interface CustomerDto {
  id: string;
  tenantId: string;
  customerType: string;
  status: string;
  companyName: string | null;
  displayName: string;
  taxNumber: string | null;
  registrationNumber: string | null;
  email: string;
  phoneNumber: string | null;
  countryCode: string | null;
  website: string | null;
  isActive: boolean;
  creditLimit: number;
  currency: string;
  createdAt: string;
  updatedAt: string;
  description: string | null;
  statusReason: string | null;
  externalId: string | null;
  href: string | null;
  validFrom: string | null;
  validUntil: string | null;
  individual: IndividualDto | null;
  organization: OrganizationDto | null;
  characteristics: CharValueDto[];
  creditProfiles: CreditProfileDto[];
  relatedParties: RelatedPartyDto[];
  contacts: ContactDto[];
  notes: CustomerNoteDto[];
  notificationHubs: NotificationHubDto[];
  contactMedia: ContactMediumDto[];
  accountRefs: AccountRefDto[];
  agreementRefs: AgreementRefDto[];
  paymentMethodRefs: PaymentMethodRefDto[];
}

export interface IndividualDto {
  id: string;
  firstName: string;
  lastName: string;
  middleName: string | null;
  salutation: string | null;
  title: string | null;
  birthDate: string | null;
  nationality: string | null;
  gender: string | null;
  kycStatus: string;
  kycVerifiedAt: string | null;
  kycVerifiedBy: string | null;
  riskRating: string;
  documents: IdentityDocumentDto[];
}

export interface IdentityDocumentDto {
  id: string;
  individualId: string;
  documentType: string;
  documentNumber: string;
  issuingAuthority: string | null;
  issuingCountry: string | null;
  issuedDate: string | null;
  expiryDate: string | null;
  isVerified: boolean;
}

export interface OrganizationDto {
  id: string;
  tradingName: string;
  companyType: string;
  industry: string | null;
  registrationNumber: string | null;
  taxNumber: string | null;
  countryOfRegistration: string | null;
  kycStatus: string;
  kycVerifiedAt: string | null;
  kycVerifiedBy: string | null;
}

export interface CharValueDto {
  key: string;
  value: string;
  valueType: string;
}

export interface CreditProfileDto {
  id: string;
  customerId: string;
  score: number;
  scoreType: string;
  validFrom: string | null;
  validUntil: string | null;
  riskRating: string | null;
}

export interface NotificationHubDto {
  hubType: string;
  identifier: string;
  isOptIn: boolean;
  validFrom: string | null;
  validUntil: string | null;
}

export interface ContactMediumDto {
  mediumType: string;
  isPreferred: boolean;
  validFrom: string | null;
  validUntil: string | null;
  characteristics: ContactCharValueDto[];
}

export interface ContactCharValueDto {
  key: string;
  value: string;
  valueType: string;
}

export interface RelatedPartyDto {
  name: string;
  role: string;
  referredId: string;
  referredType: string;
}

export interface CustomerNoteDto {
  id: string;
  customerId: string;
  content: string;
  category: string;
  createdById: string;
  createdAt: string;
}

export interface ContactDto {
  id: string;
  customerId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  countryCode: string | null;
  mobileNumber: string | null;
  position: string | null;
  isPrimary: boolean;
  isBilling: boolean;
  isTechnical: boolean;
  createdAt: string;
}

export interface NoteDto {
  id: string;
  customerId: string;
  content: string;
  category: string;
  author: string;
  createdAt: string;
}

export interface SegmentDto {
  id: string;
  name: string;
  description: string;
  rules: Record<string, unknown>;
  customerCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface PermissionDto {
  id: string;
  code: string;
  name: string;
  description: string | null;
  module: string;
  resource: string;
  action: string;
}

export interface RoleDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissions: PermissionDto[];
  createdAt: string;
  updatedAt: string;
}

// IAM
export interface IamUserDto {
  id: string;
  tenantId?: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  isActive: boolean;
  emailVerified: boolean;
  lastLoginAt?: string | null;
  externalId?: string | null;
  role?: string;
  createdAt: string;
  updatedAt: string;
}

export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  connectionString: string | null;
  isActive: boolean;
  settings: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CategoryDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  parentCategoryId: string | null;
  isActive: boolean;
  lifecycleStatus: string;
  sortOrder: number;
  version: number;
  validFrom: string | null;
  validTo: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CatalogDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  catalogType: string | null;
  lifecycleStatus: string;
  version: number;
  validFrom: string | null;
  validTo: string | null;
  createdAt: string;
  updatedAt: string;
}

// Catalog
export interface ProductDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  categoryId: string | null;
  categoryName: string | null;
  productType: string;
  isActive: boolean;
  isShippable: boolean;
  taxable: boolean;
  taxCategory: string | null;
  productNumber: string | null;
  productSpecificationId: string | null;
  lifecycleStatus: string;
  createdAt: string;
  updatedAt: string;
  specifications: ProductSpecValueDto[];
  offers: OfferDto[];
}

export interface ProductSpecValueDto {
  name: string;
  value: string;
  isRequired: boolean;
}

export interface ProductSpecificationCharacteristicDto {
  id: string;
  productSpecificationId: string;
  name: string;
  description: string | null;
  valueType: string;
  configurable: boolean;
  minValue: number | null;
  maxValue: number | null;
  regex: string | null;
  sortOrder: number;
  maxCardinality: number | null;
  isRequired: boolean;
  values: ProductSpecificationCharacteristicValueDto[];
}

export interface ProductSpecificationCharacteristicValueDto {
  id: string;
  characteristicId: string;
  value: string;
  unitOfMeasure: string | null;
  isDefault: boolean;
  valueFrom: string | null;
  valueTo: string | null;
  rangeInterval: string | null;
  validFrom: string | null;
  validTo: string | null;
}

export interface ProductSpecificationDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  brand: string | null;
  version: string | null;
  productNumber: string | null;
  lifecycleStatus: string;
  validFrom: string | null;
  validTo: string | null;
  createdAt: string;
  updatedAt: string;
  characteristics: ProductSpecificationCharacteristicDto[];
  relationships: ProductSpecificationRelationshipDto[];
}

export interface ProductSpecificationRelationshipDto {
  id: string;
  productSpecificationId: string;
  targetSpecificationId: string;
  relationshipType: string;
  role: string | null;
  validFrom: string | null;
  validTo: string | null;
}

export interface BundledProductOfferingDto {
  id: string;
  offerId: string;
  bundledOfferId: string;
  name: string | null;
  quantity: number;
  referralType: string | null;
  bundledOffer: OfferDto | null;
}

export interface ProductOfferingTermDto {
  id: string;
  offerId: string;
  name: string;
  description: string | null;
  duration: number;
  durationUnit: string;
  termType: string;
  validFrom: string | null;
  validTo: string | null;
}

export interface OfferDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  offerType: string;
  isActive: boolean;
  isContract: boolean;
  contractDurationMonths: number | null;
  billingPeriod: string | null;
  taxInclusive: boolean;
  sortOrder: number;
  validFrom: string | null;
  validTo: string | null;
  createdAt: string;
  updatedAt: string;
  pricings: OfferPricingDto[];
  discounts: OfferDiscountDto[];
  terms: ProductOfferingTermDto[];
  bundledOfferings?: BundledProductOfferingDto[];
}

export interface PriceRangeDto {
  id: string;
  offerPricingId: string;
  minQuantity: number;
  maxQuantity: number | null;
  price: number;
  isActive: boolean;
}

export interface OfferPricingDto {
  id: string;
  offerId: string;
  pricingType: string;
  currency: string;
  recurringPrice: number;
  oneTimePrice: number;
  usagePrice: number;
  unitOfMeasure: string | null;
  minQuantity: number | null;
  maxQuantity: number | null;
  isActive: boolean;
  name?: string | null;
  description?: string | null;
  priceApplicationType?: string | null;
  priceRanges?: PriceRangeDto[];
}

export interface OfferDiscountDto {
  id: string;
  discountType: string;
  discountValue: number;
  discountPeriodMonths: number | null;
  validFrom: string | null;
  validTo: string | null;
  isActive: boolean;
  description: string | null;
}

// Orders
export interface OrderDto {
  id: string;
  tenantId: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  orderDate: string;
  status: string;
  orderType: string;
  subTotal: number;
  taxTotal: number;
  discountTotal: number;
  grandTotal: number;
  currency: string;
  notes: string | null;
  billingAddressStreet: string | null;
  billingAddressCity: string | null;
  billingAddressState: string | null;
  billingAddressPostalCode: string | null;
  billingAddressCountry: string | null;
  shippingAddressStreet: string | null;
  shippingAddressCity: string | null;
  shippingAddressState: string | null;
  shippingAddressPostalCode: string | null;
  shippingAddressCountry: string | null;
  createdById: string;
  approvedById: string | null;
  approvedAt: string | null;
  cancellationReason: string | null;
  description: string | null;
  channel: string | null;
  priority: string | null;
  requestedStartDate: string | null;
  requestedCompletionDate: string | null;
  expectedCompletionDate: string | null;
  notificationContact: string | null;
  externalId: string | null;
  quoteId: string | null;
  createdAt: string;
  items: OrderItemDto[];
  payments: OrderPaymentDto[];
}

export interface OrderItemDto {
  id: string;
  orderId: string;
  productId: string;
  productName: string;
  offerId: string;
  offerName: string;
  quantity: number;
  unitPrice: number;
  recurringPrice: number;
  discountAmount: number;
  taxAmount: number;
  totalPrice: number;
  billingPeriod: string;
  isActive: boolean;
  startDate: string;
  endDate: string | null;
}

export interface OrderPaymentDto {
  id: string;
  orderId: string;
  amount: number;
  paymentMethod: string;
  paymentReference: string;
  paidAt: string;
  status: string;
}

export interface OrderFulfillmentDto {
  id: string;
  orderId: string;
  status: string;
  workflowInstanceId: string | null;
  startedAt: string;
  completedAt: string | null;
  errorMessage: string | null;
}

export interface EntitlementDto {
  id: string;
  subscriptionId: string;
  entitlementType: string;
  name: string;
  limit: number;
  used: number;
  unit: string;
  isUnlimited: boolean;
  isOverridable: boolean;
  validFrom: string;
  validTo: string | null;
}

export interface EntitlementDefinition {
  entitlementType: string;
  name: string;
  limit: number;
  used: number;
  unit: string;
  isUnlimited: boolean;
  isOverridable: boolean;
  validFrom: string;
  validTo: string | null;
}

// Subscriptions
export interface SubscriptionDto {
  id: string;
  tenantId: string;
  customerId: string;
  customerName: string;
  orderId: string;
  orderItemId: string;
  productId: string;
  offerId: string;
  offerName: string;
  status: string;
  billingPeriod: string;
  currency: string;
  price: number;
  quantity: number;
  startDate: string;
  endDate: string | null;
  cancelledAt: string | null;
  suspendedAt: string | null;
  activationDate: string | null;
  renewalDate: string | null;
  createdAt: string;
  updatedAt: string;
}

// CRM Agreements
export interface AgreementDto {
  id: string;
  customerId: string;
  name: string;
  agreementType: string;
  status: string;
  validFrom: string | null;
  validUntil: string | null;
  description: string | null;
  signedAt: string | null;
  signedBy: string | null;
  createdAt: string;
  updatedAt: string;
}

// Billing
export interface BillingAccountDto {
  id: string;
  customerId: string;
  accountType: string;
  name: string;
  status: string;
  creditLimit: number;
  currency: string;
  validFrom: string | null;
  validUntil: string | null;
  description: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface BillDto {
  id: string;
  billNumber: string;
  customerId: string;
  customerName: string;
  period: string;
  status: string;
  totalAmount: number;
  currency: string;
  issueDate: string;
  dueDate: string;
  createdAt: string;
  updatedAt: string;
}

export interface TaxRuleDto {
  id: string;
  name: string;
  rate: number;
  region: string;
  productCategory: string;
  status: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  createdAt: string;
}

export interface BillAdjustmentDto {
  id: string;
  billId: string;
  amount: number;
  reason: string;
  type: string;
  createdBy: string;
  createdAt: string;
}

export interface BillingJobDto {
  id: string;
  name: string;
  type: string;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  totalProcessed: number;
  totalErrors: number;
  createdAt: string;
}

export interface BillingCycleDto {
  id: string;
  name: string;
  billingDate: string;
  period: string;
  status: string;
  customerCount: number;
  totalAmount: number;
  executedAt: string | null;
  createdAt: string;
}

// Invoices
export interface InvoiceDto {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  subscriptionId: string;
  issueDate: string;
  dueDate: string;
  totalAmount: number;
  currency: string;
  status: string;
  paidAmount: number;
  balance: number;
  lineItems: InvoiceLineItemDto[];
  createdAt: string;
  updatedAt: string;
}

export interface InvoiceLineItemDto {
  description: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface CreditNoteDto {
  id: string;
  creditNoteNumber: string;
  invoiceId: string;
  invoiceNumber: string;
  customerId: string;
  amount: number;
  currency: string;
  reason: string;
  status: string;
  createdAt: string;
}

export interface DisputeDto {
  id: string;
  invoiceId: string;
  invoiceNumber: string;
  customerId: string;
  reason: string;
  amount: number;
  status: string;
  resolution: string | null;
  createdAt: string;
  updatedAt: string;
}

// Payments
export interface RefundDto {
  id: string;
  refundNumber: string;
  paymentId: string;
  paymentNumber: string;
  amount: number;
  currency: string;
  reason: string;
  status: string;
  createdAt: string;
}

export interface ReconciliationDto {
  id: string;
  statementDate: string;
  totalAmount: number;
  matchedCount: number;
  unmatchedCount: number;
  status: string;
  createdAt: string;
}

export interface PaymentDto {
  id: string;
  paymentNumber: string;
  invoiceId: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  amount: number;
  currency: string;
  method: string;
  status: string;
  transactionId: string;
  paidAt: string;
  createdAt: string;
  updatedAt: string;
}

// Ticketing
export interface TicketCommentDto {
  id: string;
  ticketId: string;
  author: string;
  content: string;
  isInternal: boolean;
  createdAt: string;
}

export interface TicketDto {
  id: string;
  ticketNumber: string;
  customerId: string;
  customerName: string;
  subject: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  assignedTo: string | null;
  resolution: string | null;
  source: string;
  createdAt: string;
  updatedAt: string;
}

export interface SlaDefinitionDto {
  id: string;
  name: string;
  description: string;
  responseTime: number;
  resolutionTime: number;
  priority: string;
  slaLevel: string;
  createdAt: string;
}

// Network
export interface NetworkLinkDto {
  id: string;
  sourceId: string;
  sourceName: string;
  targetId: string;
  targetName: string;
  type: string;
  status: string;
  bandwidth: number | null;
  createdAt: string;
}

export interface NetworkConnectionDto {
  id: string;
  elementId: string;
  connectedElementId: string;
  connectedElementName: string;
  type: string;
  status: string;
  interface_: string;
  createdAt: string;
}

export interface PonPortDto {
  id: string;
  oltId: string;
  portNumber: number;
  status: string;
  ontCount: number;
  maxOntCount: number;
}

export interface CapacityAlertDto {
  id: string;
  elementId: string;
  elementName: string;
  metric: string;
  threshold: number;
  currentValue: number;
  severity: string;
  createdAt: string;
}

export interface CapacityOverviewDto {
  totalElements: number;
  totalBandwidth: number;
  usedBandwidth: number;
  alerts: CapacityAlertDto[];
}

export interface NetworkElementDto {
  id: string;
  tenantId: string;
  name: string;
  hostname: string;
  ipAddress: string;
  elementType: string;
  vendor: string;
  model: string;
  softwareVersion: string | null;
  serialNumber: string | null;
  location: string | null;
  status: string;
  managementIP: string | null;
  isManaged: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SubnetDto {
  id: string;
  tenantId: string;
  network: string;
  name: string;
  description: string | null;
  gateway: string | null;
  vlanId: number;
  location: string | null;
  status: string;
  createdAt: string;
}

export interface VlanDto {
  id: string;
  vlanId: number;
  name: string;
  description: string;
  status: string;
  subnet: string;
  createdAt: string;
}

export interface OltDto {
  id: string;
  name: string;
  model: string;
  vendor: string;
  status: string;
  location: string;
  ponPortCount: number;
  ipAddress: string;
  createdAt: string;
}

// Service Inventory
export interface ServiceDto {
  id: string;
  tenantId: string;
  customerId: string;
  subscriptionId: string;
  serviceType: string;
  serviceIdentifier: string;
  status: string;
  activationDate: string | null;
  suspendedAt: string | null;
  decommissionedAt: string | null;
  configuration: string | null;
  location: string | null;
  createdAt: string;
  updatedAt: string;
  resources: ServiceResourceDto[];
}

export interface ServiceResourceDto {
  id: string;
  serviceId: string;
  resourceType: string;
  resourceIdentifier: string;
  status: string;
  allocatedAt: string;
  releasedAt: string | null;
}

export interface ServiceTopologyDto {
  id: string;
  serviceId: string;
  topologyType: string;
  createdAt: string;
  updatedAt: string;
  links: TopologyLinkDto[];
}

export interface TopologyLinkDto {
  id: string;
  serviceTopologyId: string;
  sourceServiceId: string;
  targetServiceId: string;
  linkType: string;
  direction: string;
  attributes: string | null;
}

export interface DiscoveryJobDto {
  id: string;
  tenantId: string;
  discoveryType: string;
  configuration: string | null;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  resourcesFound: number;
  resourcesMatched: number;
  errorMessage: string | null;
  createdBy: string;
  createdAt: string;
}

// Provisioning
export interface ProvisioningJobDto {
  id: string;
  name: string;
  type: string;
  status: string;
  serviceId: string | null;
  orderId: string | null;
  templateId: string | null;
  startedAt: string | null;
  completedAt: string | null;
  createdAt: string;
  updatedAt: string;
  parameters: Record<string, string>;
  logs: ProvisioningLogEntryDto[] | null;
}

export interface ProvisioningLogEntryDto {
  id: string;
  step: number;
  message: string;
  level: string;
  result: string | null;
  error: string | null;
  timestamp: string;
}

export interface ProvisioningTemplateDto {
  id: string;
  name: string;
  description: string;
  serviceType: string;
  steps: ProvisioningTemplateStepDto[];
  parameters: ProvisioningTemplateParameterDto[] | null;
  createdAt: string;
  updatedAt: string;
}

export interface ProvisioningTemplateStepDto {
  id: string;
  name: string;
  order: number;
  type: string;
  config: Record<string, unknown>;
}

export interface ProvisioningTemplateParameterDto {
  name: string;
  type: string;
  default: string;
  required: boolean;
}

// Workflow
export interface WorkflowDefinitionDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  category: string;
  isActive: boolean;
  version: number;
  createdAt: string;
  updatedAt: string;
  steps: WorkflowStepDto[];
}

export interface WorkflowStepDto {
  id: string;
  workflowDefinitionId: string;
  stepNumber: number;
  name: string;
  description: string | null;
  stepType: string;
  handlerType: string | null;
  configuration: string | null;
  timeout: number;
  retryCount: number;
  retryDelaySeconds: number;
  isRequired: boolean;
  createdAt: string;
}

export interface WorkflowInstanceDto {
  id: string;
  workflowDefinitionId: string;
  workflowDefinitionName: string;
  triggerEntityType: string;
  triggerEntityId: string;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  createdBy: string;
  tasks: WorkflowTaskDto[];
}

export interface WorkflowTaskDto {
  id: string;
  workflowInstanceId: string;
  workflowStepId: string;
  stepName: string;
  status: string;
  assignedTo: string | null;
  startedAt: string | null;
  completedAt: string | null;
  result: string | null;
  errorMessage: string | null;
  retryCount: number;
}

export interface WorkflowSlaDto {
  id: string;
  definitionId: string;
  name: string;
  description: string;
  responseTime: number;
  resolutionTime: number;
  status: string;
  thresholds: Record<string, number>;
  createdAt: string;
  updatedAt: string;
}

// Notifications
export interface NotificationDto {
  id: string;
  type: string;
  title: string;
  message: string;
  channel: string;
  status: string;
  recipientId: string;
  readAt: string | null;
  createdAt: string;
}

export interface NotificationTemplateDto {
  id: string;
  name: string;
  subject: string;
  body: string;
  channel: string;
  variables: string[];
  createdAt: string;
  updatedAt: string;
}

export interface NotificationPreferenceDto {
  id: string;
  customerId: string;
  notificationType: string;
  channel: string;
  isOptedIn: boolean;
  optInAt: string | null;
  optOutAt: string | null;
}

// Collections
export interface CollectionCaseDto {
  id: string;
  tenantId: string;
  customerId: string;
  customerName: string;
  status: string;
  totalOverdueAmount: number;
  currency: string;
  currentDunningLevel: number;
  openedAt: string;
  lastActionAt: string | null;
  resolvedAt: string | null;
  assignedTo: string | null;
  notes: string | null;
  actions: CollectionActionDto[];
  paymentArrangements: PaymentArrangementDto[];
}

export interface CollectionActionDto {
  id: string;
  collectionCaseId: string;
  actionType: string;
  dunningLevel: number;
  description: string;
  performedAt: string;
  performedBy: string | null;
  nextActionDate: string | null;
  isCompleted: boolean;
}

export interface PaymentArrangementDto {
  id: string;
  collectionCaseId: string;
  customerId: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
  installmentCount: number;
  installmentAmount: number;
  frequency: string;
  firstPaymentDate: string;
  lastPaymentDate: string | null;
  createdAt: string;
  defaultedAt: string | null;
}

// API Gateway
export interface ApiRouteDto {
  id: string;
  tenantId: string;
  path: string;
  method: string;
  targetModule: string;
  targetPath: string;
  requireAuthentication: boolean;
  requiredPermissions: string[];
  rateLimitPerMinute: number;
  isActive: boolean;
}

export interface ApiKeyDto {
  id: string;
  tenantId: string;
  partnerId: string | null;
  name: string;
  key: string;
  status: string;
  permissions: string[];
  allowedIPs: string[];
  rateLimitPerMinute: number;
  expiresAt: string | null;
  createdAt: string;
  revokedAt: string | null;
}

export interface PartnerDto {
  id: string;
  tenantId: string;
  name: string;
  contactName: string;
  contactEmail: string;
  allowedIPs: string[];
  isActive: boolean;
  slaLevel: string;
  maxRequestsPerDay: number;
  createdAt: string;
  apiKeys: ApiKeyDto[] | null;
}

// Reporting
export interface ReportDefinitionDto {
  id: string;
  name: string;
  description: string;
  type: string;
  config: Record<string, unknown>;
  createdAt: string;
}

export interface ReportExecutionDto {
  id: string;
  definitionId: string;
  status: string;
  outputFormat: string;
  startedAt: string;
  completedAt: string | null;
  error: string | null;
}

export interface ScheduledReportDto {
  id: string;
  tenantId: string;
  reportDefinitionId: string;
  cronExpression: string;
  recipients: string[];
  lastRunAt: string | null;
  nextRunAt: string | null;
  isActive: boolean;
}

export interface DashboardWidgetDto {
  id: string;
  title: string;
  type: string;
  config: Record<string, unknown>;
  position: number;
  size: string;
}

// Audit
export interface AuditEntryDto {
  id: string;
  tenantId: string;
  entityType: string;
  entityId: string;
  action: string;
  changes: string | null;
  performedById: string | null;
  performedByName: string | null;
  performedAt: string;
  ipAddress: string | null;
  userAgent: string | null;
  correlationId: string | null;
  isSensitive: boolean;
}

export interface AuditAlertDto {
  id: string;
  ruleName: string;
  severity: string;
  message: string;
  acknowledged: boolean;
  createdAt: string;
}

export interface AlertRuleDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  alertType: string;
  severity: string;
  threshold: number;
  windowMinutes: number;
  isActive: boolean;
  createdAt: string;
}

// Rating
export interface RatingRuleDto {
  id: string;
  name: string;
  description: string;
  priority: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface PromotionDto {
  id: string;
  code: string;
  name: string;
  description: string;
  discountType: string;
  discountValue: number;
  isActive: boolean;
  validFrom: string;
  validTo: string;
  createdAt: string;
  updatedAt: string;
}

export interface UsageRecordDto {
  id: string;
  subscriptionId: string;
  usageType: string;
  quantity: number;
  unit: string;
  recordedAt: string;
  status: string;
}

// Error types
export interface ProblemDetails {
  type: string | null;
  title: string | null;
  status: number | null;
  detail: string | null;
  instance: string | null;
  extensions: Record<string, unknown> | null;
}

export interface ValidationProblemDetails extends ProblemDetails {
  errors: Record<string, string[]> | null;
}
