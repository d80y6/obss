-- scripts/migrations/subscriptions-to-products.sql
-- Creates Product records from all existing Subscription records
-- Each Subscription becomes one Product with Created status

INSERT INTO products (id, tenant_id, customer_id, name, description, status, created_at, updated_at)
SELECT
    gen_random_uuid(),
    s.tenant_id,
    s.customer_id,
    s.offer_name,
    'Migrated from subscription ' || s.id,
    CASE
        WHEN s.status = 'Active' THEN 'Active'
        WHEN s.status = 'Suspended' THEN 'Suspended'
        WHEN s.status = 'Cancelled' THEN 'Cancelled'
        WHEN s.status = 'Expired' THEN 'Terminated'
        ELSE 'Created'
    END,
    s.created_at,
    NOW()
FROM subscriptions s
WHERE NOT EXISTS (
    SELECT 1 FROM products p WHERE p.customer_id = s.customer_id AND p.name = s.offer_name
);
