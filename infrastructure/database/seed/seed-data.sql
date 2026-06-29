-- =============================================================================
-- OBSS Platform - Seed Data
-- Telecom OSS/BSS Platform
-- =============================================================================
-- This script inserts baseline reference data required for the platform
-- to function. It is idempotent and safe to run multiple times.
-- =============================================================================

BEGIN;

-- ── Tenants ─────────────────────────────────────────────────────────────────
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

-- ── Roles ───────────────────────────────────────────────────────────────────
INSERT INTO roles (id, tenant_id, name, description, is_system, created_at)
VALUES
    (
        'b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a21',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Admin',
        'Full system access with all administrative privileges',
        true,
        NOW()
    ),
    (
        'b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a22',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Manager',
        'Operational management access with supervisory capabilities',
        true,
        NOW()
    ),
    (
        'b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a23',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Agent',
        'Customer service agent with ticketing and CRM access',
        false,
        NOW()
    ),
    (
        'b1aebc99-9c0b-4ef8-bb6d-6bb9bd380a24',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Customer',
        'Self-service portal access for end customers',
        false,
        NOW()
    )
ON CONFLICT (tenant_id, name) DO NOTHING;

-- ── Users ───────────────────────────────────────────────────────────────────
-- Password: admin123 (bcrypt hash)
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
ON CONFLICT (tenant_id, username) DO NOTHING;

-- ── User-Role Assignments ───────────────────────────────────────────────────
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

-- ── Permissions ─────────────────────────────────────────────────────────────
INSERT INTO permissions (id, code, name, description, module, resource, action)
VALUES
    -- IAM Module
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a51', 'iam.users.read',     'Read Users',       'View user details',             'IAM', 'users',     'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a52', 'iam.users.create',   'Create Users',     'Create new users',              'IAM', 'users',     'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a53', 'iam.users.update',   'Update Users',     'Modify existing users',         'IAM', 'users',     'update'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a54', 'iam.users.delete',   'Delete Users',     'Remove users from the system',  'IAM', 'users',     'delete'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a55', 'iam.roles.read',     'Read Roles',       'View role definitions',         'IAM', 'roles',     'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a56', 'iam.roles.manage',   'Manage Roles',     'Create and modify roles',       'IAM', 'roles',     'manage'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a57', 'iam.tenants.read',   'Read Tenants',     'View tenant configurations',    'IAM', 'tenants',   'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a58', 'iam.tenants.manage', 'Manage Tenants',   'Modify tenant settings',        'IAM', 'tenants',   'manage'),

    -- CRM Module
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a61', 'crm.customers.read',     'Read Customers',       'View customer records',              'CRM', 'customers',  'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a62', 'crm.customers.create',   'Create Customers',     'Add new customers',                  'CRM', 'customers',  'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a63', 'crm.customers.update',   'Update Customers',     'Modify customer information',        'CRM', 'customers',  'update'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a64', 'crm.segments.manage',    'Manage Segments',      'Create and manage customer segments','CRM', 'segments',   'manage'),

    -- Billing Module
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a71', 'billing.invoices.read',   'Read Invoices',    'View invoice details',          'Billing',  'invoices',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a72', 'billing.invoices.create', 'Create Invoices',  'Generate new invoices',         'Billing',  'invoices',    'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a73', 'billing.payments.read',   'Read Payments',    'View payment records',          'Billing',  'payments',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a74', 'billing.payments.process','Process Payments',  'Process customer payments',     'Billing',  'payments',    'process'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a75', 'billing.rates.read',      'Read Rates',       'View rate definitions',         'Billing',  'rates',       'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a76', 'billing.rates.manage',    'Manage Rates',     'Create and modify rate plans',  'Billing',  'rates',       'manage'),

    -- Catalog Module
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a81', 'catalog.products.read',   'Read Products',    'View product catalog',          'Catalog',  'products',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a82', 'catalog.products.create', 'Create Products',  'Add new products',              'Catalog',  'products',    'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a83', 'catalog.products.update', 'Update Products',  'Modify product definitions',    'Catalog',  'products',    'update'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a84', 'catalog.offers.read',     'Read Offers',      'View service offers',           'Catalog',  'offers',      'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a85', 'catalog.offers.manage',   'Manage Offers',    'Create and modify offers',      'Catalog',  'offers',      'manage'),

    -- Collections Module
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a91', 'collections.cases.read',  'Read Cases',       'View collection cases',         'Collections', 'cases',     'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380a92', 'collections.cases.manage','Manage Cases',     'Manage collection workflows',   'Collections', 'cases',     'manage'),

    -- Ticketing Module
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380aa1', 'ticketing.tickets.read',   'Read Tickets',     'View support tickets',          'Ticketing', 'tickets',    'read'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380aa2', 'ticketing.tickets.create', 'Create Tickets',   'Open new support tickets',      'Ticketing', 'tickets',    'create'),
    ('e1aebc99-9c0b-4ef8-bb6d-6bb9bd380aa3', 'ticketing.tickets.update', 'Update Tickets',   'Update ticket status and notes','Ticketing', 'tickets',    'update')
