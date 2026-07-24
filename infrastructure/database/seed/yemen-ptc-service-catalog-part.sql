-- Yemen PTC Service Catalog - obss_service_catalog seed
BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'service_categories' AND column_name = 'name_ar') THEN
        ALTER TABLE service_categories ADD COLUMN name_ar character varying(200);
        ALTER TABLE service_categories ADD COLUMN description_ar character varying(2000);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'service_specifications' AND column_name = 'name_ar') THEN
        ALTER TABLE service_specifications ADD COLUMN name_ar character varying(200);
        ALTER TABLE service_specifications ADD COLUMN description_ar character varying(2000);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'service_spec_characteristics' AND column_name = 'name_ar') THEN
        ALTER TABLE service_spec_characteristics ADD COLUMN name_ar character varying(200);
        ALTER TABLE service_spec_characteristics ADD COLUMN description_ar character varying(2000);
    END IF;
END $$;

INSERT INTO service_categories (id, tenant_id, name, name_ar, description, description_ar, parent_category_id, lifecycle_status, version, created_at, updated_at)
VALUES
(
    'a0000000-0000-4000-a000-000000000020',
    'a0000000-0000-4000-a000-000000000001',
    'Residential Services',
    'الخدمات السكنية',
    'Residential telecom services for individual customers',
    'خدمات الاتصالات السكنية للعملاء الأفراد',
    NULL, 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000030',
    'a0000000-0000-4000-a000-000000000001',
    'Business Services',
    'خدمات الأعمال',
    'Enterprise and business telecom services',
    'خدمات الاتصالات للمؤسسات والشركات',
    NULL, 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000021',
    'a0000000-0000-4000-a000-000000000001',
    'Residential Broadband',
    'النطاق العريض السكني',
    'Fixed broadband internet services for homes',
    'خدمات الإنترنت الثابت عريض النطاق للمنازل',
    'a0000000-0000-4000-a000-000000000020', 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000022',
    'a0000000-0000-4000-a000-000000000001',
    'Residential Voice',
    'الخدمات الصوتية السكنية',
    'Fixed and mobile voice telephony services',
    'خدمات الهاتف الثابت والمتحرك',
    'a0000000-0000-4000-a000-000000000020', 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000023',
    'a0000000-0000-4000-a000-000000000001',
    'Residential Mobile',
    'الخدمات المتنقلة السكنية',
    'Mobile and wireless data services',
    'خدمات البيانات المتنقلة واللاسلكية',
    'a0000000-0000-4000-a000-000000000020', 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000024',
    'a0000000-0000-4000-a000-000000000001',
    'Residential Bundles',
    'الباقات السكنية',
    'Bundled residential service packages',
    'باقات الخدمات السكنية المجمعة',
    'a0000000-0000-4000-a000-000000000020', 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000031',
    'a0000000-0000-4000-a000-000000000001',
    'Business Connectivity',
    'خدمات الربط للأعمال',
    'Dedicated connectivity and data services for businesses',
    'خدمات الربط المخصصة والبيانات للشركات',
    'a0000000-0000-4000-a000-000000000030', 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000032',
    'a0000000-0000-4000-a000-000000000001',
    'Business Voice',
    'الخدمات الصوتية للأعمال',
    'Voice and unified communications for enterprises',
    'خدمات الصوت والاتصالات الموحدة للشركات',
    'a0000000-0000-4000-a000-000000000030', 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000033',
    'a0000000-0000-4000-a000-000000000001',
    'Cloud & Hosting',
    'السحابة والاستضافة',
    'Cloud, hosting and infrastructure services',
    'خدمات السحابة والاستضافة والبنية التحتية',
    'a0000000-0000-4000-a000-000000000030', 'Active', 1, NOW(), NOW()
),
(
    'a0000000-0000-4000-a000-000000000034',
    'a0000000-0000-4000-a000-000000000001',
    'Value-Added Business',
    'خدمات القيمة المضافة للأعمال',
    'Value-added and specialized business services',
    'الخدمات المتخصصة وذات القيمة المضافة للشركات',
    'a0000000-0000-4000-a000-000000000030', 'Active', 1, NOW(), NOW()
)
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 3a: Service Specifications - Residential Services (1-9)
-- =========================================================================

