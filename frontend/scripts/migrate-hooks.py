#!/usr/bin/env python3
"""
Migrate all API hooks from using @/types/api to @/api/generated.
"""
import os
import re

HOOK_DIR = os.path.join(os.path.dirname(os.path.dirname(__file__)), 'src', 'api', 'hooks')

# Mapping from old type imports to new ones
# (old_type_name, new_type_name, new_source)
TYPE_MAP = {
    # Response DTOs
    'CustomerDto': ('CustomerDto', 'dto'),
    'OrderDto': ('OrderDto', 'dto'),
    'SubscriptionDto': ('SubscriptionDto', 'dto'),
    'OfferDto': ('OfferDto', 'dto'),
    'ProductDto': ('ProductDto', 'dto'),
    'InvoiceDto': ('InvoiceDto', 'dto'),
    'PaymentDto': ('PaymentDto', 'dto'),
    'BillDto': ('BillDto', 'dto'),
    'TicketDto': ('TicketDto', 'dto'),
    'IamUserDto': ('IamUserDto', 'dto'),
    'RoleDto': ('RoleDto', 'dto'),
    'TenantDto': ('TenantDto', 'dto'),
    'SegmentDto': ('SegmentDto', 'dto'),
    'WorkflowDefinitionDto': ('WorkflowDefinitionDto', 'dto'),
    'WorkflowInstanceDto': ('WorkflowInstanceDto', 'dto'),
    'ProvisioningJobDto': ('ProvisioningJobDto', 'dto'),
    'ProvisioningTemplateDto': ('ProvisioningTemplateDto', 'dto'),
    'ProvisioningLogEntryDto': ('ProvisioningLogEntryDto', 'dto'),
    'NotificationDto': ('NotificationDto', 'dto'),
    'NotificationTemplateDto': ('NotificationTemplateDto', 'dto'),
    'ReportDefinitionDto': ('ReportDefinitionDto', 'dto'),
    'ReportExecutionDto': ('ReportExecutionDto', 'dto'),
    'CollectionCaseDto': ('CollectionCaseDto', 'dto'),
    'CollectionActionDto': ('CollectionActionDto', 'dto'),
    'PaymentArrangementDto': ('PaymentArrangementDto', 'dto'),
    'ApiRouteDto': ('ApiRouteDto', 'dto'),
    'ApiKeyDto': ('ApiKeyDto', 'dto'),
    'PartnerDto': ('PartnerDto', 'dto'),
    'ServiceInventoryDto': ('ServiceDto', 'dto'),
    'ServiceTopologyDto': ('ServiceTopologyDto', 'dto'),
    'ResourceDto': ('ResourceDto', 'dto'),
    'DiscoveryJobDto': ('DiscoveryJobDto', 'dto'),
    'AuditEntryDto': ('AuditEntryDto', 'dto'),
    'AuditAlertDto': ('AuditAlertDto', 'dto'),
    'NetworkElementDto': ('NetworkElementDto', 'dto'),
    'SubnetDto': ('SubnetDto', 'dto'),
    'VlanDto': ('VlanDto', 'dto'),
    
    # Request types (commands)
    'CustomerCreateRequest': ('CreateCustomerCommand', 'cmd'),
    'ProductCreateRequest': ('CreateProductCommand', 'cmd'),
    'OrderCreateRequest': ('CreateOrderCommand', 'cmd'),
    'ProvisioningJobCreateRequest': ('CreateProvisioningJobCommand', 'cmd'),
    'ProvisioningTemplateCreateRequest': ('CreateProvisioningTemplateCommand', 'cmd'),
    'ServiceCreateRequest': ('CreateServiceCommand', 'cmd'),
}

def migrate_file(filepath):
    with open(filepath) as f:
        content = f.read()
    
    original = content
    
    # Find the import line from @/types/api
    import_match = re.search(r'import\s*\{([^}]+)\}\s*from\s*["\']@/types/api["\']', content)
    if not import_match:
        return False, "No @/types/api import found"
    
    old_types = [t.strip() for t in import_match.group(1).split(',')]
    
    # Map old types to new
    cmd_imports = []
    dto_imports = []
    unmapped = []
    
    for t in old_types:
        if t in TYPE_MAP:
            new_name, source = TYPE_MAP[t]
            if source == 'cmd':
                cmd_imports.append(new_name)
            else:
                dto_imports.append(new_name)
        else:
            unmapped.append(t)
    
    # Build new import lines
    new_imports = []
    
    if dto_imports:
        new_imports.append(f"import type {{ {', '.join(sorted(set(dto_imports)))} }} from '@/api/generated/dto'")
    if cmd_imports:
        new_imports.append(f"import type {{ {', '.join(sorted(set(cmd_imports)))} }} from '@/api/generated'")
    
    if unmapped:
        # Some types might need to come from the old location still
        # Or they could be from @/types/api that we haven't mapped yet
        new_imports.append(f"import type {{ {', '.join(unmapped)} }} from '@/types/api'")
    
    # Replace the import line
    new_content = re.sub(
        r'import\s*\{[^}]+\}\s*from\s*["\']@/types/api["\']',
        '\n'.join(new_imports),
        content
    )
    
    if new_content != original:
        with open(filepath, 'w') as f:
            f.write(new_content)
        return True, f"Replaced: {import_match.group(0).strip()} -> {len(cmd_imports)} cmd + {len(dto_imports)} dto"
    else:
        return False, "No change"

# Process all hook files
results = []
for fname in sorted(os.listdir(HOOK_DIR)):
    if fname.endswith('.ts'):
        fpath = os.path.join(HOOK_DIR, fname)
        success, msg = migrate_file(fpath)
        results.append((fname, success, msg))
        if success:
            print(f"✓ {fname}: {msg}")
        elif msg != "No @/types/api import found":
            print(f"⚠ {fname}: {msg}")

print(f"\nProcessed {len(results)} files")
print(f"Updated: {sum(1 for _, s, _ in results if s)}")
print(f"Skipped: {sum(1 for _, s, m in results if m == 'No @/types/api import found')}")
