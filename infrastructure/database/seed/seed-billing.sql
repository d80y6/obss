-- =============================================================================
-- OBSS Platform - Billing Seed Data (Reference Currencies)
-- =============================================================================
BEGIN;

CREATE TABLE IF NOT EXISTS currencies (
    code       VARCHAR(3)  PRIMARY KEY,
    name       VARCHAR(100) NOT NULL,
    symbol     VARCHAR(10)  NOT NULL,
    is_active  BOOLEAN      NOT NULL DEFAULT true,
    is_default BOOLEAN      NOT NULL DEFAULT false,
    sort_order INTEGER      NOT NULL DEFAULT 0
);

INSERT INTO currencies (code, name, symbol, is_active, is_default, sort_order)
VALUES
    ('YER', 'Yemeni Rial', 'YER', true, true,  1),
    ('USD', 'US Dollar',   'USD', true, false, 2),
    ('SAR', 'Saudi Riyal', 'SAR', true, false, 3)
ON CONFLICT (code) DO NOTHING;

COMMIT;