ON CONFLICT (code) DO NOTHING;

-- ── Role-Permission Assignments ─────────────────────────────────────────────
-- Admin: all permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Admin'
  AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
ON CONFLICT DO NOTHING;

-- Manager: limited operational permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON TRUE
WHERE r.name = 'Manager'
  AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
  AND p.code IN (
    'iam.users.read', 'iam.roles.read', 'iam.tenants.read',
    'crm.customers.read', 'crm.customers.create', 'crm.customers.update', 'crm.segments.manage',
    'billing.invoices.read', 'billing.invoices.create', 'billing.payments.read',
    'catalog.products.read', 'catalog.offers.read',
    'collections.cases.read', 'collections.cases.manage',
    'ticketing.tickets.read', 'ticketing.tickets.create', 'ticketing.tickets.update'
  )
ON CONFLICT DO NOTHING;

-- Agent: customer-facing permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON TRUE
WHERE r.name = 'Agent'
  AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
  AND p.code IN (
    'iam.users.read',
    'crm.customers.read', 'crm.customers.create', 'crm.customers.update',
    'billing.invoices.read', 'billing.payments.read',
    'catalog.products.read', 'catalog.offers.read',
    'ticketing.tickets.read', 'ticketing.tickets.create', 'ticketing.tickets.update'
  )
ON CONFLICT DO NOTHING;

-- Customer: self-service permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON TRUE
WHERE r.name = 'Customer'
  AND r.tenant_id = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
  AND p.code IN (
    'billing.invoices.read', 'billing.payments.read',
    'catalog.products.read', 'catalog.offers.read',
    'ticketing.tickets.read', 'ticketing.tickets.create'
  )
ON CONFLICT DO NOTHING;

