import type { components } from './types';

// Request command types (from OpenAPI schemas)
export type CreateCategoryRequest = components["schemas"]["CreateCategoryRequest"];
export type CreateCustomerCommand = components["schemas"]["CreateCustomerCommand"];
export type CreateOrderCommand = components["schemas"]["CreateOrderCommand"];
export type CreateSubscriptionCommand = components["schemas"]["CreateSubscriptionCommand"];
export type CreateProductCommand = components["schemas"]["CreateProductCommand"];
export type CreateOfferCommand = components["schemas"]["CreateOfferCommand"];
export type CreateUserCommand = components["schemas"]["CreateUserCommand"];
export type CreateTenantCommand = components["schemas"]["CreateTenantCommand"];
export type CreateTicketCommand = components["schemas"]["CreateTicketCommand"];
export type CreateInvoiceFromBillCommand = components["schemas"]["CreateInvoiceFromBillCommand"];
export type RecordPaymentCommand = components["schemas"]["RecordPaymentCommand"];
export type GenerateBillCommand = components["schemas"]["GenerateBillCommand"];
export type CreateNetworkElementCommand = components["schemas"]["CreateNetworkElementCommand"];
export type CreateServiceCommand = components["schemas"]["CreateServiceCommand"];
export type CreateProvisioningJobCommand = components["schemas"]["CreateProvisioningJobCommand"];
export type CreateProvisioningTemplateCommand = components["schemas"]["CreateProvisioningTemplateCommand"];
export type CreateWorkflowDefinitionCommand = components["schemas"]["CreateWorkflowDefinitionCommand"];
export type CreateSegmentCommand = components["schemas"]["CreateSegmentCommand"];
export type CreateVLANCommand = components["schemas"]["CreateVLANCommand"];
export type CreateOLTCommand = components["schemas"]["CreateOLTCommand"];
export type CreateSubnetCommand = components["schemas"]["CreateSubnetCommand"];
export type CreateConnectivityLinkCommand = components["schemas"]["CreateConnectivityLinkCommand"];
export type CreateWorkflowSlaCommand = components["schemas"]["CreateWorkflowSlaCommand"];
export type CreateSlaDefinitionCommand = components["schemas"]["CreateSlaDefinitionCommand"];
export type CreateRatingRuleCommand = components["schemas"]["CreateRatingRuleCommand"];
export type CreatePromotionCommand = components["schemas"]["CreatePromotionCommand"];
export type SubmitUsageCommand = components["schemas"]["SubmitUsageCommand"];
export type CreateReportDefinitionCommand = components["schemas"]["CreateReportDefinitionCommand"];
export type CreateDashboardWidgetCommand = components["schemas"]["CreateDashboardWidgetCommand"];
export type ScheduleReportCommand = components["schemas"]["ScheduleReportCommand"];
export type SendNotificationCommand = components["schemas"]["SendNotificationCommand"];
export type CreateNotificationTemplateCommand = components["schemas"]["CreateNotificationTemplateCommand"];
export type UpdateTemplateCommand = components["schemas"]["UpdateTemplateCommand"];
export type CreateApiKeyCommand = components["schemas"]["CreateApiKeyCommand"];
export type RegisterApiRouteCommand = components["schemas"]["RegisterApiRouteCommand"];
export type RegisterPartnerCommand = components["schemas"]["RegisterPartnerCommand"];
export type OpenCollectionCaseCommand = components["schemas"]["OpenCollectionCaseCommand"];
export type CreatePaymentArrangementCommand = components["schemas"]["CreatePaymentArrangementCommand"];
export type CreateAlertRuleCommand = components["schemas"]["CreateAlertRuleCommand"];
export type RecordCapacityCommand = components["schemas"]["RecordCapacityCommand"];
export type ApplyPromotionCommand = components["schemas"]["ApplyPromotionCommand"];
export type CreateTaxRuleCommand = components["schemas"]["CreateTaxRuleCommand"];
export type GenerateBillingCycleCommand = components["schemas"]["GenerateBillingCycleCommand"];
export type CreateAuditEntryCommand = components["schemas"]["CreateAuditEntryCommand"];
export type EscalateTicketCommand = components["schemas"]["EscalateTicketCommand"];
export type AssignTicketCommand = components["schemas"]["AssignTicketCommand"];
export type ResolveTicketCommand = components["schemas"]["ResolveTicketCommand"];
export type RefundPaymentCommand = components["schemas"]["RefundPaymentCommand"];
export type CancelOrderCommand = components["schemas"]["CancelOrderCommand"];
export type UpdateCustomerCommand = components["schemas"]["UpdateCustomerCommand"];
export type UpdateUserCommand = components["schemas"]["UpdateUserCommand"];
export type UpdateProductCommand = components["schemas"]["UpdateProductCommand"];
export type UpdateNetworkElementCommand = components["schemas"]["UpdateNetworkElementCommand"];
export type CancelSubscriptionCommand = components["schemas"]["CancelSubscriptionCommand"];
export type SuspendSubscriptionCommand = components["schemas"]["SuspendSubscriptionCommand"];
export type AddContactCommand = components["schemas"]["AddContactCommand"];
export type AddNoteCommand = components["schemas"]["AddNoteCommand"];
export type ValidateConfigurationQuery = components["schemas"]["ValidateConfigurationQuery"];