-- 1. Super Shamel (Bundle)

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000100',
    'a0000000-0000-4000-a000-000000000001',
    'Super Shamel',
    'سوبر شامل',
    'All-in-one bundle: FTTH internet + VoIP telephony + Mobile data',
    'باقة شاملة: إنترنت FTTH + هاتف VoIP + بيانات متنقلة',
    'PTC Yemen', '1.0', 'Active', true, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 2. FTTH Residential

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000101',
    'a0000000-0000-4000-a000-000000000001',
    'FTTH Residential',
    'الألياف الضوئية للمنازل',
    'Fiber-to-the-home broadband internet for residential customers',
    'إنترنت عبر الألياف الضوئية للمنازل للعملاء السكنيين',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 3. Hatif Tawasol

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000102',
    'a0000000-0000-4000-a000-000000000001',
    'Hatif Tawasol',
    'هاتفي تواصل',
    'VoIP telephone service delivered over FTTH or ADSL broadband',
    'خدمة الهاتف عبر VoIP عبر الألياف الضوئية أو ADSL',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 4. Supernet ADSL

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000103',
    'a0000000-0000-4000-a000-000000000001',
    'Supernet ADSL',
    'سوبرنت ADSL',
    'ADSL broadband internet for residential customers',
    'إنترنت ADSL عريض النطاق للعملاء السكنيين',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 5. Yemen 4G

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000104',
    'a0000000-0000-4000-a000-000000000001',
    'Yemen 4G',
    'يمن فور جي',
    'Mobile 4G/LTE data service for mobile broadband access',
    'خدمة بيانات الجيل الرابع LTE للنطاق العريض المتنقل',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 6. Home Wireless 4G

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000105',
    'a0000000-0000-4000-a000-000000000001',
    'Home Wireless 4G',
    'اللاسلكي المنزلي 4G',
    'Fixed wireless broadband using 4G LTE with indoor CPE',
    'إنترنت لاسلكي ثابت عبر الجيل الرابع LTE مع جهاز CPE داخلي',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 7. Supplementary Telephone Services

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000106',
    'a0000000-0000-4000-a000-000000000001',
    'Supplementary Telephone Services',
    'الخدمات الهاتفية الإضافية',
    'Additional telephony features including call forwarding, caller ID, voicemail',
    'خدمات هاتفية إضافية تشمل تحويل المكالمات، معرف المتصل، البريد الصوتي',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 8. Hatif Fawtara (Fixed Postpaid)

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000107',
    'a0000000-0000-4000-a000-000000000001',
    'Hatif Fawtara',
    'هاتفي فوترة',
    'Postpaid fixed telephone line with monthly billing',
    'خط هاتف ثابت بنظام الفوترة الشهرية',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 9. Yemen WiFi

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000108',
    'a0000000-0000-4000-a000-000000000001',
    'Yemen WiFi',
    'يمن واي فاي',
    'Public WiFi hotspot service across Yemen',
    'خدمة واي فاي عامة في جميع أنحاء اليمن',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 3b: Service Specifications - Business Services (10-23)
-- =========================================================================