-- ── Customer Segments ───────────────────────────────────────────────────────
INSERT INTO customer_segments (id, tenant_id, name, description, criteria, priority, is_active, created_at, updated_at)
VALUES
    (
        'f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab1',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Premium',
        'High-value customers with premium service agreements',
        '{"ruleGroups": [{"operator": "And", "rules": [{"field": "ContractType", "operator": "Equals", "value": "Premium"}, {"field": "MonthlySpend", "operator": "GreaterThan", "value": "500"}]}]}',
        1,
        true,
        NOW(),
        NOW()
    ),
    (
        'f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab2',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Business',
        'Corporate and business customers',
        '{"ruleGroups": [{"operator": "Or", "rules": [{"field": "CustomerType", "operator": "Equals", "value": "Corporate"}, {"field": "CustomerType", "operator": "Equals", "value": "SME"}]}]}',
        2,
        true,
        NOW(),
        NOW()
    ),
    (
        'f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab3',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Residential',
        'Individual residential customers',
        '{"ruleGroups": [{"operator": "And", "rules": [{"field": "CustomerType", "operator": "Equals", "value": "Individual"}, {"field": "ContractType", "operator": "Equals", "value": "Standard"}]}]}',
        3,
        true,
        NOW(),
        NOW()
    ),
    (
        'f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab4',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'VIP',
        'VIP customers requiring priority support',
        '{"ruleGroups": [{"operator": "And", "rules": [{"field": "AccountAge", "operator": "GreaterThan", "value": "365"}, {"field": "MonthlySpend", "operator": "GreaterThan", "value": "1000"}, {"field": "ChurnRisk", "operator": "Equals", "value": "Low"}]}]}',
        0,
        true,
        NOW(),
        NOW()
    ),
    (
        'f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab5',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'At Risk',
        'Customers showing churn risk indicators',
        '{"ruleGroups": [{"operator": "Or", "rules": [{"field": "ChurnRisk", "operator": "Equals", "value": "High"}, {"field": "OverdueBalance", "operator": "GreaterThan", "value": "0"}, {"field": "TicketFrequency", "operator": "GreaterThan", "value": "5"}]}]}',
        10,
        true,
        NOW(),
        NOW()
    )
ON CONFLICT DO NOTHING;

-- ── Product Categories ──────────────────────────────────────────────────────
INSERT INTO categories (id, tenant_id, name, description, parent_category_id, is_active, sort_order, created_at)
VALUES
    -- Top-level categories
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Mobile Services',
        'Cellular and mobile communication services',
        NULL,
        true,
        1,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac2',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Fixed Line',
        'Landline and fixed telephone services',
        NULL,
        true,
        2,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Broadband Internet',
        'Fixed broadband and fiber internet services',
        NULL,
        true,
        3,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac4',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Digital TV',
        'IPTV and digital television services',
        NULL,
        true,
        4,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac5',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Value-Added Services',
        'VAS including ringtones, content, and premium services',
        NULL,
        true,
        5,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac6',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Devices & Hardware',
        'Mobile devices, routers, modems, and accessories',
        NULL,
        true,
        6,
        NOW()
    ),

    -- Sub-categories: Mobile Services
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad1',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Prepaid',
        'Prepaid mobile plans and top-ups',
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1',
        true,
        1,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad2',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Postpaid',
        'Postpaid mobile subscription plans',
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1',
        true,
        2,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad3',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Data Packs',
        'Mobile data add-on packages',
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1',
        true,
        3,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad4',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'International Roaming',
        'Roaming packages and international call plans',
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1',
        true,
        4,
        NOW()
    ),

    -- Sub-categories: Broadband
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ae1',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Fiber Optic',
        'High-speed fiber optic broadband plans',
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3',
        true,
        1,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ae2',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'DSL',
        'DSL-based broadband internet services',
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3',
        true,
        2,
        NOW()
    ),
    (
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ae3',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'Wireless Broadband',
        'Fixed wireless and 4G/5G home internet',
        'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3',
        true,
        3,
        NOW()
    )
ON CONFLICT DO NOTHING;

-- ── Currencies ──────────────────────────────────────────────────────────────
-- Reference table for supported currencies
CREATE TABLE IF NOT EXISTS currencies (
    code        VARCHAR(3)  PRIMARY KEY,
    name        VARCHAR(100) NOT NULL,
    symbol      VARCHAR(10)  NOT NULL,
    is_active   BOOLEAN      NOT NULL DEFAULT true,
    is_default  BOOLEAN      NOT NULL DEFAULT false,
    sort_order  INTEGER      NOT NULL DEFAULT 0
);

INSERT INTO currencies (code, name, symbol, is_active, is_default, sort_order)
VALUES
    ('YER', 'Yemeni Rial',    '﷼',  true, true,  1),
    ('USD', 'US Dollar',      '$',  true, false, 2),
    ('SAR', 'Saudi Riyal',    '﷼',  true, false, 3)
ON CONFLICT (code) DO NOTHING;

COMMIT;
