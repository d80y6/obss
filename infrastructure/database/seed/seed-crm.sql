-- =============================================================================
-- OBSS Platform - CRM Seed Data
-- =============================================================================
BEGIN;

INSERT INTO customer_segments (id, tenant_id, name, description, criteria, priority, is_active, created_at, updated_at)
VALUES
    ('f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab1', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Premium',     'High-value customers with premium service agreements',     '{"ruleGroups": [{"operator": "And", "rules": [{"field": "ContractType", "operator": "Equals", "value": "Premium"}, {"field": "MonthlySpend", "operator": "GreaterThan", "value": "500"}]}]}', 1, true, NOW(), NOW()),
    ('f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab2', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Business',    'Corporate and business customers',                         '{"ruleGroups": [{"operator": "Or", "rules": [{"field": "CustomerType", "operator": "Equals", "value": "Corporate"}, {"field": "CustomerType", "operator": "Equals", "value": "SME"}]}]}', 2, true, NOW(), NOW()),
    ('f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab3', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Residential', 'Individual residential customers',                          '{"ruleGroups": [{"operator": "And", "rules": [{"field": "CustomerType", "operator": "Equals", "value": "Individual"}, {"field": "ContractType", "operator": "Equals", "value": "Standard"}]}]}', 3, true, NOW(), NOW()),
    ('f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab4', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'VIP',         'VIP customers requiring priority support',                  '{"ruleGroups": [{"operator": "And", "rules": [{"field": "AccountAge", "operator": "GreaterThan", "value": "365"}, {"field": "MonthlySpend", "operator": "GreaterThan", "value": "1000"}, {"field": "ChurnRisk", "operator": "Equals", "value": "Low"}]}]}', 0, true, NOW(), NOW()),
    ('f1aebc99-9c0b-4ef8-bb6d-6bb9bd380ab5', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'At Risk',     'Customers showing churn risk indicators',                   '{"ruleGroups": [{"operator": "Or", "rules": [{"field": "ChurnRisk", "operator": "Equals", "value": "High"}, {"field": "OverdueBalance", "operator": "GreaterThan", "value": "0"}, {"field": "TicketFrequency", "operator": "GreaterThan", "value": "5"}]}]}', 10, true, NOW(), NOW())
ON CONFLICT DO NOTHING;

COMMIT;