-- 10. Business FTTH

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000110',
    'a0000000-0000-4000-a000-000000000001',
    'Business FTTH',
    'الألياف الضوئية للأعمال',
    'Fiber-to-the-home broadband with SLA for business customers',
    'إنترنت عبر الألياف الضوئية مع اتفاقية مستوى الخدمة للشركات',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 11. DIA

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000111',
    'a0000000-0000-4000-a000-000000000001',
    'Dedicated Internet Access',
    'الإنترنت المخصص',
    'Symmetric dedicated internet access with SLA',
    'وصول مخصص ومتماثل للإنترنت مع SLA',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 12. Ethernet Connectivity

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000112',
    'a0000000-0000-4000-a000-000000000001',
    'Ethernet Connectivity',
    'الاتصال الإيثرنت',
    'Point-to-point and point-to-multipoint Ethernet connectivity',
    'اتصال إيثرنت من نقطة إلى نقطة ومن نقطة إلى عدة نقاط',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 13. TDM Circuits

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000113',
    'a0000000-0000-4000-a000-000000000001',
    'TDM Circuits',
    'دوائر TDM',
    'Time-division multiplexing circuits for legacy voice and data',
    'دوائر تعدد الإرسال بتقسيم الزمن للصوت والبيانات التقليدية',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 14. PRI Voice Channels

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000114',
    'a0000000-0000-4000-a000-000000000001',
    'PRI Voice Channels',
    'قنوات PRI الصوتية',
    'E1 PRI ISDN trunks for business voice connectivity',
    'خطوط E1 PRI للاتصالات الصوتية للشركات',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 15. 800 Free Number

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000115',
    'a0000000-0000-4000-a000-000000000001',
    '800 Free Number',
    'الرقم المجاني 800',
    'Toll-free 800 number service for businesses',
    'خدمة الرقم المجاني 800 للشركات',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 16. Static IP

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000116',
    'a0000000-0000-4000-a000-000000000001',
    'Static IP Address',
    'عنوان IP ثابت',
    'Dedicated static public IP address assignment',
    'تخصيص عنوان IP عام ثابت مخصص',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 17. Wireless Transmission over 4G

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000117',
    'a0000000-0000-4000-a000-000000000001',
    'Wireless Transmission over 4G',
    'النقل اللاسلكي عبر 4G',
    'Wireless data transmission service over 4G LTE for business',
    'خدمة نقل بيانات لاسلكية عبر الجيل الرابع LTE للشركات',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 18. Dedicated Server

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000118',
    'a0000000-0000-4000-a000-000000000001',
    'Dedicated Server',
    'خادم مخصص',
    'Physical dedicated server hosting in PTC data center',
    'استضافة خادم فعلي مخصص في مركز بيانات PTC',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 19. VPS

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000119',
    'a0000000-0000-4000-a000-000000000001',
    'Virtual Private Server',
    'خادم افتراضي خاص',
    'Virtual private server hosting with guaranteed resources',
    'استضافة خادم افتراضي خاص بموارد مضمونة',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 20. Colocation

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000120',
    'a0000000-0000-4000-a000-000000000001',
    'Colocation',
    'المساحة المشتركة',
    'Data center colocation space in PTC facilities',
    'مساحة مشاركة في مركز بيانات PTC',
    'Huawei', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 21. Web Hosting

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000121',
    'a0000000-0000-4000-a000-000000000001',
    'Web Hosting',
    'استضافة مواقع',
    'Shared and business web hosting services',
    'خدمات استضافة المواقع المشتركة والتجارية',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 22. Domain Name Registration

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000122',
    'a0000000-0000-4000-a000-000000000001',
    'Domain Name Registration',
    'تسجيل النطاقات',
    'Domain name registration and renewal services',
    'خدمات تسجيل وتجديد أسماء النطاقات',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- 23. ATM Connectivity

INSERT INTO service_specifications (id, tenant_id, name, name_ar, description, description_ar, brand, version, lifecycle_status, is_bundle, created_at, updated_at)
VALUES (
    'a0000000-0000-4000-a000-000000000123',
    'a0000000-0000-4000-a000-000000000001',
    'ATM Connectivity',
    'خدمة أجهزة الصراف الآلي',
    'Dedicated connectivity for ATM machines',
    'اتصال مخصص لأجهزة الصراف الآلي',
    'PTC Yemen', '1.0', 'Active', false, NOW(), NOW()
) ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 4: Service Spec Characteristics & Values
-- =========================================================================

