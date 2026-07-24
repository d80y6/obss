-- =============================================================================
-- OBSS Platform - Catalog Seed Data
-- =============================================================================
BEGIN;

INSERT INTO categories (id, tenant_id, name, description, parent_category_id, is_active, lifecycle_status, sort_order, version, created_at, updated_at)
VALUES
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Mobile Services',       'Cellular and mobile communication services', NULL, true, 'Active', 1, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac2', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Fixed Line',            'Landline and fixed telephone services',      NULL, true, 'Active', 2, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Broadband Internet',    'Fixed broadband and fiber internet services',NULL, true, 'Active', 3, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac4', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Digital TV',            'IPTV and digital television services',       NULL, true, 'Active', 4, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac5', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Value-Added Services',  'VAS including ringtones, content, and premium services', NULL, true, 'Active', 5, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac6', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Devices & Hardware',    'Mobile devices, routers, modems, and accessories', NULL, true, 'Active', 6, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad1', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Prepaid',               'Prepaid mobile plans and top-ups',           'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1', true, 'Active', 1, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad2', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Postpaid',              'Postpaid mobile subscription plans',         'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1', true, 'Active', 2, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad3', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Data Packs',            'Mobile data add-on packages',                'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1', true, 'Active', 3, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ad4', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'International Roaming', 'Roaming packages and international call plans','f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac1', true, 'Active', 4, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ae1', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Fiber Optic',           'High-speed fiber optic broadband plans',     'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3', true, 'Active', 1, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ae2', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'DSL',                   'DSL-based broadband internet services',      'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3', true, 'Active', 2, 1, NOW(), NOW()),
    ('f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ae3', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Wireless Broadband',    'Fixed wireless and 4G/5G home internet',     'f2aebc99-9c0b-4ef8-bb6d-6bb9bd380ac3', true, 'Active', 3, 1, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

COMMIT;
