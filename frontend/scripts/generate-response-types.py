#!/usr/bin/env python3
"""
Generate TypeScript response DTO types from OpenAPI command schemas.
Each CreateXxxCommand typically maps to XxxDto response type.
"""
import json
import re
import os

with open('/home/ubuntu/obss/frontend/openapi.json') as f:
    spec = json.load(f)

schemas = spec.get('components', {}).get('schemas', {})
command_types = {}
enum_types = {}

# Identify command types and enums
for name in schemas:
    if name.startswith('Create') and name.endswith('Command'):
        command_types[name] = schemas[name]
    elif name == name.upper() or name in ('BillingPeriod', 'OfferType', 'PricingType', 'RuleType', 'OptionType', 
        'ProductType', 'TicketPriority', 'SlaLevel', 'LifecycleStatus', 'SubscriptionStatus'):
        enum_types[name] = schemas[name]

# Known backend DTO field overrides (extra fields not in commands)
DTO_EXTRA_FIELDS = {
    'Customer': {
        'status': 'string',
        'isActive': 'boolean',
        'creditLimit': 'number',
    },
    'Order': {
        'orderNumber': 'string',
        'status': 'string',
        'subTotal': 'number',
        'taxTotal': 'number',
        'discountTotal': 'number',
        'grandTotal': 'number',
        'createdById': 'string',
        'orderDate': 'string /* date-time */',
        'items': 'OrderItemDto[]',
        'payments': 'PaymentDto[]',
    },
    'Offer': {
        'status': 'string',
        'productName': 'string',
        'pricingType': 'PricingType',
    },
    'Product': {
        'status': 'string',
    },
    'Subscription': {
        'subscriptionNumber': 'string',
        'status': 'string',
        'customerName': 'string',
        'offerName': 'string',
    },
    'Invoice': {
        'invoiceNumber': 'string',
        'status': 'string',
        'customerName': 'string',
        'lineItems': 'InvoiceLineItemDto[]',
        'paidAmount': 'number',
        'balance': 'number',
    },
    'Payment': {
        'paymentNumber': 'string',
        'status': 'string',
        'customerName': 'string',
        'invoiceNumber': 'string',
        'transactionId': 'string',
    },
    'Ticket': {
        'ticketNumber': 'string',
        'status': 'string',
        'customerName': 'string',
        'assignedTo': 'string | null',
        'resolution': 'string | null',
    },
    'Tenant': {
        'status': 'string',
        'slug': 'string',
        'settings': 'Record<string, unknown>',
    },
    'User': {
        'enabled': 'boolean',
        'role': 'string',
    },
    'Segment': {
        'customerCount': 'number',
    },
    'Bill': {
        'billNumber': 'string',
        'status': 'string',
        'customerName': 'string',
        'period': 'string',
        'totalAmount': 'number',
    },
    'NetworkElement': {
        'status': 'string',
        'firmware': 'string',
        'location': 'string | null',
    },
    'Vlan': {
        'status': 'string',
    },
    'ProvisioningJob': {
        'status': 'string',
        'startedAt': 'string | null',
        'completedAt': 'string | null',
        'logs': 'ProvisioningLogEntryDto[] | null',
    },
    'ProvisioningTemplate': {
        'status': 'string',
        'steps': 'ProvisioningTemplateStepDto[]',
        'parameters': 'ProvisioningTemplateParameterDto[] | null',
    },
    'Service': {
        'status': 'string',
        'customerName': 'string',
        'subscriptionName': 'string',
    },
    'WorkflowDefinition': {
        'status': 'string',
        'version': 'number',
        'steps': 'WorkflowStepDto[]',
    },
    'WorkflowInstance': {
        'status': 'string',
        'definitionName': 'string',
        'startedAt': 'string /* date-time */',
        'completedAt': 'string | null',
        'createdBy': 'string',
        'slaStatus': 'string | null',
    },
    'WorkflowSla': {
        'status': 'string',
    },
    'Contact': {
        'id': 'string /* uuid */',
    },
    'Notification': {
        'status': 'string',
        'recipientId': 'string',
        'readAt': 'string | null',
    },
    'NotificationTemplate': {
        'variables': 'string[]',
    },
    'CollectionCase': {
        'status': 'string',
        'customerName': 'string',
        'totalDebt': 'number',
        'assignedTo': 'string | null',
    },
    'Dispute': {
        'status': 'string',
        'invoiceNumber': 'string',
        'resolution': 'string | null',
    },
    'CreditNote': {
        'creditNoteNumber': 'string',
        'status': 'string',
        'invoiceNumber': 'string',
        'customerId': 'string',
    },
    'PaymentArrangement': {
        'status': 'string',
    },
    'SlaDefinition': {
        'status': 'string',
    },
    'SubscriptionEntitlement': {
        'subscriptionId': 'string',
    },
    'UsageRecord': {
        'status': 'string',
    },
    'RatingRule': {
        'status': 'string',
    },
    'Promotion': {
        'status': 'string',
    },
    'AuditEntry': {
        'actorId': 'string',
        'tenantId': 'string',
        'entityType': 'string',
        'entityId': 'string',
        'action': 'string',
        'changes': 'Record<string, { oldValue: unknown; newValue: unknown }>',
        'metadata': 'Record<string, unknown>',
    },
    'AlertRule': {
        'enabled': 'boolean',
    },
    'ApiRoute': {
        'status': 'string',
        'authRequired': 'boolean',
        'rateLimit': 'number',
    },
    'ApiKey': {
        'keyPrefix': 'string',
        'status': 'string',
        'partnerId': 'string',
        'expiresAt': 'string | null',
        'lastUsedAt': 'string | null',
    },
    'Partner': {
        'status': 'string',
        'contactEmail': 'string',
        'apiKeyCount': 'number',
    },
    'ReportDefinition': {
        'type': 'string',
    },
    'ScheduledReport': {
        'cron': 'string',
        'format': 'string',
        'recipients': 'string[]',
        'enabled': 'boolean',
        'definitionName': 'string',
    },
    'DashboardWidget': {
        'title': 'string',
        'type': 'string',
        'position': 'number',
        'size': 'string',
    },
    'DiscoveryJob': {
        'status': 'string',
        'targetRange': 'string',
        'discoveredCount': 'number',
        'startedAt': 'string | null',
        'completedAt': 'string | null',
    },
    'BillingCycle': {
        'status': 'string',
        'customerCount': 'number',
        'totalAmount': 'number',
        'executedAt': 'string | null',
    },
    'TaxRule': {
        'status': 'string',
        'effectiveTo': 'string | null',
    },
}