-- Super Shamel Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000201', 'a0000000-0000-4000-a000-000000000100', 'Internet Speed', 'سرعة الإنترنت', 'Maximum FTTH internet speed in Mbps', 'سرعة الإنترنت القصوى عبر FTTH بالميغابت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000202', 'a0000000-0000-4000-a000-000000000100', 'Voice Minutes', 'دقائق الصوت', 'Included VoIP minutes', 'دقائق الصوت عبر VoIP المضمنة', 'number', false, true, 2),
('a0000000-0000-4000-a000-000000000203', 'a0000000-0000-4000-a000-000000000100', 'Mobile Data', 'البيانات المتنقلة', 'Included 4G mobile data in GB', 'البيانات المتنقلة عبر الجيل الرابع المضمنة بالجيجابايت', 'number', false, true, 3) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, unit_of_measure, is_default)
VALUES
('a0000000-0000-4000-a000-000000000211', 'a0000000-0000-4000-a000-000000000201', '100', 'Mbps', true),
('a0000000-0000-4000-a000-000000000212', 'a0000000-0000-4000-a000-000000000202', '500', 'minutes', true),
('a0000000-0000-4000-a000-000000000213', 'a0000000-0000-4000-a000-000000000203', '10', 'GB', true)
ON CONFLICT DO NOTHING;

-- FTTH Residential Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000204', 'a0000000-0000-4000-a000-000000000101', 'Download Speed', 'سرعة التحميل', 'Maximum download speed in Mbps', 'سرعة التحميل القصوى بالميغابت في الثانية', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000205', 'a0000000-0000-4000-a000-000000000101', 'Upload Speed', 'سرعة الرفع', 'Maximum upload speed in Mbps', 'سرعة الرفع القصوى بالميغابت في الثانية', 'number', true, true, 2),
('a0000000-0000-4000-a000-000000000206', 'a0000000-0000-4000-a000-000000000101', 'Technology', 'التقنية', 'Access network technology', 'تقنية شبكة الوصول', 'string', false, true, 3),
('a0000000-0000-4000-a000-000000000207', 'a0000000-0000-4000-a000-000000000101', 'Equipment Vendor', 'مورد المعدات', 'OLT/ONT equipment vendor', 'مورد معدات OLT/ONT', 'string', false, false, 4) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, unit_of_measure, is_default)
VALUES
('a0000000-0000-4000-a000-000000000214', 'a0000000-0000-4000-a000-000000000206', 'FTTH', NULL, true),
('a0000000-0000-4000-a000-000000000215', 'a0000000-0000-4000-a000-000000000207', 'Huawei', NULL, true)
ON CONFLICT DO NOTHING;

-- Hatif Tawasol Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000208', 'a0000000-0000-4000-a000-000000000102', 'Included Minutes', 'الدقائق المضمنة', 'Monthly included voice minutes', 'دقائق الصوت الشهرية المضمنة', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000209', 'a0000000-0000-4000-a000-000000000102', 'Call Type', 'نوع المكالمة', 'Types of calls included', 'أنواع المكالمات المضمنة', 'string', false, true, 2) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000216', 'a0000000-0000-4000-a000-000000000209', 'Local & National', true)
ON CONFLICT DO NOTHING;

-- Supernet ADSL Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000210', 'a0000000-0000-4000-a000-000000000103', 'Download Speed', 'سرعة التحميل', 'Maximum ADSL download speed in Mbps', 'سرعة التحميل القصوى لـ ADSL بالميغابت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000220', 'a0000000-0000-4000-a000-000000000103', 'Technology', 'التقنية', 'DSL technology variant', 'نوع تقنية DSL', 'string', false, true, 2) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000217', 'a0000000-0000-4000-a000-000000000220', 'ADSL2+', true)
ON CONFLICT DO NOTHING;

