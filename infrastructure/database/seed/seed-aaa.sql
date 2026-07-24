-- =============================================================================
-- OBSS Platform - AAA Seed Data
-- NAS devices and reference data for the AAA module
-- Database: obss_aaa
-- =============================================================================
BEGIN;

-- ── NAS Devices (BRAS/BNG/WLC for Yemen PTC) ─────────────────────────────────
INSERT INTO nas_devices (id, tenant_id, name, nas_ip_address, nas_secret, nas_type, location, status, created_at, updated_at)
VALUES
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a01',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'BRAS-Sanaa-01',
        '10.10.10.1',
        'bras-sanaa-secret-2024',
        'BRAS',
        'Sanaa Data Center',
        'Active',
        NOW(),
        NOW()
    ),
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a02',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'BRAS-Aden-01',
        '10.10.20.1',
        'bras-aden-secret-2024',
        'BRAS',
        'Aden Data Center',
        'Active',
        NOW(),
        NOW()
    ),
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a03',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'BNG-Taiz-01',
        '10.10.30.1',
        'bng-taiz-secret-2024',
        'BNG',
        'Taiz Central Office',
        'Active',
        NOW(),
        NOW()
    ),
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a04',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'BNG-Hodeidah-01',
        '10.10.40.1',
        'bng-hodeidah-secret-2024',
        'BNG',
        'Hodeidah Central Office',
        'Active',
        NOW(),
        NOW()
    ),
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a05',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'WLC-Sanaa-Mall',
        '10.20.10.1',
        'wlc-sanaa-secret-2024',
        'WLC',
        'Sanaa Commercial District',
        'Active',
        NOW(),
        NOW()
    ),
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a06',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'WLC-Aden-Port',
        '10.20.20.1',
        'wlc-aden-secret-2024',
        'WLC',
        'Aden Port Area',
        'Active',
        NOW(),
        NOW()
    ),
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a07',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'VSAT-Rural-North',
        '10.30.10.1',
        'vsat-north-secret-2024',
        'VSAT',
        'Northern Rural Zone',
        'Active',
        NOW(),
        NOW()
    ),
    (
        'd1aebc99-9c0b-4ef8-bb6d-6bb9bd380a08',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        'UAG-Corporate-Portal',
        '10.40.10.1',
        'uag-corp-secret-2024',
        'UAG',
        'Corporate VPN Gateway',
        'Active',
        NOW(),
        NOW()
    )
ON CONFLICT (tenant_id, name) DO NOTHING;

COMMIT;