def json_type_to_ts(json_schema, indent=0):
    """Convert JSON schema type to TypeScript type string."""
    if not json_schema:
        return 'unknown'
    
    type_ = json_schema.get('type', '')
    ref = json_schema.get('$ref', '')
    enum = json_schema.get('enum')
    
    if ref:
        name = ref.split('/')[-1]
        return name
    
    if enum:
        return ' | '.join(f"'{v}'" for v in enum)
    
    if type_ == 'string':
        fmt = json_schema.get('format', '')
        if fmt == 'uuid':
            return 'string /* uuid */'
        if fmt == 'date-time':
            return 'string /* date-time */'
        if fmt == 'date':
            return 'string /* date */'
        return 'string'
    elif type_ == 'integer':
        return 'number'
    elif type_ == 'number':
        return 'number'
    elif type_ == 'boolean':
        return 'boolean'
    elif type_ == 'array':
        items = json_schema.get('items', {})
        return f'{json_type_to_ts(items)}[]'
    elif type_ == 'object':
        props = json_schema.get('properties', {})
        if not props:
            return 'Record<string, unknown>'
        return 'Record<string, string>'
    
    return 'unknown'

def render_ts_type(lines):
    """Render TypeScript interface lines, handling comments and indentation."""
    return '\n'.join(lines)

lines = []
lines.append('// Auto-generated response DTO types')
lines.append('// Generated from OpenAPI command schemas + backend DTO knowledge')
lines.append('')
lines.append('// --- Enums ---')
lines.append('')

# Generate enum types
enum_values = {
    'BillingPeriod': ['Monthly', 'Quarterly', 'SemiAnnual', 'Annual', 'OneTime'],
    'OfferType': ['Subscription', 'OneTime', 'Usage', 'Recurring'],
    'PricingType': ['Flat', 'Tiered', 'Volume', 'Graduated'],
    'RuleType': ['Required', 'Incompatible', 'Recommendation'],
    'OptionType': ['SingleSelect', 'MultiSelect', 'Input', 'Range'],
    'ProductType': ['Physical', 'Digital', 'Service', 'Subscription'],
    'TicketPriority': ['Low', 'Medium', 'High', 'Critical'],
    'SlaLevel': ['Standard', 'Premium', 'Enterprise'],
    'LifecycleStatus': ['Active', 'Inactive', 'Suspended', 'Terminated'],
}