-- Yemen 4G Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000221', 'a0000000-0000-4000-a000-000000000104', 'Data Allowance', 'حصة البيانات', 'Included data volume', 'حجم البيانات المضمن', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000222', 'a0000000-0000-4000-a000-000000000104', 'Validity Period', 'فترة الصلاحية', 'Plan validity duration', 'مدة صلاحية الباقة', 'string', true, true, 2),
('a0000000-0000-4000-a000-000000000223', 'a0000000-0000-4000-a000-000000000104', 'Technology', 'التقنية', 'Mobile network technology', 'تقنية الشبكة المتنقلة', 'string', false, true, 3) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000218', 'a0000000-0000-4000-a000-000000000223', 'LTE Advanced', true)
ON CONFLICT DO NOTHING;

-- Home Wireless 4G Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000224', 'a0000000-0000-4000-a000-000000000105', 'Data Allowance', 'حصة البيانات', 'Monthly data cap in GB', 'الحد الشهري للبيانات بالجيجابايت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000225', 'a0000000-0000-4000-a000-000000000105', 'Max Speed', 'السرعة القصوى', 'Maximum connection speed in Mbps', 'سرعة الاتصال القصوى بالميغابت', 'number', false, true, 2),
('a0000000-0000-4000-a000-000000000226', 'a0000000-0000-4000-a000-000000000105', 'CPE Included', 'جهاز CPE مضمن', 'Whether CPE equipment is included', 'ما إذا كان جهاز CPE مضمنًا', 'boolean', false, true, 3) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000219', 'a0000000-0000-4000-a000-000000000226', 'true', true)
ON CONFLICT DO NOTHING;

-- Supplementary Telephone Services Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000227', 'a0000000-0000-4000-a000-000000000106', 'Feature Type', 'نوع الخدمة', 'Supplementary service feature', 'خدمة إضافية', 'string', true, true, 1) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000230', 'a0000000-0000-4000-a000-000000000227', 'Call Forwarding', false),
('a0000000-0000-4000-a000-000000000231', 'a0000000-0000-4000-a000-000000000227', 'Call Waiting', false),
('a0000000-0000-4000-a000-000000000232', 'a0000000-0000-4000-a000-000000000227', 'Caller ID', false),
('a0000000-0000-4000-a000-000000000233', 'a0000000-0000-4000-a000-000000000227', 'Voicemail', false),
('a0000000-0000-4000-a000-000000000234', 'a0000000-0000-4000-a000-000000000227', 'Call Barring', false)
ON CONFLICT DO NOTHING;

-- Hatif Fawtara Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000228', 'a0000000-0000-4000-a000-000000000107', 'Rental Type', 'نوع الإيجار', 'Monthly line rental type', 'نوع الإيجار الشهري للخط', 'string', true, true, 1) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000235', 'a0000000-0000-4000-a000-000000000228', 'Standard', true),
('a0000000-0000-4000-a000-000000000236', 'a0000000-0000-4000-a000-000000000228', 'Business', false)
ON CONFLICT DO NOTHING;

-- Yemen WiFi Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000229', 'a0000000-0000-4000-a000-000000000108', 'Session Duration', 'مدة الجلسة', 'Maximum WiFi session duration', 'الحد الأقصى لمدة جلسة الواي فاي', 'number', false, true, 1) ON CONFLICT DO NOTHING;

-- Business FTTH Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000240', 'a0000000-0000-4000-a000-000000000110', 'Download Speed', 'سرعة التحميل', 'Committed download speed in Mbps', 'سرعة التحميل المضمونة بالميغابت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000241', 'a0000000-0000-4000-a000-000000000110', 'Upload Speed', 'سرعة الرفع', 'Committed upload speed in Mbps', 'سرعة الرفع المضمونة بالميغابت', 'number', true, true, 2),
('a0000000-0000-4000-a000-000000000242', 'a0000000-0000-4000-a000-000000000110', 'SLA Uptime', 'نسبة تشغيل SLA', 'Service level agreement uptime guarantee', 'ضمان تشغيل اتفاقية مستوى الخدمة', 'number', false, true, 3),
('a0000000-0000-4000-a000-000000000243', 'a0000000-0000-4000-a000-000000000110', 'Support Response', 'وقت الاستجابة للدعم', 'SLA support response time in hours', 'وقت الاستجابة للدعم الفني حسب SLA بالساعات', 'number', false, false, 4) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, unit_of_measure, is_default)
VALUES
('a0000000-0000-4000-a000-000000000237', 'a0000000-0000-4000-a000-000000000242', '99.9', 'percent', true),
('a0000000-0000-4000-a000-000000000238', 'a0000000-0000-4000-a000-000000000243', '4', 'hours', true)
ON CONFLICT DO NOTHING;