export type CompleteOrderFulfillmentCommand = components["schemas"]["CompleteOrderFulfillmentCommand"];
export type RecordInvoicePaymentCommand = components["schemas"]["RecordInvoicePaymentCommand"];
export type IssueCreditNoteCommand = components["schemas"]["IssueCreditNoteCommand"];
export type OpenDisputeCommand = components["schemas"]["OpenDisputeCommand"];
export type ResolveDisputeCommand = components["schemas"]["ResolveDisputeCommand"];
export type RejectDisputeCommand = components["schemas"]["RejectDisputeCommand"];
export type SuspendServiceCommand = components["schemas"]["SuspendServiceCommand"];

// Enum types (from dto.ts - synced with backend enum definitions)
export type BillingPeriod = import('./dto').BillingPeriod;
export type OfferType = import('./dto').OfferType;
export type PricingType = import('./dto').PricingType;
export type RuleType = import('./dto').RuleType;
export type OptionType = import('./dto').OptionType;
export type ProductType = import('./dto').ProductType;
export type TicketPriority = import('./dto').TicketPriority;
export type SlaLevel = import('./dto').SlaLevel;
export type DiscountType = import('./dto').DiscountType;
export type LifecycleStatus = import('./dto').LifecycleStatus;

// Response DTO types (from manual dto.ts)
export type {
  CustomerDto, IndividualDto, IdentityDocumentDto, OrganizationDto,
  CharValueDto, CreditProfileDto, RelatedPartyDto, CustomerNoteDto, NotificationHubDto, ContactMediumDto, ContactCharValueDto,
  OrderDto, SubscriptionDto, ProductDto,
  ProductSpecValueDto,
  ProductSpecificationCharacteristicDto,
  ProductSpecificationCharacteristicValueDto,
  ProductSpecificationDto,
  ProductSpecificationRelationshipDto,
  BundledProductOfferingDto, OfferDto,
  IamUserDto, TenantDto, TicketDto, InvoiceDto, PaymentDto, BillDto,
  NetworkElementDto, ServiceDto, ProvisioningJobDto, ProvisioningTemplateDto,
  WorkflowDefinitionDto, WorkflowInstanceDto, SegmentDto, RoleDto,
  ContactDto, NoteDto, VlanDto, OltDto, SubnetDto, WorkflowSlaDto,
  SlaDefinitionDto, RatingRuleDto, PromotionDto, UsageRecordDto,
  ReportDefinitionDto, ReportExecutionDto, DashboardWidgetDto, ScheduledReportDto,
  NotificationDto, NotificationPreferenceDto, NotificationTemplateDto, ApiKeyDto, ApiRouteDto,
  PartnerDto, CollectionCaseDto, CollectionActionDto, PaymentArrangementDto,
  CreditNoteDto, DisputeDto, InvoiceLineItemDto, OrderItemDto,
  WorkflowStepDto, WorkflowTaskDto, ProvisioningTemplateStepDto,
  ProvisioningTemplateParameterDto, ProvisioningLogEntryDto,
  AlertRuleDto, TaxRuleDto, BillingCycleDto, BillAdjustmentDto, BillingJobDto, BillingAccountDto,
  AuditEntryDto, AuditAlertDto, DiscoveryJobDto, TelephoneNumberDto,
  ServiceTopologyDto, ServiceResourceDto, TopologyLinkDto, NetworkLinkDto, NetworkConnectionDto,
  PonPortDto, CapacityAlertDto, CapacityOverviewDto, EntitlementDto, EntitlementDefinition,
  RefundDto, ReconciliationDto, TicketCommentDto, CategoryDto, CatalogDto,
  OrderPaymentDto, OrderFulfillmentDto, PermissionDto, ProblemDetails, ValidationProblemDetails,
  OfferPricingDto, OfferDiscountDto, AgreementDto, ProductOfferingTermDto,
} from './dto';

// Re-export raw types for advanced usage
export type { components, paths } from './types';