for enum_name, values in enum_values.items():
    lines.append(f'export type {enum_name} = { " | ".join(f"\'{v}\'" for v in values) };')
    lines.append('')

lines.append('')
lines.append('// --- Response DTOs ---')
lines.append('')

# Generate command → DTO mapping
for cmd_name, cmd_schema in sorted(command_types.items()):
    # Extract entity name: CreateCustomerCommand → Customer
    entity_name = cmd_name.replace('Create', '').replace('Command', '')
    dto_name = f'{entity_name}Dto'
    
    # Get extra fields for this DTO
    extras = DTO_EXTRA_FIELDS.get(entity_name, {})
    
    lines.append(f'export interface {dto_name} {{')
    lines.append(f'  /** Format: uuid */')
    lines.append(f'  id: string;')
    
    props = cmd_schema.get('properties', cmd_schema if isinstance(cmd_schema, dict) and 'type' not in cmd_schema else {})
    if isinstance(cmd_schema, dict) and 'properties' in cmd_schema:
        props = cmd_schema['properties']
    
    # Add command fields as response fields
    for prop_name, prop_schema in props.items():
        if prop_name in ('TenantId', 'CreatedById'):
            continue  # Skip tenant/user tracking fields in responses
        reqd = prop_name not in cmd_schema.get('required', [])
        ts_type = json_type_to_ts(prop_schema)
        # If the property was a command field, it might be required in the DTO
        lines.append(f'  {prop_name}: {ts_type};')
    
    # Add extra response-only fields
    for extra_name, extra_type in sorted(extras.items()):
        lines.append(f'  {extra_name}: {extra_type};')
    
    # Standard tracking fields
    lines.append(f'  /** Format: date-time */')
    lines.append(f'  createdAt: string;')
    lines.append(f'  /** Format: date-time */')
    lines.append(f'  updatedAt: string;')
    
    lines.append('}')
    lines.append('')

# Generate additional specialized DTOs not covered by Create commands
lines.append('// --- Specialized DTOs ---')
lines.append('')

extra_dtos = {
    'OrderItemDto': {
        'id': 'string /* uuid */',
        'orderId': 'string /* uuid */',
        'productId': 'string /* uuid */',
        'productName': 'string',
        'quantity': 'number',
        'unitPrice': 'number',
        'totalPrice': 'number',
        'recurringPrice': 'number',
        'billingPeriod': 'string',
    },
    'InvoiceLineItemDto': {
        'description': 'string',
        'quantity': 'number',
        'unitPrice': 'number',
        'totalPrice': 'number',
    },
    'WorkflowStepDto': {
        'id': 'string /* uuid */',
        'name': 'string',
        'type': 'string',
        'config': 'Record<string, unknown>',
        'order': 'number',
        'dependsOn': 'string[]',
    },
    'ProvisioningTemplateStepDto': {
        'id': 'string /* uuid */',
        'name': 'string',
        'order': 'number',
        'type': 'string',
        'config': 'Record<string, unknown>',
    },
    'ProvisioningTemplateParameterDto': {
        'name': 'string',
        'type': 'string',
        'default': 'string',
        'required': 'boolean',
    },
    'ProvisioningLogEntryDto': {
        'id': 'string /* uuid */',
        'step': 'number',
        'message': 'string',
        'level': 'string',
        'result': 'string | null',
        'error': 'string | null',
        'timestamp': 'string /* date-time */',
    },
    'WorkflowTaskDto': {
        'id': 'string /* uuid */',
        'name': 'string',
        'status': 'string',
        'type': 'string',
        'assignedTo': 'string | null',
        'startedAt': 'string | null',
        'completedAt': 'string | null',
    },
}

for name, fields in sorted(extra_dtos.items()):
    lines.append(f'export interface {name} {{')
    for field_name, field_type in fields.items():
        lines.append(f'  {field_name}: {field_type};')
    lines.append('}')
    lines.append('')

output = '\n'.join(lines)
output_path = os.path.join(os.path.dirname(os.path.dirname(__file__)), 'src', 'api', 'generated', 'dto.ts')
with open(output_path, 'w') as f:
    f.write(output)
print(f"Generated {output_path}")
print(f"  - {len(command_types)} command→DTO mappings")
print(f"  - {len(extra_dtos)} specialized DTOs")
print(f"  - {len(enum_values)} enum types")