-- DIA Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000244', 'a0000000-0000-4000-a000-000000000111', 'Bandwidth', 'سعة النطاق', 'Dedicated bandwidth in Mbps', 'سعة النطاق المخصصة بالميغابت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000245', 'a0000000-0000-4000-a000-000000000111', 'Contention Ratio', 'نسبة التنافس', 'Bandwidth contention ratio', 'نسبة التنافس على النطاق', 'string', false, true, 2),
('a0000000-0000-4000-a000-000000000246', 'a0000000-0000-4000-a000-000000000111', 'SLA Uptime', 'نسبة تشغيل SLA', 'Service level agreement uptime guarantee', 'ضمان تشغيل اتفاقية مستوى الخدمة', 'number', false, true, 3),
('a0000000-0000-4000-a000-000000000247', 'a0000000-0000-4000-a000-000000000111', 'Traffic Type', 'نوع الحركة', 'Type of internet traffic', 'نوع حركة الإنترنت', 'string', false, true, 4) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000239', 'a0000000-0000-4000-a000-000000000245', '1:1', true),
('a0000000-0000-4000-a000-000000000248', 'a0000000-0000-4000-a000-000000000246', '99.99', true),
('a0000000-0000-4000-a000-000000000249', 'a0000000-0000-4000-a000-000000000247', 'Symmetric', true)
ON CONFLICT DO NOTHING;

-- Ethernet Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000250', 'a0000000-0000-4000-a000-000000000112', 'Port Speed', 'سرعة المنفذ', 'Ethernet port speed in Mbps/Gbps', 'سرعة منفذ الإيثرنت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000251', 'a0000000-0000-4000-a000-000000000112', 'Topology', 'طوبولوجيا الشبكة', 'Network topology type', 'نوع طوبولوجيا الشبكة', 'string', true, true, 2) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000252', 'a0000000-0000-4000-a000-000000000251', 'Point-to-Point', true),
('a0000000-0000-4000-a000-000000000253', 'a0000000-0000-4000-a000-000000000251', 'Point-to-Multipoint', false)
ON CONFLICT DO NOTHING;

-- TDM Circuits Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000254', 'a0000000-0000-4000-a000-000000000113', 'Circuit Type', 'نوع الدائرة', 'TDM circuit type', 'نوع دائرة TDM', 'string', true, true, 1),
('a0000000-0000-4000-a000-000000000255', 'a0000000-0000-4000-a000-000000000113', 'Capacity', 'السعة', 'Circuit capacity in E1 or STM units', 'سعة الدائرة بوحدات E1 أو STM', 'number', true, true, 2) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000256', 'a0000000-0000-4000-a000-000000000254', 'E1', true),
('a0000000-0000-4000-a000-000000000257', 'a0000000-0000-4000-a000-000000000254', 'STM-1', false),
('a0000000-0000-4000-a000-000000000258', 'a0000000-0000-4000-a000-000000000254', 'STM-4', false)
ON CONFLICT DO NOTHING;

-- PRI Voice Channels Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000259', 'a0000000-0000-4000-a000-000000000114', 'Channel Count', 'عدد القنوات', 'Number of B-channels', 'عدد قنوات B', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000260', 'a0000000-0000-4000-a000-000000000114', 'Signaling Type', 'نوع الإشارات', 'PRI signaling protocol', 'بروتوكول إشارات PRI', 'string', false, true, 2) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000261', 'a0000000-0000-4000-a000-000000000260', 'ISDN PRI', true)
ON CONFLICT DO NOTHING;

