-- Yemen PTC IAM tenant - obss_iam seed
BEGIN;

INSERT INTO tenants (id, name, slug, is_active, settings, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000001',
    'Yemen PTC',
    'yemen-ptc',
    true,
    '{"timezone": "Asia/Aden", "locale": "ar-YE", "date_format": "YYYY-MM-DD", "currency": "YER"}',
    NOW(),
    NOW()
)
ON CONFLICT (slug) DO NOTHING;

COMMIT;
