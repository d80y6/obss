-- Yemen PTC CRM - obss_crm seed
BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'customer_segments' AND column_name = 'name_ar') THEN
        ALTER TABLE customer_segments ADD COLUMN name_ar character varying(200);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'customer_segments' AND column_name = 'description_ar') THEN
        ALTER TABLE customer_segments ADD COLUMN description_ar character varying(1000);
    END IF;
END $$;

INSERT INTO customer_segments (id, tenant_id, name, name_ar, description, description_ar, criteria, priority, is_active, created_at, updated_at)
VALUES
(
    'a0000000-0000-4000-a000-000000000010',
    'a0000000-0000-4000-a000-000000000001',
    'Residential Yemen',
    'سكني اليمن',
    'Individual residential customers in Yemen',
    'العملاء الأفراد السكنيون في اليمن',
    '{"ruleGroups": [{"operator": "And", "rules": [{"field": "CustomerType", "operator": "Equals", "value": "Individual"}, {"field": "TenantId", "operator": "Equals", "value": "yemen-ptc"}]}]}',
    1,
    true,
    NOW(),
    NOW()
),
(
    'a0000000-0000-4000-a000-000000000011',
    'a0000000-0000-4000-a000-000000000001',
    'Business Yemen',
    'أعمال اليمن',
    'Corporate and business customers in Yemen',
    'العملاء من الشركات والمؤسسات في اليمن',
    '{"ruleGroups": [{"operator": "Or", "rules": [{"field": "CustomerType", "operator": "Equals", "value": "Corporate"}, {"field": "CustomerType", "operator": "Equals", "value": "SME"}, {"field": "TenantId", "operator": "Equals", "value": "yemen-ptc"}]}]}',
    2,
    true,
    NOW(),
    NOW()
)
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 2: Service Categories
-- =========================================================================


COMMIT;