-- 800 Free Number Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000262', 'a0000000-0000-4000-a000-000000000115', 'Included Minutes', 'الدقائق المضمنة', 'Monthly included inbound minutes', 'الدقائق الشهرية المضمنة للواردة', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000263', 'a0000000-0000-4000-a000-000000000115', 'Concurrent Calls', 'المكالمات المتزامنة', 'Maximum concurrent call capacity', 'الحد الأقصى للمكالمات المتزامنة', 'number', true, false, 2) ON CONFLICT DO NOTHING;

-- Static IP Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000264', 'a0000000-0000-4000-a000-000000000116', 'IPv4 Count', 'عدد IPv4', 'Number of IPv4 addresses', 'عدد عناوين IPv4', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000265', 'a0000000-0000-4000-a000-000000000116', 'IPv6 Prefix', 'بادئة IPv6', 'IPv6 prefix length if applicable', 'طول بادئة IPv6 إن وجد', 'string', true, false, 2) ON CONFLICT DO NOTHING;

-- Wireless Transmission Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000266', 'a0000000-0000-4000-a000-000000000117', 'Bandwidth', 'سعة النطاق', 'Committed bandwidth in Mbps', 'سعة النطاق المضمونة بالميغابت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000267', 'a0000000-0000-4000-a000-000000000117', 'Technology', 'التقنية', 'Wireless technology generation', 'تقنية الجيل اللاسلكي', 'string', false, true, 2) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000268', 'a0000000-0000-4000-a000-000000000267', '4G LTE Advanced', true)
ON CONFLICT DO NOTHING;

-- Dedicated Server Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000269', 'a0000000-0000-4000-a000-000000000118', 'CPU Cores', 'أنوية المعالج', 'Number of CPU cores', 'عدد أنوية المعالج', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000270', 'a0000000-0000-4000-a000-000000000118', 'RAM', 'الذاكرة', 'Memory in GB', 'الذاكرة بالجيجابايت', 'number', true, true, 2),
('a0000000-0000-4000-a000-000000000271', 'a0000000-0000-4000-a000-000000000118', 'Storage', 'التخزين', 'Storage capacity in GB', 'سعة التخزين بالجيجابايت', 'number', true, true, 3),
('a0000000-0000-4000-a000-000000000272', 'a0000000-0000-4000-a000-000000000118', 'Bandwidth', 'سعة النطاق', 'Monthly bandwidth allowance in TB', 'حصة النطاق الشهرية بالتيرابايت', 'number', true, true, 4) ON CONFLICT DO NOTHING;

-- VPS Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000273', 'a0000000-0000-4000-a000-000000000119', 'vCPU', 'معالج افتراضي', 'Number of virtual CPU cores', 'عدد أنوية المعالج الافتراضية', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000274', 'a0000000-0000-4000-a000-000000000119', 'RAM', 'الذاكرة', 'Memory in GB', 'الذاكرة بالجيجابايت', 'number', true, true, 2),
('a0000000-0000-4000-a000-000000000275', 'a0000000-0000-4000-a000-000000000119', 'Storage', 'التخزين', 'SSD storage in GB', 'مساحة تخزين SSD بالجيجابايت', 'number', true, true, 3) ON CONFLICT DO NOTHING;

-- Colocation Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000276', 'a0000000-0000-4000-a000-000000000120', 'Space Unit', 'وحدة المساحة', 'Rack units or cabinet space', 'وحدات الرف أو مساحة الخزانة', 'string', true, true, 1),
('a0000000-0000-4000-a000-000000000277', 'a0000000-0000-4000-a000-000000000120', 'Power', 'الطاقة', 'Power allocation in watts', 'حصة الطاقة بالواط', 'number', true, true, 2),
('a0000000-0000-4000-a000-000000000278', 'a0000000-0000-4000-a000-000000000120', 'Bandwidth', 'النطاق', 'Included bandwidth in Mbps', 'النطاق المضمن بالميغابت', 'number', true, true, 3) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000279', 'a0000000-0000-4000-a000-000000000276', 'Half Rack', false),
('a0000000-0000-4000-a000-000000000280', 'a0000000-0000-4000-a000-000000000276', 'Full Rack', true),
('a0000000-0000-4000-a000-000000000281', 'a0000000-0000-4000-a000-000000000276', 'Cabinet', false)
ON CONFLICT DO NOTHING;

