-- =============================================================================
-- OBSS Platform - IAM Seed Data
-- =============================================================================
BEGIN;

INSERT INTO tenants (id, name, slug, is_active, settings, created_at, updated_at)
VALUES
    (
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Default Tenant',
        'default',
        true,
        '{"timezone": "Asia/Aden", "locale": "en-YE", "date_format": "YYYY-MM-DD", "currency": "YER"}',
        NOW(),
        NOW()
    )
ON CONFLICT (slug) DO NOTHING;

INSERT INTO roles (id, tenant_id, name, description, is_system, created_at)
VALUES
    ('b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a21', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Admin',    'Full system access with all administrative privileges', true, NOW()),
    ('b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a22', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Manager',  'Operational management access with supervisory capabilities', true, NOW()),
    ('b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a23', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Agent',    'Customer service agent with ticketing and CRM access', false, NOW()),
    ('b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a24', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Customer', 'Self-service portal access for end customers', false, NOW())
ON CONFLICT (tenant_id, name) DO NOTHING;

INSERT INTO users (id, tenant_id, username, email, first_name, last_name, is_active, email_verified, created_at, updated_at)
VALUES
    (
        'c1aebc99-9c0b-4ef8-bb6d-6bb9bd380a31',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'admin',
        'admin@obss.local',
        'System',
        'Administrator',
        true,
        true,
        NOW(),
        NOW()
    )
ON CONFLICT (id) DO NOTHING;

INSERT INTO user_roles (id, user_id, role_id, assigned_at, assigned_by)
VALUES
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a41',
        'c1aebc99-9c0b-4ef8-bb6d-6bb9bd380a31',
        'b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a21',
        NOW(),
        'c1aebc99-9c0b-4ef8-bb6d-6bb9bd380a31'
    )
ON CONFLICT (user_id, role_id) DO NOTHING;

INSERT INTO permissions (id, code, name, description, module, resource, action)
VALUES
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a51', 'iam.users.read',     'Read Users',       'View user details',             'IAM', 'users',     'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a52', 'iam.users.create',   'Create Users',     'Create new users',              'IAM', 'users',     'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a53', 'iam.users.update',   'Update Users',     'Modify existing users',         'IAM', 'users',     'update'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a54', 'iam.users.delete',   'Delete Users',     'Remove users from the system',  'IAM', 'users',     'delete'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a55', 'iam.roles.read',     'Read Roles',       'View role definitions',         'IAM', 'roles',     'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a56', 'iam.roles.manage',   'Manage Roles',     'Create and modify roles',       'IAM', 'roles',     'manage'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a57', 'iam.tenants.read',   'Read Tenants',     'View tenant configurations',    'IAM', 'tenants',   'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a58', 'iam.tenants.manage', 'Manage Tenants',   'Modify tenant settings',        'IAM', 'tenants',   'manage'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a61', 'crm.customers.read',     'Read Customers',       'View customer records',              'CRM', 'customers',  'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a62', 'crm.customers.create',   'Create Customers',     'Add new customers',                  'CRM', 'customers',  'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a63', 'crm.customers.update',   'Update Customers',     'Modify customer information',        'CRM', 'customers',  'update'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a64', 'crm.segments.manage',    'Manage Segments',      'Create and manage customer segments','CRM', 'segments',   'manage'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a71', 'billing.invoices.read',   'Read Invoices',    'View invoice details',          'Billing',  'invoices',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a72', 'billing.invoices.create', 'Create Invoices',  'Generate new invoices',         'Billing',  'invoices',    'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a73', 'billing.payments.read',   'Read Payments',    'View payment records',          'Billing',  'payments',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a74', 'billing.payments.process','Process Payments',  'Process customer payments',     'Billing',  'payments',    'process'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a75', 'billing.rates.read',      'Read Rates',       'View rate definitions',         'Billing',  'rates',       'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a76', 'billing.rates.manage',    'Manage Rates',     'Create and modify rate plans',  'Billing',  'rates',       'manage'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a81', 'catalog.products.read',   'Read Products',    'View product catalog',          'Catalog',  'products',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a82', 'catalog.products.create', 'Create Products',  'Add new products',              'Catalog',  'products',    'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a83', 'catalog.products.update', 'Update Products',  'Modify product definitions',    'Catalog',  'products',    'update'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a84', 'catalog.offers.read',     'Read Offers',      'View service offers',           'Catalog',  'offers',      'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a85', 'catalog.offers.manage',   'Manage Offers',    'Create and modify offers',      'Catalog',  'offers',      'manage'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a91', 'collections.cases.read',  'Read Cases',       'View collection cases',         'Collections', 'cases',     'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a92', 'collections.cases.manage','Manage Cases',     'Manage collection workflows',   'Collections', 'cases',     'manage'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380aa1', 'ticketing.tickets.read',   'Read Tickets',     'View support tickets',          'Ticketing', 'tickets',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380aa2', 'ticketing.tickets.create', 'Create Tickets',   'Open new support tickets',      'Ticketing', 'tickets',    'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380aa3', 'ticketing.tickets.update', 'Update Tickets',   'Update ticket status and notes','Ticketing', 'tickets',    'update')
ON CONFLICT (code) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r CROSS JOIN permissions p WHERE r.name = 'Admin' AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p ON TRUE
WHERE r.name = 'Manager' AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
  AND p.code IN ('iam.users.read','iam.roles.read','iam.tenants.read','crm.customers.read','crm.customers.create','crm.customers.update','crm.segments.manage','billing.invoices.read','billing.invoices.create','billing.payments.read','catalog.products.read','catalog.offers.read','collections.cases.read','collections.cases.manage','ticketing.tickets.read','ticketing.tickets.create','ticketing.tickets.update')
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p ON TRUE
WHERE r.name = 'Agent' AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
  AND p.code IN ('iam.users.read','crm.customers.read','crm.customers.create','crm.customers.update','billing.invoices.read','billing.payments.read','catalog.products.read','catalog.offers.read','ticketing.tickets.read','ticketing.tickets.create','ticketing.tickets.update')
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p ON TRUE
WHERE r.name = 'Customer' AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
  AND p.code IN ('billing.invoices.read','billing.payments.read','catalog.products.read','catalog.offers.read','ticketing.tickets.read','ticketing.tickets.create')
ON CONFLICT DO NOTHING;

COMMIT;