-- Web Hosting Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000282', 'a0000000-0000-4000-a000-000000000121', 'Disk Space', 'مساحة التخزين', 'Web hosting disk space in GB', 'مساحة تخزين الاستضافة بالجيجابايت', 'number', true, true, 1),
('a0000000-0000-4000-a000-000000000283', 'a0000000-0000-4000-a000-000000000121', 'Bandwidth', 'النطاق', 'Monthly bandwidth in GB', 'النطاق الشهري بالجيجابايت', 'number', true, true, 2),
('a0000000-0000-4000-a000-000000000284', 'a0000000-0000-4000-a000-000000000121', 'Domains Supported', 'النطاقات المدعومة', 'Number of hosted domains', 'عدد النطاقات المستضافة', 'number', true, true, 3) ON CONFLICT DO NOTHING;

-- Domain Registration Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000285', 'a0000000-0000-4000-a000-000000000122', 'TLD', 'النطاق الأعلى', 'Top-level domain extension', 'امتداد النطاق الأعلى', 'string', true, true, 1),
('a0000000-0000-4000-a000-000000000286', 'a0000000-0000-4000-a000-000000000122', 'Registration Period', 'فترة التسجيل', 'Registration duration in years', 'مدة التسجيل بالسنوات', 'number', true, true, 2) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000287', 'a0000000-0000-4000-a000-000000000285', '.com', true),
('a0000000-0000-4000-a000-000000000288', 'a0000000-0000-4000-a000-000000000285', '.net', false),
('a0000000-0000-4000-a000-000000000289', 'a0000000-0000-4000-a000-000000000285', '.org', false),
('a0000000-0000-4000-a000-000000000290', 'a0000000-0000-4000-a000-000000000285', '.ye', false)
ON CONFLICT DO NOTHING;

-- ATM Connectivity Characteristics

INSERT INTO service_spec_characteristics (id, service_specification_id, name, name_ar, description, description_ar, value_type, configurable, is_required, sort_order)
VALUES
('a0000000-0000-4000-a000-000000000291', 'a0000000-0000-4000-a000-000000000123', 'Connection Type', 'نوع الاتصال', 'ATM connectivity technology type', 'نوع تقنية اتصال الصراف الآلي', 'string', true, true, 1),
('a0000000-0000-4000-a000-000000000292', 'a0000000-0000-4000-a000-000000000123', 'Bandwidth', 'سعة النطاق', 'Connection bandwidth in Mbps', 'سعة النطاق بالميغابت', 'number', true, true, 2),
('a0000000-0000-4000-a000-000000000293', 'a0000000-0000-4000-a000-000000000123', 'SLA Uptime', 'نسبة تشغيل SLA', 'Service level agreement uptime', 'نسبة تشغيل اتفاقية مستوى الخدمة', 'number', false, true, 3) ON CONFLICT DO NOTHING;


INSERT INTO service_spec_characteristic_values (id, characteristic_id, value, is_default)
VALUES
('a0000000-0000-4000-a000-000000000294', 'a0000000-0000-4000-a000-000000000291', 'MPLS', true),
('a0000000-0000-4000-a000-000000000295', 'a0000000-0000-4000-a000-000000000291', 'Ethernet', false),
('a0000000-0000-4000-a000-000000000296', 'a0000000-0000-4000-a000-000000000293', '99.99', true)
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 5: Catalog Categories
-- =========================================================================


COMMIT;
