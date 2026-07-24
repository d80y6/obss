-- Yemen PTC Catalog - obss_catalog seed
BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'categories' AND column_name = 'name_ar') THEN
        ALTER TABLE categories ADD COLUMN name_ar character varying(200);
        ALTER TABLE categories ADD COLUMN description_ar character varying(1000);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'products' AND column_name = 'name_ar') THEN
        ALTER TABLE products ADD COLUMN name_ar character varying(200);
        ALTER TABLE products ADD COLUMN description_ar character varying(2000);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'offers' AND column_name = 'name_ar') THEN
        ALTER TABLE offers ADD COLUMN name_ar character varying(200);
        ALTER TABLE offers ADD COLUMN description_ar character varying(2000);
    END IF;
END $$;

INSERT INTO categories (id, tenant_id, name, name_ar, description, description_ar, parent_category_id, is_active, lifecycle_status, sort_order, created_at, updated_at, version)
VALUES
('a0000000-0000-4000-a000-000000000301', 'a0000000-0000-4000-a000-000000000001', 'Residential Internet', 'إنترنت سكني', 'Broadband internet products for residential customers', 'منتجات الإنترنت عريض النطاق للعملاء السكنيين', NULL, true, 'Active', 1, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000302', 'a0000000-0000-4000-a000-000000000001', 'Residential Voice', 'صوت سكني', 'Telephone and voice products for residential customers', 'منتجات الهاتف والصوت للعملاء السكنيين', NULL, true, 'Active', 2, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000303', 'a0000000-0000-4000-a000-000000000001', 'Residential Mobile', 'متنقل سكني', 'Mobile and wireless products for residential customers', 'منتجات المتنقل واللاسلكي للعملاء السكنيين', NULL, true, 'Active', 3, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000304', 'a0000000-0000-4000-a000-000000000001', 'Residential Bundles', 'باقات سكنية', 'Bundled residential service packages', 'باقات الخدمات السكنية المجمعة', NULL, true, 'Active', 4, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000311', 'a0000000-0000-4000-a000-000000000001', 'Fiber Optic', 'الألياف الضوئية', 'Fiber optic broadband plans', 'باقات النطاق العريض عبر الألياف الضوئية', 'a0000000-0000-4000-a000-000000000301', true, 'Active', 1, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000312', 'a0000000-0000-4000-a000-000000000001', 'ADSL', 'ADSL', 'ADSL broadband internet plans', 'باقات الإنترنت عبر ADSL', 'a0000000-0000-4000-a000-000000000301', true, 'Active', 2, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000313', 'a0000000-0000-4000-a000-000000000001', 'Wireless Broadband', 'النطاق العريض اللاسلكي', 'Fixed wireless 4G broadband plans', 'باقات النطاق العريض اللاسلكي الثابت عبر 4G', 'a0000000-0000-4000-a000-000000000303', true, 'Active', 1, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000321', 'a0000000-0000-4000-a000-000000000001', 'Business Connectivity', 'ربط الأعمال', 'Dedicated connectivity and data products for businesses', 'منتجات الربط المخصصة والبيانات للشركات', NULL, true, 'Active', 5, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000322', 'a0000000-0000-4000-a000-000000000001', 'Business Voice', 'الصوت للأعمال', 'Voice and telephony products for businesses', 'منتجات الصوت والهاتف للشركات', NULL, true, 'Active', 6, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000323', 'a0000000-0000-4000-a000-000000000001', 'Cloud & Infrastructure', 'السحابة والبنية التحتية', 'Cloud hosting and data center products', 'منتجات الاستضافة السحابية ومراكز البيانات', NULL, true, 'Active', 7, NOW(), NOW(), 1),
('a0000000-0000-4000-a000-000000000324', 'a0000000-0000-4000-a000-000000000001', 'Value-Added Business', 'قيمة مضافة للأعمال', 'Value-added and specialized business products', 'منتجات متخصصة وذات قيمة مضافة للشركات', NULL, true, 'Active', 8, NOW(), NOW(), 1)
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 6: Products
-- =========================================================================

-- Residential Products

INSERT INTO products (id, tenant_id, name, name_ar, description, description_ar, category_id, product_type, is_active, is_shippable, taxable, lifecycle_status, product_number, created_at, updated_at)
VALUES
-- Super Shamel Bundle
('b0000000-0000-4000-a000-000000000100', 'a0000000-0000-4000-a000-000000000001', 'Super Shamel', 'سوبر شامل', 'All-in-one bundle: FTTH 100Mbps, Hatif Tawasol 500min, Yemen 4G 10GB', 'باقة شاملة مع FTTH 100 ميجابت، هاتفي تواصل 500 دقيقة، يمن 4G 10 جيجابايت', 'a0000000-0000-4000-a000-000000000304', 'BUNDLE', true, false, true, 'Active', 'RES-BND-001', NOW(), NOW()),
-- FTTH
('b0000000-0000-4000-a000-000000000101', 'a0000000-0000-4000-a000-000000000001', 'FTTH 20Mbps', 'FTTH 20 ميجابت', 'Fiber optic internet - 20Mbps download, 10Mbps upload', 'إنترنت عبر الألياف الضوئية - 20 ميجابت تحميل، 10 ميجابت رفع', 'a0000000-0000-4000-a000-000000000311', 'BROADBAND', true, false, true, 'Active', 'RES-FTH-020', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000102', 'a0000000-0000-4000-a000-000000000001', 'FTTH 50Mbps', 'FTTH 50 ميجابت', 'Fiber optic internet - 50Mbps download, 25Mbps upload', 'إنترنت عبر الألياف الضوئية - 50 ميجابت تحميل، 25 ميجابت رفع', 'a0000000-0000-4000-a000-000000000311', 'BROADBAND', true, false, true, 'Active', 'RES-FTH-050', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000103', 'a0000000-0000-4000-a000-000000000001', 'FTTH 100Mbps', 'FTTH 100 ميجابت', 'Fiber optic internet - 100Mbps download, 50Mbps upload', 'إنترنت عبر الألياف الضوئية - 100 ميجابت تحميل، 50 ميجابت رفع', 'a0000000-0000-4000-a000-000000000311', 'BROADBAND', true, false, true, 'Active', 'RES-FTH-100', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000104', 'a0000000-0000-4000-a000-000000000001', 'FTTH 200Mbps', 'FTTH 200 ميجابت', 'Fiber optic internet - 200Mbps download, 100Mbps upload', 'إنترنت عبر الألياف الضوئية - 200 ميجابت تحميل، 100 ميجابت رفع', 'a0000000-0000-4000-a000-000000000311', 'BROADBAND', true, false, true, 'Active', 'RES-FTH-200', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000105', 'a0000000-0000-4000-a000-000000000001', 'FTTH 500Mbps', 'FTTH 500 ميجابت', 'Fiber optic internet - 500Mbps download, 250Mbps upload', 'إنترنت عبر الألياف الضوئية - 500 ميجابت تحميل، 250 ميجابت رفع', 'a0000000-0000-4000-a000-000000000311', 'BROADBAND', true, false, true, 'Active', 'RES-FTH-500', NOW(), NOW()),
-- Hatif Tawasol
('b0000000-0000-4000-a000-000000000106', 'a0000000-0000-4000-a000-000000000001', 'Hatif Tawasol 500', 'هاتفي تواصل 500', 'VoIP telephone service with 500 monthly minutes', 'خدمة هاتف VoIP مع 500 دقيقة شهرية', 'a0000000-0000-4000-a000-000000000302', 'VOICE', true, false, true, 'Active', 'RES-HTF-500', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000107', 'a0000000-0000-4000-a000-000000000001', 'Hatif Tawasol 1000', 'هاتفي تواصل 1000', 'VoIP telephone service with 1000 monthly minutes', 'خدمة هاتف VoIP مع 1000 دقيقة شهرية', 'a0000000-0000-4000-a000-000000000302', 'VOICE', true, false, true, 'Active', 'RES-HTF-1000', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000108', 'a0000000-0000-4000-a000-000000000001', 'Hatif Tawasol Unlimited', 'هاتفي تواصل غير محدود', 'VoIP telephone service with unlimited local minutes', 'خدمة هاتف VoIP مع دقائق محلية غير محدودة', 'a0000000-0000-4000-a000-000000000302', 'VOICE', true, false, true, 'Active', 'RES-HTF-UNL', NOW(), NOW()),
-- ADSL
('b0000000-0000-4000-a000-000000000109', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 4Mbps', 'سوبرنت ADSL 4 ميجابت', 'ADSL broadband - 4Mbps download', 'إنترنت ADSL - 4 ميجابت تحميل', 'a0000000-0000-4000-a000-000000000312', 'BROADBAND', true, false, true, 'Active', 'RES-ADL-004', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000110', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 8Mbps', 'سوبرنت ADSL 8 ميجابت', 'ADSL broadband - 8Mbps download', 'إنترنت ADSL - 8 ميجابت تحميل', 'a0000000-0000-4000-a000-000000000312', 'BROADBAND', true, false, true, 'Active', 'RES-ADL-008', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000111', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 16Mbps', 'سوبرنت ADSL 16 ميجابت', 'ADSL broadband - 16Mbps download', 'إنترنت ADSL - 16 ميجابت تحميل', 'a0000000-0000-4000-a000-000000000312', 'BROADBAND', true, false, true, 'Active', 'RES-ADL-016', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000112', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 20Mbps', 'سوبرنت ADSL 20 ميجابت', 'ADSL broadband - 20Mbps download (ADSL2+)', 'إنترنت ADSL - 20 ميجابت تحميل (ADSL2+)', 'a0000000-0000-4000-a000-000000000312', 'BROADBAND', true, false, true, 'Active', 'RES-ADL-020', NOW(), NOW()),
-- Yemen 4G
('b0000000-0000-4000-a000-000000000113', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Daily', 'يمن فور جي يومي', 'Daily 4G data pack - 1GB', 'باقة بيانات يومية 4G - 1 جيجابايت', 'a0000000-0000-4000-a000-000000000313', 'MOBILE_DATA', true, false, true, 'Active', 'RES-4G-DLY', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000114', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Weekly', 'يمن فور جي أسبوعي', 'Weekly 4G data pack - 5GB', 'باقة بيانات أسبوعية 4G - 5 جيجابايت', 'a0000000-0000-4000-a000-000000000313', 'MOBILE_DATA', true, false, true, 'Active', 'RES-4G-WKY', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000115', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Monthly 10GB', 'يمن فور جي شهري 10 جيجابايت', 'Monthly 4G data pack - 10GB', 'باقة بيانات شهرية 4G - 10 جيجابايت', 'a0000000-0000-4000-a000-000000000313', 'MOBILE_DATA', true, false, true, 'Active', 'RES-4G-M10', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000116', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Monthly 30GB', 'يمن فور جي شهري 30 جيجابايت', 'Monthly 4G data pack - 30GB', 'باقة بيانات شهرية 4G - 30 جيجابايت', 'a0000000-0000-4000-a000-000000000313', 'MOBILE_DATA', true, false, true, 'Active', 'RES-4G-M30', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000117', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Monthly 100GB', 'يمن فور جي شهري 100 جيجابايت', 'Monthly 4G data pack - 100GB', 'باقة بيانات شهرية 4G - 100 جيجابايت', 'a0000000-0000-4000-a000-000000000313', 'MOBILE_DATA', true, false, true, 'Active', 'RES-4G-M100', NOW(), NOW()),
-- Home Wireless 4G
('b0000000-0000-4000-a000-000000000118', 'a0000000-0000-4000-a000-000000000001', 'Home Wireless 4G 50GB', 'اللاسلكي المنزلي 4G 50 جيجابايت', 'Fixed wireless 4G broadband - 50GB monthly data', 'إنترنت لاسلكي ثابت 4G - 50 جيجابايت بيانات شهرية', 'a0000000-0000-4000-a000-000000000313', 'BROADBAND', true, false, true, 'Active', 'RES-HW4-050', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000119', 'a0000000-0000-4000-a000-000000000001', 'Home Wireless 4G 100GB', 'اللاسلكي المنزلي 4G 100 جيجابايت', 'Fixed wireless 4G broadband - 100GB monthly data', 'إنترنت لاسلكي ثابت 4G - 100 جيجابايت بيانات شهرية', 'a0000000-0000-4000-a000-000000000313', 'BROADBAND', true, false, true, 'Active', 'RES-HW4-100', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000120', 'a0000000-0000-4000-a000-000000000001', 'Home Wireless 4G Unlimited', 'اللاسلكي المنزلي 4G غير محدود', 'Fixed wireless 4G broadband - unlimited data (50Mbps max)', 'إنترنت لاسلكي ثابت 4G - بيانات غير محدودة (سرعة قصوى 50 ميجابت)', 'a0000000-0000-4000-a000-000000000313', 'BROADBAND', true, false, true, 'Active', 'RES-HW4-UNL', NOW(), NOW()),
-- Supplementary Telephone Services
('b0000000-0000-4000-a000-000000000121', 'a0000000-0000-4000-a000-000000000001', 'Call Forwarding', 'تحويل المكالمات', 'Forward calls to another number', 'تحويل المكالمات إلى رقم آخر', 'a0000000-0000-4000-a000-000000000302', 'VAS', true, false, true, 'Active', 'RES-SUP-CF', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000122', 'a0000000-0000-4000-a000-000000000001', 'Call Waiting', 'انتظار المكالمات', 'Place active calls on hold and answer incoming calls', 'وضع المكالمات النشطة في الانتظار والرد على المكالمات الواردة', 'a0000000-0000-4000-a000-000000000302', 'VAS', true, false, true, 'Active', 'RES-SUP-CW', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000123', 'a0000000-0000-4000-a000-000000000001', 'Caller ID', 'معرف المتصل', 'Display caller number on incoming calls', 'عرض رقم المتصل في المكالمات الواردة', 'a0000000-0000-4000-a000-000000000302', 'VAS', true, false, true, 'Active', 'RES-SUP-CID', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000124', 'a0000000-0000-4000-a000-000000000001', 'Voicemail', 'البريد الصوتي', 'Personal voicemail box for missed calls', 'صندوق بريد صوتي شخصي للمكالمات الفائتة', 'a0000000-0000-4000-a000-000000000302', 'VAS', true, false, true, 'Active', 'RES-SUP-VM', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000125', 'a0000000-0000-4000-a000-000000000001', 'Call Barring', 'حظر المكالمات', 'Restrict outgoing or incoming calls', 'تقييد المكالمات الصادرة أو الواردة', 'a0000000-0000-4000-a000-000000000302', 'VAS', true, false, true, 'Active', 'RES-SUP-CB', NOW(), NOW()),
-- Hatif Fawtara
('b0000000-0000-4000-a000-000000000126', 'a0000000-0000-4000-a000-000000000001', 'Hatif Fawtara Standard', 'هاتفي فوترة قياسي', 'Postpaid fixed telephone line with standard rental', 'خط هاتف ثابت بنظام الفوترة مع إيجار قياسي', 'a0000000-0000-4000-a000-000000000302', 'VOICE', true, false, true, 'Active', 'RES-HFW-STD', NOW(), NOW()),
-- Yemen WiFi
('b0000000-0000-4000-a000-000000000127', 'a0000000-0000-4000-a000-000000000001', 'Yemen WiFi Daily Pass', 'يمن واي فاي تذكرة يومية', '24-hour WiFi hotspot access pass', 'تصريح وصول واي فاي لمدة 24 ساعة', 'a0000000-0000-4000-a000-000000000301', 'INTERNET', true, false, true, 'Active', 'RES-WF-DAY', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000128', 'a0000000-0000-4000-a000-000000000001', 'Yemen WiFi Weekly Pass', 'يمن واي فاي تذكرة أسبوعية', '7-day WiFi hotspot access pass', 'تصريح وصول واي فاي لمدة 7 أيام', 'a0000000-0000-4000-a000-000000000301', 'INTERNET', true, false, true, 'Active', 'RES-WF-WKY', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000129', 'a0000000-0000-4000-a000-000000000001', 'Yemen WiFi Monthly Pass', 'يمن واي فاي تذكرة شهرية', '30-day WiFi hotspot access pass', 'تصريح وصول واي فاي لمدة 30 يومًا', 'a0000000-0000-4000-a000-000000000301', 'INTERNET', true, false, true, 'Active', 'RES-WF-MON', NOW(), NOW())
ON CONFLICT DO NOTHING;

-- Business Products

INSERT INTO products (id, tenant_id, name, name_ar, description, description_ar, category_id, product_type, is_active, is_shippable, taxable, lifecycle_status, product_number, created_at, updated_at)
VALUES
-- Business FTTH
('b0000000-0000-4000-a000-000000000130', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 50Mbps', 'FTTH للأعمال 50 ميجابت', 'Business fiber optic - 50Mbps with SLA 99.9%', 'ألياف ضوئية للأعمال - 50 ميجابت مع SLA 99.9%', 'a0000000-0000-4000-a000-000000000321', 'BROADBAND', true, false, true, 'Active', 'BIZ-FTH-050', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000131', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 100Mbps', 'FTTH للأعمال 100 ميجابت', 'Business fiber optic - 100Mbps with SLA 99.9%', 'ألياف ضوئية للأعمال - 100 ميجابت مع SLA 99.9%', 'a0000000-0000-4000-a000-000000000321', 'BROADBAND', true, false, true, 'Active', 'BIZ-FTH-100', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000132', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 200Mbps', 'FTTH للأعمال 200 ميجابت', 'Business fiber optic - 200Mbps with SLA 99.9%', 'ألياف ضوئية للأعمال - 200 ميجابت مع SLA 99.9%', 'a0000000-0000-4000-a000-000000000321', 'BROADBAND', true, false, true, 'Active', 'BIZ-FTH-200', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000133', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 500Mbps', 'FTTH للأعمال 500 ميجابت', 'Business fiber optic - 500Mbps with SLA 99.9%', 'ألياف ضوئية للأعمال - 500 ميجابت مع SLA 99.9%', 'a0000000-0000-4000-a000-000000000321', 'BROADBAND', true, false, true, 'Active', 'BIZ-FTH-500', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000134', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 1Gbps', 'FTTH للأعمال 1 جيجابت', 'Business fiber optic - 1Gbps with SLA 99.9%', 'ألياف ضوئية للأعمال - 1 جيجابت مع SLA 99.9%', 'a0000000-0000-4000-a000-000000000321', 'BROADBAND', true, false, true, 'Active', 'BIZ-FTH-1G', NOW(), NOW()),
-- DIA
('b0000000-0000-4000-a000-000000000135', 'a0000000-0000-4000-a000-000000000001', 'DIA 10Mbps', 'إنترنت مخصص 10 ميجابت', 'Dedicated symmetric internet - 10Mbps, 1:1 contention', 'إنترنت مخصص متماثل - 10 ميجابت، تنافس 1:1', 'a0000000-0000-4000-a000-000000000321', 'DIA', true, false, true, 'Active', 'BIZ-DIA-010', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000136', 'a0000000-0000-4000-a000-000000000001', 'DIA 50Mbps', 'إنترنت مخصص 50 ميجابت', 'Dedicated symmetric internet - 50Mbps, 1:1 contention', 'إنترنت مخصص متماثل - 50 ميجابت، تنافس 1:1', 'a0000000-0000-4000-a000-000000000321', 'DIA', true, false, true, 'Active', 'BIZ-DIA-050', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000137', 'a0000000-0000-4000-a000-000000000001', 'DIA 100Mbps', 'إنترنت مخصص 100 ميجابت', 'Dedicated symmetric internet - 100Mbps, 1:1 contention', 'إنترنت مخصص متماثل - 100 ميجابت، تنافس 1:1', 'a0000000-0000-4000-a000-000000000321', 'DIA', true, false, true, 'Active', 'BIZ-DIA-100', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000138', 'a0000000-0000-4000-a000-000000000001', 'DIA 500Mbps', 'إنترنت مخصص 500 ميجابت', 'Dedicated symmetric internet - 500Mbps, 1:1 contention', 'إنترنت مخصص متماثل - 500 ميجابت، تنافس 1:1', 'a0000000-0000-4000-a000-000000000321', 'DIA', true, false, true, 'Active', 'BIZ-DIA-500', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000139', 'a0000000-0000-4000-a000-000000000001', 'DIA 1Gbps', 'إنترنت مخصص 1 جيجابت', 'Dedicated symmetric internet - 1Gbps, 1:1 contention', 'إنترنت مخصص متماثل - 1 جيجابت، تنافس 1:1', 'a0000000-0000-4000-a000-000000000321', 'DIA', true, false, true, 'Active', 'BIZ-DIA-1G', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000140', 'a0000000-0000-4000-a000-000000000001', 'DIA 10Gbps', 'إنترنت مخصص 10 جيجابت', 'Dedicated symmetric internet - 10Gbps, 1:1 contention', 'إنترنت مخصص متماثل - 10 جيجابت، تنافس 1:1', 'a0000000-0000-4000-a000-000000000321', 'DIA', true, false, true, 'Active', 'BIZ-DIA-10G', NOW(), NOW()),
-- Ethernet
('b0000000-0000-4000-a000-000000000141', 'a0000000-0000-4000-a000-000000000001', 'Ethernet Point-to-Point 100Mbps', 'إيثرنت نقطة-لنقطة 100 ميجابت', 'Point-to-point Ethernet - 100Mbps', 'إيثرنت من نقطة إلى نقطة - 100 ميجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-ETH-P2P-100', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000142', 'a0000000-0000-4000-a000-000000000001', 'Ethernet Point-to-Point 1Gbps', 'إيثرنت نقطة-لنقطة 1 جيجابت', 'Point-to-point Ethernet - 1Gbps', 'إيثرنت من نقطة إلى نقطة - 1 جيجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-ETH-P2P-1G', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000143', 'a0000000-0000-4000-a000-000000000001', 'Ethernet Point-to-Multipoint 100Mbps', 'إيثرنت نقطة-لعدة نقاط 100 ميجابت', 'Point-to-multipoint Ethernet - 100Mbps per site', 'إيثرنت من نقطة إلى عدة نقاط - 100 ميجابت لكل موقع', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-ETH-P2MP-100', NOW(), NOW()),
-- TDM
('b0000000-0000-4000-a000-000000000144', 'a0000000-0000-4000-a000-000000000001', 'E1 Circuit', 'دائرة E1', 'E1 TDM circuit - 2.048Mbps', 'دائرة E1 - 2.048 ميجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-TDM-E1', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000145', 'a0000000-0000-4000-a000-000000000001', 'STM-1 Circuit', 'دائرة STM-1', 'STM-1 TDM circuit - 155.52Mbps', 'دائرة STM-1 - 155.52 ميجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-TDM-STM1', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000146', 'a0000000-0000-4000-a000-000000000001', 'STM-4 Circuit', 'دائرة STM-4', 'STM-4 TDM circuit - 622.08Mbps', 'دائرة STM-4 - 622.08 ميجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-TDM-STM4', NOW(), NOW()),
-- PRI
('b0000000-0000-4000-a000-000000000147', 'a0000000-0000-4000-a000-000000000001', 'PRI E1 Trunk', 'خط PRI E1', 'E1 PRI ISDN trunk - 30 voice channels', 'خط E1 PRI - 30 قناة صوتية', 'a0000000-0000-4000-a000-000000000322', 'VOICE', true, false, true, 'Active', 'BIZ-PRI-E1', NOW(), NOW()),
-- 800 Number
('b0000000-0000-4000-a000-000000000148', 'a0000000-0000-4000-a000-000000000001', '800 Free Number Basic', 'الرقم المجاني 800 أساسي', 'Toll-free 800 number with 500 included minutes', 'رقم مجاني 800 مع 500 دقيقة مضمنة', 'a0000000-0000-4000-a000-000000000322', 'VOICE', true, false, true, 'Active', 'BIZ-800-BAS', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000149', 'a0000000-0000-4000-a000-000000000001', '800 Free Number Premium', 'الرقم المجاني 800 ممتاز', 'Toll-free 800 number with 2000 included minutes', 'رقم مجاني 800 مع 2000 دقيقة مضمنة', 'a0000000-0000-4000-a000-000000000322', 'VOICE', true, false, true, 'Active', 'BIZ-800-PRM', NOW(), NOW()),
-- Static IP
('b0000000-0000-4000-a000-000000000150', 'a0000000-0000-4000-a000-000000000001', 'Static IPv4 Address', 'عنوان IPv4 ثابت', 'Single dedicated static public IPv4 address', 'عنوان IPv4 عام ثابت مخصص واحد', 'a0000000-0000-4000-a000-000000000324', 'VAS', true, false, true, 'Active', 'BIZ-IP-SGL', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000151', 'a0000000-0000-4000-a000-000000000001', 'Static IP Block /29', 'كتلة عناوين ثابتة /29', 'Block of 5 usable static public IPv4 addresses', 'كتلة من 5 عناوين IPv4 عامة ثابتة قابلة للاستخدام', 'a0000000-0000-4000-a000-000000000324', 'VAS', true, false, true, 'Active', 'BIZ-IP-29', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000152', 'a0000000-0000-4000-a000-000000000001', 'Static IP Block /28', 'كتلة عناوين ثابتة /28', 'Block of 13 usable static public IPv4 addresses', 'كتلة من 13 عنوان IPv4 عام ثابت قابل للاستخدام', 'a0000000-0000-4000-a000-000000000324', 'VAS', true, false, true, 'Active', 'BIZ-IP-28', NOW(), NOW()),
-- Wireless Transmission
('b0000000-0000-4000-a000-000000000153', 'a0000000-0000-4000-a000-000000000001', 'Wireless Transmission 10Mbps', 'نقل لاسلكي 10 ميجابت', 'Wireless data transmission over 4G - 10Mbps', 'نقل بيانات لاسلكي عبر 4G - 10 ميجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-WTX-010', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000154', 'a0000000-0000-4000-a000-000000000001', 'Wireless Transmission 50Mbps', 'نقل لاسلكي 50 ميجابت', 'Wireless data transmission over 4G - 50Mbps', 'نقل بيانات لاسلكي عبر 4G - 50 ميجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-WTX-050', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000155', 'a0000000-0000-4000-a000-000000000001', 'Wireless Transmission 100Mbps', 'نقل لاسلكي 100 ميجابت', 'Wireless data transmission over 4G - 100Mbps', 'نقل بيانات لاسلكي عبر 4G - 100 ميجابت', 'a0000000-0000-4000-a000-000000000321', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-WTX-100', NOW(), NOW()),
-- Dedicated Server
('b0000000-0000-4000-a000-000000000156', 'a0000000-0000-4000-a000-000000000001', 'Dedicated Server Basic', 'خادم مخصص أساسي', '4 vCPU, 8GB RAM, 500GB HDD, 5TB bandwidth', '4 معالج, 8 جيجابايت رام, 500 جيجابايت HDD, 5 تيرابايت نطاق', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-DSR-BAS', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000157', 'a0000000-0000-4000-a000-000000000001', 'Dedicated Server Standard', 'خادم مخصص قياسي', '8 vCPU, 16GB RAM, 1TB HDD, 10TB bandwidth', '8 معالج, 16 جيجابايت رام, 1 تيرابايت HDD, 10 تيرابايت نطاق', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-DSR-STD', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000158', 'a0000000-0000-4000-a000-000000000001', 'Dedicated Server Premium', 'خادم مخصص ممتاز', '16 vCPU, 32GB RAM, 2TB SSD, 20TB bandwidth', '16 معالج, 32 جيجابايت رام, 2 تيرابايت SSD, 20 تيرابايت نطاق', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-DSR-PRM', NOW(), NOW()),
-- VPS
('b0000000-0000-4000-a000-000000000159', 'a0000000-0000-4000-a000-000000000001', 'VPS 1 vCPU', 'VPS 1 معالج', '1 vCPU, 1GB RAM, 20GB SSD', '1 معالج, 1 جيجابايت رام, 20 جيجابايت SSD', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-VPS-001', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000160', 'a0000000-0000-4000-a000-000000000001', 'VPS 2 vCPU', 'VPS 2 معالج', '2 vCPU, 4GB RAM, 50GB SSD', '2 معالج, 4 جيجابايت رام, 50 جيجابايت SSD', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-VPS-002', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000161', 'a0000000-0000-4000-a000-000000000001', 'VPS 4 vCPU', 'VPS 4 معالج', '4 vCPU, 8GB RAM, 100GB SSD', '4 معالج, 8 جيجابايت رام, 100 جيجابايت SSD', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-VPS-004', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000162', 'a0000000-0000-4000-a000-000000000001', 'VPS 8 vCPU', 'VPS 8 معالج', '8 vCPU, 16GB RAM, 200GB SSD', '8 معالج, 16 جيجابايت رام, 200 جيجابايت SSD', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-VPS-008', NOW(), NOW()),
-- Colocation
('b0000000-0000-4000-a000-000000000163', 'a0000000-0000-4000-a000-000000000001', 'Colocation Half Rack', 'مساحة نصف رف', 'Half rack colocation - 10U, 500W power, 100Mbps bandwidth', 'مساحة نصف رف - 10U, 500 واط طاقة, 100 ميجابت نطاق', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-COL-HR', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000164', 'a0000000-0000-4000-a000-000000000001', 'Colocation Full Rack', 'مساحة رف كامل', 'Full rack colocation - 20U, 1000W power, 200Mbps bandwidth', 'مساحة رف كامل - 20U, 1000 واط طاقة, 200 ميجابت نطاق', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-COL-FR', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000165', 'a0000000-0000-4000-a000-000000000001', 'Colocation Cabinet', 'خزانة مشاركة', 'Private cabinet colocation - 42U, 3000W power, 1Gbps bandwidth', 'خزانة مشاركة خاصة - 42U, 3000 واط طاقة, 1 جيجابت نطاق', 'a0000000-0000-4000-a000-000000000323', 'INFRASTRUCTURE', true, false, true, 'Active', 'BIZ-COL-CAB', NOW(), NOW()),
-- Web Hosting
('b0000000-0000-4000-a000-000000000166', 'a0000000-0000-4000-a000-000000000001', 'Web Hosting Basic', 'استضافة مواقع أساسية', '5GB disk, 50GB bandwidth, 1 domain', '5 جيجابايت تخزين, 50 جيجابايت نطاق, نطاق واحد', 'a0000000-0000-4000-a000-000000000323', 'HOSTING', true, false, true, 'Active', 'BIZ-WEB-BAS', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000167', 'a0000000-0000-4000-a000-000000000001', 'Web Hosting Standard', 'استضافة مواقع قياسية', '20GB disk, 200GB bandwidth, 5 domains', '20 جيجابايت تخزين, 200 جيجابايت نطاق, 5 نطاقات', 'a0000000-0000-4000-a000-000000000323', 'HOSTING', true, false, true, 'Active', 'BIZ-WEB-STD', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000168', 'a0000000-0000-4000-a000-000000000001', 'Web Hosting Premium', 'استضافة مواقع ممتازة', '100GB disk, 1TB bandwidth, unlimited domains', '100 جيجابايت تخزين, 1 تيرابايت نطاق, نطاقات غير محدودة', 'a0000000-0000-4000-a000-000000000323', 'HOSTING', true, false, true, 'Active', 'BIZ-WEB-PRM', NOW(), NOW()),
-- Domain Registration
('b0000000-0000-4000-a000-000000000169', 'a0000000-0000-4000-a000-000000000001', 'Domain Registration .com', 'تسجيل نطاق .com', 'Domain name registration - .com extension', 'تسجيل اسم نطاق - امتداد .com', 'a0000000-0000-4000-a000-000000000324', 'HOSTING', true, false, true, 'Active', 'BIZ-DOM-COM', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000170', 'a0000000-0000-4000-a000-000000000001', 'Domain Registration .net', 'تسجيل نطاق .net', 'Domain name registration - .net extension', 'تسجيل اسم نطاق - امتداد .net', 'a0000000-0000-4000-a000-000000000324', 'HOSTING', true, false, true, 'Active', 'BIZ-DOM-NET', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000171', 'a0000000-0000-4000-a000-000000000001', 'Domain Registration .org', 'تسجيل نطاق .org', 'Domain name registration - .org extension', 'تسجيل اسم نطاق - امتداد .org', 'a0000000-0000-4000-a000-000000000324', 'HOSTING', true, false, true, 'Active', 'BIZ-DOM-ORG', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000172', 'a0000000-0000-4000-a000-000000000001', 'Domain Registration .ye', 'تسجيل نطاق .ye', 'Domain name registration - .ye (Yemen) extension', 'تسجيل اسم نطاق - امتداد .ye (اليمن)', 'a0000000-0000-4000-a000-000000000324', 'HOSTING', true, false, true, 'Active', 'BIZ-DOM-YE', NOW(), NOW()),
-- ATM Connectivity
('b0000000-0000-4000-a000-000000000173', 'a0000000-0000-4000-a000-000000000001', 'ATM Connectivity MPLS 2Mbps', 'ربط صراف آلي MPLS 2 ميجابت', 'MPLS-based ATM connectivity - 2Mbps, SLA 99.99%', 'ربط صراف آلي عبر MPLS - 2 ميجابت, SLA 99.99%', 'a0000000-0000-4000-a000-000000000324', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-ATM-MPLS-2', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000174', 'a0000000-0000-4000-a000-000000000001', 'ATM Connectivity MPLS 5Mbps', 'ربط صراف آلي MPLS 5 ميجابت', 'MPLS-based ATM connectivity - 5Mbps, SLA 99.99%', 'ربط صراف آلي عبر MPLS - 5 ميجابت, SLA 99.99%', 'a0000000-0000-4000-a000-000000000324', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-ATM-MPLS-5', NOW(), NOW()),
('b0000000-0000-4000-a000-000000000175', 'a0000000-0000-4000-a000-000000000001', 'ATM Connectivity Ethernet 10Mbps', 'ربط صراف آلي إيثرنت 10 ميجابت', 'Ethernet-based ATM connectivity - 10Mbps, SLA 99.99%', 'ربط صراف آلي عبر إيثرنت - 10 ميجابت, SLA 99.99%', 'a0000000-0000-4000-a000-000000000324', 'CONNECTIVITY', true, false, true, 'Active', 'BIZ-ATM-ETH-10', NOW(), NOW())
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 7: Offers with Pricing
-- =========================================================================

-- Super Shamel Bundle Offer

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES (
    'c0000000-0000-4000-a000-000000000001',
    'a0000000-0000-4000-a000-000000000001',
    'Super Shamel Monthly',
    'سوبر شامل شهري',
    'All-in-one bundle: FTTH 100Mbps + Hatif Tawasol 500min + Yemen 4G 10GB',
    'باقة شاملة: FTTH 100 ميجابت + هاتفي تواصل 500 دقيقة + يمن 4G 10 جيجابايت',
    'BUNDLE', true, false, 'Monthly', true, 1, NOW(), NOW()
) ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name, description)
VALUES (
    'd0000000-0000-4000-a000-000000000001',
    'c0000000-0000-4000-a000-000000000001',
    'RECURRING', 'YER', 20000.00, 0, 0, 'Monthly',
    true, 'Super Shamel Monthly Price',
    'Discounted bundle price - save 5000 YER vs individual services'
) ON CONFLICT DO NOTHING;

-- FTTH Residential Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000011', 'a0000000-0000-4000-a000-000000000001', 'FTTH 20Mbps Monthly', 'FTTH 20 ميجابت شهري', 'Fiber optic broadband - 20Mbps', 'إنترنت ألياف ضوئية - 20 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000012', 'a0000000-0000-4000-a000-000000000001', 'FTTH 50Mbps Monthly', 'FTTH 50 ميجابت شهري', 'Fiber optic broadband - 50Mbps', 'إنترنت ألياف ضوئية - 50 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000013', 'a0000000-0000-4000-a000-000000000001', 'FTTH 100Mbps Monthly', 'FTTH 100 ميجابت شهري', 'Fiber optic broadband - 100Mbps', 'إنترنت ألياف ضوئية - 100 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000014', 'a0000000-0000-4000-a000-000000000001', 'FTTH 200Mbps Monthly', 'FTTH 200 ميجابت شهري', 'Fiber optic broadband - 200Mbps', 'إنترنت ألياف ضوئية - 200 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 4, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000015', 'a0000000-0000-4000-a000-000000000001', 'FTTH 500Mbps Monthly', 'FTTH 500 ميجابت شهري', 'Fiber optic broadband - 500Mbps', 'إنترنت ألياف ضوئية - 500 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 5, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000011', 'c0000000-0000-4000-a000-000000000011', 'RECURRING', 'YER', 15000, 0, 0, 'Monthly', true, 'FTTH 20Mbps'),
('d0000000-0000-4000-a000-000000000012', 'c0000000-0000-4000-a000-000000000012', 'RECURRING', 'YER', 25000, 0, 0, 'Monthly', true, 'FTTH 50Mbps'),
('d0000000-0000-4000-a000-000000000013', 'c0000000-0000-4000-a000-000000000013', 'RECURRING', 'YER', 35000, 0, 0, 'Monthly', true, 'FTTH 100Mbps'),
('d0000000-0000-4000-a000-000000000014', 'c0000000-0000-4000-a000-000000000014', 'RECURRING', 'YER', 55000, 0, 0, 'Monthly', true, 'FTTH 200Mbps'),
('d0000000-0000-4000-a000-000000000015', 'c0000000-0000-4000-a000-000000000015', 'RECURRING', 'YER', 75000, 0, 0, 'Monthly', true, 'FTTH 500Mbps')
ON CONFLICT DO NOTHING;

-- Hatif Tawasol Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000021', 'a0000000-0000-4000-a000-000000000001', 'Hatif Tawasol 500 Monthly', 'هاتفي تواصل 500 شهري', 'VoIP telephony - 500 minutes', 'هاتف VoIP - 500 دقيقة', 'SUBSCRIPTION', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000022', 'a0000000-0000-4000-a000-000000000001', 'Hatif Tawasol 1000 Monthly', 'هاتفي تواصل 1000 شهري', 'VoIP telephony - 1000 minutes', 'هاتف VoIP - 1000 دقيقة', 'SUBSCRIPTION', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000023', 'a0000000-0000-4000-a000-000000000001', 'Hatif Tawasol Unlimited Monthly', 'هاتفي تواصل غير محدود شهري', 'VoIP telephony - unlimited local minutes', 'هاتف VoIP - دقائق محلية غير محدودة', 'SUBSCRIPTION', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000021', 'c0000000-0000-4000-a000-000000000021', 'RECURRING', 'YER', 3000, 0, 0, 'Monthly', true, 'Hatif Tawasol 500'),
('d0000000-0000-4000-a000-000000000022', 'c0000000-0000-4000-a000-000000000022', 'RECURRING', 'YER', 5000, 0, 0, 'Monthly', true, 'Hatif Tawasol 1000'),
('d0000000-0000-4000-a000-000000000023', 'c0000000-0000-4000-a000-000000000023', 'RECURRING', 'YER', 8000, 0, 0, 'Monthly', true, 'Hatif Tawasol Unlimited')
ON CONFLICT DO NOTHING;

-- Supernet ADSL Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000031', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 4Mbps Monthly', 'سوبرنت ADSL 4 ميجابت شهري', 'ADSL broadband - 4Mbps', 'إنترنت ADSL - 4 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000032', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 8Mbps Monthly', 'سوبرنت ADSL 8 ميجابت شهري', 'ADSL broadband - 8Mbps', 'إنترنت ADSL - 8 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000033', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 16Mbps Monthly', 'سوبرنت ADSL 16 ميجابت شهري', 'ADSL broadband - 16Mbps', 'إنترنت ADSL - 16 ميجابت', 'SUBSCRIPTION', true, false, 'Monthly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000034', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL 20Mbps Monthly', 'سوبرنت ADSL 20 ميجابت شهري', 'ADSL broadband - 20Mbps (ADSL2+)', 'إنترنت ADSL - 20 ميجابت (ADSL2+)', 'SUBSCRIPTION', true, false, 'Monthly', true, 4, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000031', 'c0000000-0000-4000-a000-000000000031', 'RECURRING', 'YER', 7000, 0, 0, 'Monthly', true, 'ADSL 4Mbps'),
('d0000000-0000-4000-a000-000000000032', 'c0000000-0000-4000-a000-000000000032', 'RECURRING', 'YER', 10000, 0, 0, 'Monthly', true, 'ADSL 8Mbps'),
('d0000000-0000-4000-a000-000000000033', 'c0000000-0000-4000-a000-000000000033', 'RECURRING', 'YER', 15000, 0, 0, 'Monthly', true, 'ADSL 16Mbps'),
('d0000000-0000-4000-a000-000000000034', 'c0000000-0000-4000-a000-000000000034', 'RECURRING', 'YER', 20000, 0, 0, 'Monthly', true, 'ADSL 20Mbps')
ON CONFLICT DO NOTHING;

-- Yemen 4G Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000041', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Daily 1GB', 'يمن فور جي يومي 1 جيجابايت', 'Daily data - 1GB 4G', 'بيانات يومية - 1 جيجابايت 4G', 'PREPAID', true, false, 'OneTime', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000042', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Weekly 5GB', 'يمن فور جي أسبوعي 5 جيجابايت', 'Weekly data - 5GB 4G', 'بيانات أسبوعية - 5 جيجابايت 4G', 'PREPAID', true, false, 'OneTime', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000043', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Monthly 10GB', 'يمن فور جي شهري 10 جيجابايت', 'Monthly data - 10GB 4G', 'بيانات شهرية - 10 جيجابايت 4G', 'PREPAID', true, false, 'Monthly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000044', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Monthly 30GB', 'يمن فور جي شهري 30 جيجابايت', 'Monthly data - 30GB 4G', 'بيانات شهرية - 30 جيجابايت 4G', 'PREPAID', true, false, 'Monthly', true, 4, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000045', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G Monthly 100GB', 'يمن فور جي شهري 100 جيجابايت', 'Monthly data - 100GB 4G', 'بيانات شهرية - 100 جيجابايت 4G', 'PREPAID', true, false, 'Monthly', true, 5, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000041', 'c0000000-0000-4000-a000-000000000041', 'ONE_TIME', 'YER', 0, 1000, 0, 'Daily', true, '4G Daily 1GB'),
('d0000000-0000-4000-a000-000000000042', 'c0000000-0000-4000-a000-000000000042', 'ONE_TIME', 'YER', 0, 3000, 0, 'Weekly', true, '4G Weekly 5GB'),
('d0000000-0000-4000-a000-000000000043', 'c0000000-0000-4000-a000-000000000043', 'RECURRING', 'YER', 5000, 0, 0, 'Monthly', true, '4G Monthly 10GB'),
('d0000000-0000-4000-a000-000000000044', 'c0000000-0000-4000-a000-000000000044', 'RECURRING', 'YER', 10000, 0, 0, 'Monthly', true, '4G Monthly 30GB'),
('d0000000-0000-4000-a000-000000000045', 'c0000000-0000-4000-a000-000000000045', 'RECURRING', 'YER', 15000, 0, 0, 'Monthly', true, '4G Monthly 100GB')
ON CONFLICT DO NOTHING;

-- Home Wireless 4G Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000051', 'a0000000-0000-4000-a000-000000000001', 'Home Wireless 4G 50GB Monthly', 'لاسلكي منزلي 4G 50 جيجابايت شهري', 'Fixed wireless 4G - 50GB monthly', 'لاسلكي ثابت 4G - 50 جيجابايت شهرياً', 'SUBSCRIPTION', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000052', 'a0000000-0000-4000-a000-000000000001', 'Home Wireless 4G 100GB Monthly', 'لاسلكي منزلي 4G 100 جيجابايت شهري', 'Fixed wireless 4G - 100GB monthly', 'لاسلكي ثابت 4G - 100 جيجابايت شهرياً', 'SUBSCRIPTION', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000053', 'a0000000-0000-4000-a000-000000000001', 'Home Wireless 4G Unlimited Monthly', 'لاسلكي منزلي 4G غير محدود شهري', 'Fixed wireless 4G - unlimited monthly', 'لاسلكي ثابت 4G - غير محدود شهرياً', 'SUBSCRIPTION', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000051', 'c0000000-0000-4000-a000-000000000051', 'RECURRING', 'YER', 8000, 0, 0, 'Monthly', true, 'Home Wireless 50GB'),
('d0000000-0000-4000-a000-000000000052', 'c0000000-0000-4000-a000-000000000052', 'RECURRING', 'YER', 12000, 0, 0, 'Monthly', true, 'Home Wireless 100GB'),
('d0000000-0000-4000-a000-000000000053', 'c0000000-0000-4000-a000-000000000053', 'RECURRING', 'YER', 20000, 0, 0, 'Monthly', true, 'Home Wireless Unlimited')
ON CONFLICT DO NOTHING;

-- Supplementary Telephone Services Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000061', 'a0000000-0000-4000-a000-000000000001', 'Call Forwarding', 'تحويل المكالمات', 'Forward calls to another number', 'تحويل المكالمات', 'VAS', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000062', 'a0000000-0000-4000-a000-000000000001', 'Call Waiting', 'انتظار المكالمات', 'Call waiting service', 'انتظار المكالمات', 'VAS', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000063', 'a0000000-0000-4000-a000-000000000001', 'Caller ID', 'معرف المتصل', 'Display caller number', 'عرض رقم المتصل', 'VAS', true, false, 'Monthly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000064', 'a0000000-0000-4000-a000-000000000001', 'Voicemail', 'البريد الصوتي', 'Personal voicemail box', 'صندوق بريد صوتي شخصي', 'VAS', true, false, 'Monthly', true, 4, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000065', 'a0000000-0000-4000-a000-000000000001', 'Call Barring', 'حظر المكالمات', 'Restrict calls', 'تقييد المكالمات', 'VAS', true, false, 'Monthly', true, 5, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000061', 'c0000000-0000-4000-a000-000000000061', 'RECURRING', 'YER', 500, 0, 0, 'Monthly', true, 'Call Forwarding'),
('d0000000-0000-4000-a000-000000000062', 'c0000000-0000-4000-a000-000000000062', 'RECURRING', 'YER', 500, 0, 0, 'Monthly', true, 'Call Waiting'),
('d0000000-0000-4000-a000-000000000063', 'c0000000-0000-4000-a000-000000000063', 'RECURRING', 'YER', 300, 0, 0, 'Monthly', true, 'Caller ID'),
('d0000000-0000-4000-a000-000000000064', 'c0000000-0000-4000-a000-000000000064', 'RECURRING', 'YER', 1000, 0, 0, 'Monthly', true, 'Voicemail'),
('d0000000-0000-4000-a000-000000000065', 'c0000000-0000-4000-a000-000000000065', 'RECURRING', 'YER', 500, 0, 0, 'Monthly', true, 'Call Barring')
ON CONFLICT DO NOTHING;

-- Hatif Fawtara Offer

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES (
    'c0000000-0000-4000-a000-000000000071',
    'a0000000-0000-4000-a000-000000000001',
    'Hatif Fawtara Standard Monthly',
    'هاتفي فوترة قياسي شهري',
    'Postpaid fixed telephone line with standard monthly rental',
    'خط هاتف ثابت بنظام الفوترة مع إيجار شهري قياسي',
    'SUBSCRIPTION', true, false, 'Monthly', true, 1, NOW(), NOW()
) ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES (
    'd0000000-0000-4000-a000-000000000071',
    'c0000000-0000-4000-a000-000000000071',
    'RECURRING', 'YER', 5000, 0, 0, 'Monthly',
    true, 'Hatif Fawtara Standard'
) ON CONFLICT DO NOTHING;

-- Yemen WiFi Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000081', 'a0000000-0000-4000-a000-000000000001', 'Yemen WiFi Daily Pass', 'يمن واي فاي تذكرة يومية', '24-hour WiFi hotspot pass', 'تصريح واي فاي 24 ساعة', 'PREPAID', true, false, 'OneTime', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000082', 'a0000000-0000-4000-a000-000000000001', 'Yemen WiFi Weekly Pass', 'يمن واي فاي تذكرة أسبوعية', '7-day WiFi hotspot pass', 'تصريح واي فاي 7 أيام', 'PREPAID', true, false, 'OneTime', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000083', 'a0000000-0000-4000-a000-000000000001', 'Yemen WiFi Monthly Pass', 'يمن واي فاي تذكرة شهرية', '30-day WiFi hotspot pass', 'تصريح واي فاي 30 يومًا', 'PREPAID', true, false, 'OneTime', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000081', 'c0000000-0000-4000-a000-000000000081', 'ONE_TIME', 'YER', 0, 500, 0, 'Daily', true, 'WiFi Daily'),
('d0000000-0000-4000-a000-000000000082', 'c0000000-0000-4000-a000-000000000082', 'ONE_TIME', 'YER', 0, 2000, 0, 'Weekly', true, 'WiFi Weekly'),
('d0000000-0000-4000-a000-000000000083', 'c0000000-0000-4000-a000-000000000083', 'ONE_TIME', 'YER', 0, 5000, 0, 'Monthly', true, 'WiFi Monthly')
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 7b: Business Offers with Pricing
-- =========================================================================

-- Business FTTH Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000101', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 50Mbps Monthly', 'FTTH للأعمال 50 ميجابت شهري', 'Business fiber - 50Mbps with SLA', 'ألياف للأعمال - 50 ميجابت مع SLA', 'SUBSCRIPTION', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000102', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 100Mbps Monthly', 'FTTH للأعمال 100 ميجابت شهري', 'Business fiber - 100Mbps with SLA', 'ألياف للأعمال - 100 ميجابت مع SLA', 'SUBSCRIPTION', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000103', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 200Mbps Monthly', 'FTTH للأعمال 200 ميجابت شهري', 'Business fiber - 200Mbps with SLA', 'ألياف للأعمال - 200 ميجابت مع SLA', 'SUBSCRIPTION', true, false, 'Monthly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000104', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 500Mbps Monthly', 'FTTH للأعمال 500 ميجابت شهري', 'Business fiber - 500Mbps with SLA', 'ألياف للأعمال - 500 ميجابت مع SLA', 'SUBSCRIPTION', true, false, 'Monthly', true, 4, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000105', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH 1Gbps Monthly', 'FTTH للأعمال 1 جيجابت شهري', 'Business fiber - 1Gbps with SLA', 'ألياف للأعمال - 1 جيجابت مع SLA', 'SUBSCRIPTION', true, false, 'Monthly', true, 5, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000101', 'c0000000-0000-4000-a000-000000000101', 'RECURRING', 'YER', 50000, 0, 0, 'Monthly', true, 'Biz FTTH 50Mbps'),
('d0000000-0000-4000-a000-000000000102', 'c0000000-0000-4000-a000-000000000102', 'RECURRING', 'YER', 75000, 0, 0, 'Monthly', true, 'Biz FTTH 100Mbps'),
('d0000000-0000-4000-a000-000000000103', 'c0000000-0000-4000-a000-000000000103', 'RECURRING', 'YER', 125000, 0, 0, 'Monthly', true, 'Biz FTTH 200Mbps'),
('d0000000-0000-4000-a000-000000000104', 'c0000000-0000-4000-a000-000000000104', 'RECURRING', 'YER', 250000, 0, 0, 'Monthly', true, 'Biz FTTH 500Mbps'),
('d0000000-0000-4000-a000-000000000105', 'c0000000-0000-4000-a000-000000000105', 'RECURRING', 'YER', 500000, 0, 0, 'Monthly', true, 'Biz FTTH 1Gbps')
ON CONFLICT DO NOTHING;

-- DIA Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000111', 'a0000000-0000-4000-a000-000000000001', 'DIA 10Mbps Monthly', 'إنترنت مخصص 10 ميجابت شهري', 'Dedicated symmetric internet - 10Mbps', 'إنترنت مخصص متماثل - 10 ميجابت', 'DIA', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000112', 'a0000000-0000-4000-a000-000000000001', 'DIA 50Mbps Monthly', 'إنترنت مخصص 50 ميجابت شهري', 'Dedicated symmetric internet - 50Mbps', 'إنترنت مخصص متماثل - 50 ميجابت', 'DIA', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000113', 'a0000000-0000-4000-a000-000000000001', 'DIA 100Mbps Monthly', 'إنترنت مخصص 100 ميجابت شهري', 'Dedicated symmetric internet - 100Mbps', 'إنترنت مخصص متماثل - 100 ميجابت', 'DIA', true, false, 'Monthly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000114', 'a0000000-0000-4000-a000-000000000001', 'DIA 500Mbps Monthly', 'إنترنت مخصص 500 ميجابت شهري', 'Dedicated symmetric internet - 500Mbps', 'إنترنت مخصص متماثل - 500 ميجابت', 'DIA', true, false, 'Monthly', true, 4, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000115', 'a0000000-0000-4000-a000-000000000001', 'DIA 1Gbps Monthly', 'إنترنت مخصص 1 جيجابت شهري', 'Dedicated symmetric internet - 1Gbps', 'إنترنت مخصص متماثل - 1 جيجابت', 'DIA', true, false, 'Monthly', true, 5, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000116', 'a0000000-0000-4000-a000-000000000001', 'DIA 10Gbps Monthly', 'إنترنت مخصص 10 جيجابت شهري', 'Dedicated symmetric internet - 10Gbps', 'إنترنت مخصص متماثل - 10 جيجابت', 'DIA', true, false, 'Monthly', true, 6, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000111', 'c0000000-0000-4000-a000-000000000111', 'RECURRING', 'YER', 100000, 0, 0, 'Monthly', true, 'DIA 10Mbps'),
('d0000000-0000-4000-a000-000000000112', 'c0000000-0000-4000-a000-000000000112', 'RECURRING', 'YER', 250000, 0, 0, 'Monthly', true, 'DIA 50Mbps'),
('d0000000-0000-4000-a000-000000000113', 'c0000000-0000-4000-a000-000000000113', 'RECURRING', 'YER', 500000, 0, 0, 'Monthly', true, 'DIA 100Mbps'),
('d0000000-0000-4000-a000-000000000114', 'c0000000-0000-4000-a000-000000000114', 'RECURRING', 'YER', 1500000, 0, 0, 'Monthly', true, 'DIA 500Mbps'),
('d0000000-0000-4000-a000-000000000115', 'c0000000-0000-4000-a000-000000000115', 'RECURRING', 'YER', 2500000, 0, 0, 'Monthly', true, 'DIA 1Gbps'),
('d0000000-0000-4000-a000-000000000116', 'c0000000-0000-4000-a000-000000000116', 'RECURRING', 'YER', 5000000, 0, 0, 'Monthly', true, 'DIA 10Gbps')
ON CONFLICT DO NOTHING;

-- Ethernet Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000121', 'a0000000-0000-4000-a000-000000000001', 'Ethernet P2P 100Mbps Monthly', 'إيثرنت نقطة-لنقطة 100 ميجابت شهري', 'Point-to-point Ethernet - 100Mbps', 'إيثرنت نقطة-لنقطة - 100 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000122', 'a0000000-0000-4000-a000-000000000001', 'Ethernet P2P 1Gbps Monthly', 'إيثرنت نقطة-لنقطة 1 جيجابت شهري', 'Point-to-point Ethernet - 1Gbps', 'إيثرنت نقطة-لنقطة - 1 جيجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000123', 'a0000000-0000-4000-a000-000000000001', 'Ethernet P2MP 100Mbps Monthly', 'إيثرنت نقطة-لعدة نقاط 100 ميجابت شهري', 'Point-to-multipoint Ethernet - 100Mbps/site', 'إيثرنت نقطة-لعدة نقاط - 100 ميجابت/موقع', 'CONNECTIVITY', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000121', 'c0000000-0000-4000-a000-000000000121', 'RECURRING', 'YER', 200000, 0, 0, 'Monthly', true, 'Ethernet P2P 100Mbps'),
('d0000000-0000-4000-a000-000000000122', 'c0000000-0000-4000-a000-000000000122', 'RECURRING', 'YER', 500000, 0, 0, 'Monthly', true, 'Ethernet P2P 1Gbps'),
('d0000000-0000-4000-a000-000000000123', 'c0000000-0000-4000-a000-000000000123', 'RECURRING', 'YER', 150000, 0, 0, 'Monthly', true, 'Ethernet P2MP 100Mbps')
ON CONFLICT DO NOTHING;

-- TDM Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000131', 'a0000000-0000-4000-a000-000000000001', 'E1 Circuit Monthly', 'دائرة E1 شهرياً', 'E1 TDM circuit - 2.048Mbps', 'دائرة E1 - 2.048 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000132', 'a0000000-0000-4000-a000-000000000001', 'STM-1 Circuit Monthly', 'دائرة STM-1 شهرياً', 'STM-1 TDM circuit - 155.52Mbps', 'دائرة STM-1 - 155.52 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000133', 'a0000000-0000-4000-a000-000000000001', 'STM-4 Circuit Monthly', 'دائرة STM-4 شهرياً', 'STM-4 TDM circuit - 622.08Mbps', 'دائرة STM-4 - 622.08 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000131', 'c0000000-0000-4000-a000-000000000131', 'RECURRING', 'YER', 100000, 0, 0, 'Monthly', true, 'E1 Circuit'),
('d0000000-0000-4000-a000-000000000132', 'c0000000-0000-4000-a000-000000000132', 'RECURRING', 'YER', 500000, 0, 0, 'Monthly', true, 'STM-1 Circuit'),
('d0000000-0000-4000-a000-000000000133', 'c0000000-0000-4000-a000-000000000133', 'RECURRING', 'YER', 1500000, 0, 0, 'Monthly', true, 'STM-4 Circuit')
ON CONFLICT DO NOTHING;

-- PRI Offer

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES (
    'c0000000-0000-4000-a000-000000000141',
    'a0000000-0000-4000-a000-000000000001',
    'PRI E1 Trunk Monthly',
    'خط PRI E1 شهري',
    'E1 PRI ISDN trunk - 30 voice channels',
    'خط E1 PRI - 30 قناة صوتية',
    'VOICE', true, false, 'Monthly', true, 1, NOW(), NOW()
) ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES (
    'd0000000-0000-4000-a000-000000000141',
    'c0000000-0000-4000-a000-000000000141',
    'RECURRING', 'YER', 75000, 0, 0, 'Monthly',
    true, 'PRI E1 Trunk'
) ON CONFLICT DO NOTHING;

-- 800 Number Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000151', 'a0000000-0000-4000-a000-000000000001', '800 Free Number Basic Monthly', 'الرقم المجاني 800 أساسي شهري', 'Toll-free 800 - 500 min included', 'رقم 800 مجاني - 500 دقيقة مضمنة', 'VOICE', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000152', 'a0000000-0000-4000-a000-000000000001', '800 Free Number Premium Monthly', 'الرقم المجاني 800 ممتاز شهري', 'Toll-free 800 - 2000 min included', 'رقم 800 مجاني - 2000 دقيقة مضمنة', 'VOICE', true, false, 'Monthly', true, 2, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000151', 'c0000000-0000-4000-a000-000000000151', 'RECURRING', 'YER', 10000, 0, 50, 'Monthly', true, '800 Basic'),
('d0000000-0000-4000-a000-000000000152', 'c0000000-0000-4000-a000-000000000152', 'RECURRING', 'YER', 25000, 0, 50, 'Monthly', true, '800 Premium')
ON CONFLICT DO NOTHING;

-- Static IP Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000161', 'a0000000-0000-4000-a000-000000000001', 'Static IPv4 Monthly', 'عنوان IPv4 ثابت شهري', 'Single static public IPv4', 'عنوان IPv4 عام ثابت واحد', 'VAS', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000162', 'a0000000-0000-4000-a000-000000000001', 'Static IP Block /29 Monthly', 'كتلة عناوين ثابتة /29 شهري', 'Block of 5 static IPv4 addresses', 'كتلة من 5 عناوين IPv4 ثابتة', 'VAS', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000163', 'a0000000-0000-4000-a000-000000000001', 'Static IP Block /28 Monthly', 'كتلة عناوين ثابتة /28 شهري', 'Block of 13 static IPv4 addresses', 'كتلة من 13 عنوان IPv4 ثابت', 'VAS', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000161', 'c0000000-0000-4000-a000-000000000161', 'RECURRING', 'YER', 3000, 0, 0, 'Monthly', true, 'Static IPv4 Single'),
('d0000000-0000-4000-a000-000000000162', 'c0000000-0000-4000-a000-000000000162', 'RECURRING', 'YER', 10000, 0, 0, 'Monthly', true, 'Static IP /29'),
('d0000000-0000-4000-a000-000000000163', 'c0000000-0000-4000-a000-000000000163', 'RECURRING', 'YER', 20000, 0, 0, 'Monthly', true, 'Static IP /28')
ON CONFLICT DO NOTHING;

-- Wireless Transmission Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000171', 'a0000000-0000-4000-a000-000000000001', 'Wireless 4G 10Mbps Monthly', 'نقل لاسلكي 4G 10 ميجابت شهري', 'Wireless transmission - 10Mbps', 'نقل لاسلكي - 10 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000172', 'a0000000-0000-4000-a000-000000000001', 'Wireless 4G 50Mbps Monthly', 'نقل لاسلكي 4G 50 ميجابت شهري', 'Wireless transmission - 50Mbps', 'نقل لاسلكي - 50 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000173', 'a0000000-0000-4000-a000-000000000001', 'Wireless 4G 100Mbps Monthly', 'نقل لاسلكي 4G 100 ميجابت شهري', 'Wireless transmission - 100Mbps', 'نقل لاسلكي - 100 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000171', 'c0000000-0000-4000-a000-000000000171', 'RECURRING', 'YER', 50000, 0, 0, 'Monthly', true, 'Wireless 4G 10Mbps'),
('d0000000-0000-4000-a000-000000000172', 'c0000000-0000-4000-a000-000000000172', 'RECURRING', 'YER', 100000, 0, 0, 'Monthly', true, 'Wireless 4G 50Mbps'),
('d0000000-0000-4000-a000-000000000173', 'c0000000-0000-4000-a000-000000000173', 'RECURRING', 'YER', 200000, 0, 0, 'Monthly', true, 'Wireless 4G 100Mbps')
ON CONFLICT DO NOTHING;

-- Dedicated Server Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000181', 'a0000000-0000-4000-a000-000000000001', 'Dedicated Server Basic Monthly', 'خادم مخصص أساسي شهري', '4 CPU, 8GB RAM, 500GB HDD, 5TB', '4 معالج, 8 جيجا رام, 500 جيجا HDD, 5 تيرا', 'INFRASTRUCTURE', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000182', 'a0000000-0000-4000-a000-000000000001', 'Dedicated Server Standard Monthly', 'خادم مخصص قياسي شهري', '8 CPU, 16GB RAM, 1TB HDD, 10TB', '8 معالج, 16 جيجا رام, 1 تيرا HDD, 10 تيرا', 'INFRASTRUCTURE', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000183', 'a0000000-0000-4000-a000-000000000001', 'Dedicated Server Premium Monthly', 'خادم مخصص ممتاز شهري', '16 CPU, 32GB RAM, 2TB SSD, 20TB', '16 معالج, 32 جيجا رام, 2 تيرا SSD, 20 تيرا', 'INFRASTRUCTURE', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000181', 'c0000000-0000-4000-a000-000000000181', 'RECURRING', 'YER', 50000, 0, 0, 'Monthly', true, 'Dedicated Basic'),
('d0000000-0000-4000-a000-000000000182', 'c0000000-0000-4000-a000-000000000182', 'RECURRING', 'YER', 100000, 0, 0, 'Monthly', true, 'Dedicated Standard'),
('d0000000-0000-4000-a000-000000000183', 'c0000000-0000-4000-a000-000000000183', 'RECURRING', 'YER', 200000, 0, 0, 'Monthly', true, 'Dedicated Premium')
ON CONFLICT DO NOTHING;

-- VPS Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000191', 'a0000000-0000-4000-a000-000000000001', 'VPS 1 vCPU Monthly', 'VPS 1 معالج شهري', '1 vCPU, 1GB RAM, 20GB SSD', '1 معالج, 1 جيجا رام, 20 جيجا SSD', 'INFRASTRUCTURE', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000192', 'a0000000-0000-4000-a000-000000000001', 'VPS 2 vCPU Monthly', 'VPS 2 معالج شهري', '2 vCPU, 4GB RAM, 50GB SSD', '2 معالج, 4 جيجا رام, 50 جيجا SSD', 'INFRASTRUCTURE', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000193', 'a0000000-0000-4000-a000-000000000001', 'VPS 4 vCPU Monthly', 'VPS 4 معالج شهري', '4 vCPU, 8GB RAM, 100GB SSD', '4 معالج, 8 جيجا رام, 100 جيجا SSD', 'INFRASTRUCTURE', true, false, 'Monthly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000194', 'a0000000-0000-4000-a000-000000000001', 'VPS 8 vCPU Monthly', 'VPS 8 معالج شهري', '8 vCPU, 16GB RAM, 200GB SSD', '8 معالج, 16 جيجا رام, 200 جيجا SSD', 'INFRASTRUCTURE', true, false, 'Monthly', true, 4, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000191', 'c0000000-0000-4000-a000-000000000191', 'RECURRING', 'YER', 10000, 0, 0, 'Monthly', true, 'VPS 1 vCPU'),
('d0000000-0000-4000-a000-000000000192', 'c0000000-0000-4000-a000-000000000192', 'RECURRING', 'YER', 20000, 0, 0, 'Monthly', true, 'VPS 2 vCPU'),
('d0000000-0000-4000-a000-000000000193', 'c0000000-0000-4000-a000-000000000193', 'RECURRING', 'YER', 35000, 0, 0, 'Monthly', true, 'VPS 4 vCPU'),
('d0000000-0000-4000-a000-000000000194', 'c0000000-0000-4000-a000-000000000194', 'RECURRING', 'YER', 50000, 0, 0, 'Monthly', true, 'VPS 8 vCPU')
ON CONFLICT DO NOTHING;

-- Colocation Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000201', 'a0000000-0000-4000-a000-000000000001', 'Colocation Half Rack Monthly', 'مساحة نصف رف شهري', 'Half rack - 10U, 500W, 100Mbps', 'نصف رف - 10U, 500 واط, 100 ميجابت', 'INFRASTRUCTURE', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000202', 'a0000000-0000-4000-a000-000000000001', 'Colocation Full Rack Monthly', 'مساحة رف كامل شهري', 'Full rack - 20U, 1000W, 200Mbps', 'رف كامل - 20U, 1000 واط, 200 ميجابت', 'INFRASTRUCTURE', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000203', 'a0000000-0000-4000-a000-000000000001', 'Colocation Cabinet Monthly', 'خزانة مشاركة شهري', 'Cabinet - 42U, 3000W, 1Gbps', 'خزانة - 42U, 3000 واط, 1 جيجابت', 'INFRASTRUCTURE', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000201', 'c0000000-0000-4000-a000-000000000201', 'RECURRING', 'YER', 100000, 0, 0, 'Monthly', true, 'Colo Half Rack'),
('d0000000-0000-4000-a000-000000000202', 'c0000000-0000-4000-a000-000000000202', 'RECURRING', 'YER', 200000, 0, 0, 'Monthly', true, 'Colo Full Rack'),
('d0000000-0000-4000-a000-000000000203', 'c0000000-0000-4000-a000-000000000203', 'RECURRING', 'YER', 500000, 0, 0, 'Monthly', true, 'Colo Cabinet')
ON CONFLICT DO NOTHING;

-- Web Hosting Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000211', 'a0000000-0000-4000-a000-000000000001', 'Web Hosting Basic Monthly', 'استضافة أساسية شهري', '5GB disk, 50GB bw, 1 domain', '5 جيجا تخزين, 50 جيجا نطاق, نطاق واحد', 'HOSTING', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000212', 'a0000000-0000-4000-a000-000000000001', 'Web Hosting Standard Monthly', 'استضافة قياسية شهري', '20GB disk, 200GB bw, 5 domains', '20 جيجا تخزين, 200 جيجا نطاق, 5 نطاقات', 'HOSTING', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000213', 'a0000000-0000-4000-a000-000000000001', 'Web Hosting Premium Monthly', 'استضافة ممتازة شهري', '100GB disk, 1TB bw, unlimited', '100 جيجا تخزين, 1 تيرا نطاق, غير محدود', 'HOSTING', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000211', 'c0000000-0000-4000-a000-000000000211', 'RECURRING', 'YER', 3000, 0, 0, 'Monthly', true, 'Hosting Basic'),
('d0000000-0000-4000-a000-000000000212', 'c0000000-0000-4000-a000-000000000212', 'RECURRING', 'YER', 7000, 0, 0, 'Monthly', true, 'Hosting Standard'),
('d0000000-0000-4000-a000-000000000213', 'c0000000-0000-4000-a000-000000000213', 'RECURRING', 'YER', 15000, 0, 0, 'Monthly', true, 'Hosting Premium')
ON CONFLICT DO NOTHING;

-- Domain Registration Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000221', 'a0000000-0000-4000-a000-000000000001', 'Domain .com Annual', 'نطاق .com سنوي', 'Domain registration - .com', 'تسجيل نطاق - .com', 'HOSTING', true, false, 'Yearly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000222', 'a0000000-0000-4000-a000-000000000001', 'Domain .net Annual', 'نطاق .net سنوي', 'Domain registration - .net', 'تسجيل نطاق - .net', 'HOSTING', true, false, 'Yearly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000223', 'a0000000-0000-4000-a000-000000000001', 'Domain .org Annual', 'نطاق .org سنوي', 'Domain registration - .org', 'تسجيل نطاق - .org', 'HOSTING', true, false, 'Yearly', true, 3, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000224', 'a0000000-0000-4000-a000-000000000001', 'Domain .ye Annual', 'نطاق .ye سنوي', 'Domain registration - .ye', 'تسجيل نطاق - .ye', 'HOSTING', true, false, 'Yearly', true, 4, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000221', 'c0000000-0000-4000-a000-000000000221', 'RECURRING', 'YER', 0, 5000, 0, 'Yearly', true, 'Domain .com'),
('d0000000-0000-4000-a000-000000000222', 'c0000000-0000-4000-a000-000000000222', 'RECURRING', 'YER', 0, 5000, 0, 'Yearly', true, 'Domain .net'),
('d0000000-0000-4000-a000-000000000223', 'c0000000-0000-4000-a000-000000000223', 'RECURRING', 'YER', 0, 5000, 0, 'Yearly', true, 'Domain .org'),
('d0000000-0000-4000-a000-000000000224', 'c0000000-0000-4000-a000-000000000224', 'RECURRING', 'YER', 0, 5000, 0, 'Yearly', true, 'Domain .ye')
ON CONFLICT DO NOTHING;

-- ATM Connectivity Offers

INSERT INTO offers (id, tenant_id, name, name_ar, description, description_ar, offer_type, is_active, is_contract, billing_period, tax_inclusive, sort_order, created_at, updated_at)
VALUES
('c0000000-0000-4000-a000-000000000231', 'a0000000-0000-4000-a000-000000000001', 'ATM MPLS 2Mbps Monthly', 'ربط صراف آلي MPLS 2 ميجابت شهري', 'ATM connectivity - MPLS 2Mbps', 'ربط صراف آلي - MPLS 2 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 1, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000232', 'a0000000-0000-4000-a000-000000000001', 'ATM MPLS 5Mbps Monthly', 'ربط صراف آلي MPLS 5 ميجابت شهري', 'ATM connectivity - MPLS 5Mbps', 'ربط صراف آلي - MPLS 5 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 2, NOW(), NOW()),
('c0000000-0000-4000-a000-000000000233', 'a0000000-0000-4000-a000-000000000001', 'ATM Ethernet 10Mbps Monthly', 'ربط صراف آلي إيثرنت 10 ميجابت شهري', 'ATM connectivity - Ethernet 10Mbps', 'ربط صراف آلي - إيثرنت 10 ميجابت', 'CONNECTIVITY', true, false, 'Monthly', true, 3, NOW(), NOW())
ON CONFLICT DO NOTHING;


INSERT INTO offer_pricings (id, offer_id, pricing_type, currency, recurring_price, one_time_price, usage_price, unit_of_measure, is_active, name)
VALUES
('d0000000-0000-4000-a000-000000000231', 'c0000000-0000-4000-a000-000000000231', 'RECURRING', 'YER', 50000, 0, 0, 'Monthly', true, 'ATM MPLS 2Mbps'),
('d0000000-0000-4000-a000-000000000232', 'c0000000-0000-4000-a000-000000000232', 'RECURRING', 'YER', 75000, 0, 0, 'Monthly', true, 'ATM MPLS 5Mbps'),
('d0000000-0000-4000-a000-000000000233', 'c0000000-0000-4000-a000-000000000233', 'RECURRING', 'YER', 100000, 0, 0, 'Monthly', true, 'ATM Ethernet 10Mbps')
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 8: Product-Offer Relationships
-- =========================================================================

INSERT INTO product_offers (id, product_id, offer_id, is_primary, is_required, created_at)
SELECT
  v.column1::uuid,
  v.column2::uuid,
  v.column3::uuid,
  v.column4::boolean,
  v.column5::boolean,
  v.column6::timestamptz
FROM (VALUES
-- Super Shamel Bundle
('e0000000-0000-4000-a000-000000000001', 'b0000000-0000-4000-a000-000000000100', 'c0000000-0000-4000-a000-000000000001', true, true, NOW()),
-- FTTH Residential
('e0000000-0000-4000-a000-000000000011', 'b0000000-0000-4000-a000-000000000101', 'c0000000-0000-4000-a000-000000000011', true, false, NOW()),
('e0000000-0000-4000-a000-000000000012', 'b0000000-0000-4000-a000-000000000102', 'c0000000-0000-4000-a000-000000000012', true, false, NOW()),
('e0000000-0000-4000-a000-000000000013', 'b0000000-0000-4000-a000-000000000103', 'c0000000-0000-4000-a000-000000000013', true, false, NOW()),
('e0000000-0000-4000-a000-000000000014', 'b0000000-0000-4000-a000-000000000104', 'c0000000-0000-4000-a000-000000000014', true, false, NOW()),
('e0000000-0000-4000-a000-000000000015', 'b0000000-0000-4000-a000-000000000105', 'c0000000-0000-4000-a000-000000000015', true, false, NOW()),
-- Hatif Tawasol
('e0000000-0000-4000-a000-000000000021', 'b0000000-0000-4000-a000-000000000106', 'c0000000-0000-4000-a000-000000000021', true, false, NOW()),
('e0000000-0000-4000-a000-000000000022', 'b0000000-0000-4000-a000-000000000107', 'c0000000-0000-4000-a000-000000000022', true, false, NOW()),
('e0000000-0000-4000-a000-000000000023', 'b0000000-0000-4000-a000-000000000108', 'c0000000-0000-4000-a000-000000000023', true, false, NOW()),
-- ADSL
('e0000000-0000-4000-a000-000000000031', 'b0000000-0000-4000-a000-000000000109', 'c0000000-0000-4000-a000-000000000031', true, false, NOW()),
('e0000000-0000-4000-a000-000000000032', 'b0000000-0000-4000-a000-000000000110', 'c0000000-0000-4000-a000-000000000032', true, false, NOW()),
('e0000000-0000-4000-a000-000000000033', 'b0000000-0000-4000-a000-000000000111', 'c0000000-0000-4000-a000-000000000033', true, false, NOW()),
('e0000000-0000-4000-a000-000000000034', 'b0000000-0000-4000-a000-000000000112', 'c0000000-0000-4000-a000-000000000034', true, false, NOW()),
-- Yemen 4G
('e0000000-0000-4000-a000-000000000041', 'b0000000-0000-4000-a000-000000000113', 'c0000000-0000-4000-a000-000000000041', true, false, NOW()),
('e0000000-0000-4000-a000-000000000042', 'b0000000-0000-4000-a000-000000000114', 'c0000000-0000-4000-a000-000000000042', true, false, NOW()),
('e0000000-0000-4000-a000-000000000043', 'b0000000-0000-4000-a000-000000000115', 'c0000000-0000-4000-a000-000000000043', true, false, NOW()),
('e0000000-0000-4000-a000-000000000044', 'b0000000-0000-4000-a000-000000000116', 'c0000000-0000-4000-a000-000000000044', true, false, NOW()),
('e0000000-0000-4000-a000-000000000045', 'b0000000-0000-4000-a000-000000000117', 'c0000000-0000-4000-a000-000000000045', true, false, NOW()),
-- Home Wireless 4G
('e0000000-0000-4000-a000-000000000051', 'b0000000-0000-4000-a000-000000000118', 'c0000000-0000-4000-a000-000000000051', true, false, NOW()),
('e0000000-0000-4000-a000-000000000052', 'b0000000-0000-4000-a000-000000000119', 'c0000000-0000-4000-a000-000000000052', true, false, NOW()),
('e0000000-0000-4000-a000-000000000053', 'b0000000-0000-4000-a000-000000000120', 'c0000000-0000-4000-a000-000000000053', true, false, NOW()),
-- Supplementary Telephone
('e0000000-0000-4000-a000-000000000061', 'b0000000-0000-4000-a000-000000000121', 'c0000000-0000-4000-a000-000000000061', true, false, NOW()),
('e0000000-0000-4000-a000-000000000062', 'b0000000-0000-4000-a000-000000000122', 'c0000000-0000-4000-a000-000000000062', true, false, NOW()),
('e0000000-0000-4000-a000-000000000063', 'b0000000-0000-4000-a000-000000000123', 'c0000000-0000-4000-a000-000000000063', true, false, NOW()),
('e0000000-0000-4000-a000-000000000064', 'b0000000-0000-4000-a000-000000000124', 'c0000000-0000-4000-a000-000000000064', true, false, NOW()),
('e0000000-0000-4000-a000-000000000065', 'b0000000-0000-4000-a000-000000000125', 'c0000000-0000-4000-a000-000000000065', true, false, NOW()),
-- Hatif Fawtara
('e0000000-0000-4000-a000-000000000071', 'b0000000-0000-4000-a000-000000000126', 'c0000000-0000-4000-a000-000000000071', true, false, NOW()),
-- Yemen WiFi
('e0000000-0000-4000-a000-000000000081', 'b0000000-0000-4000-a000-000000000127', 'c0000000-0000-4000-a000-000000000081', true, false, NOW()),
('e0000000-0000-4000-a000-000000000082', 'b0000000-0000-4000-a000-000000000128', 'c0000000-0000-4000-a000-000000000082', true, false, NOW()),
('e0000000-0000-4000-a000-000000000083', 'b0000000-0000-4000-a000-000000000129', 'c0000000-0000-4000-a000-000000000083', true, false, NOW()),
-- Business FTTH
('e0000000-0000-4000-a000-000000000101', 'b0000000-0000-4000-a000-000000000130', 'c0000000-0000-4000-a000-000000000101', true, false, NOW()),
('e0000000-0000-4000-a000-000000000102', 'b0000000-0000-4000-a000-000000000131', 'c0000000-0000-4000-a000-000000000102', true, false, NOW()),
('e0000000-0000-4000-a000-000000000103', 'b0000000-0000-4000-a000-000000000132', 'c0000000-0000-4000-a000-000000000103', true, false, NOW()),
('e0000000-0000-4000-a000-000000000104', 'b0000000-0000-4000-a000-000000000133', 'c0000000-0000-4000-a000-000000000104', true, false, NOW()),
('e0000000-0000-4000-a000-000000000105', 'b0000000-0000-4000-a000-000000000134', 'c0000000-0000-4000-a000-000000000105', true, false, NOW()),
-- DIA
('e0000000-0000-4000-a000-000000000111', 'b0000000-0000-4000-a000-000000000135', 'c0000000-0000-4000-a000-000000000111', true, false, NOW()),
('e0000000-0000-4000-a000-000000000112', 'b0000000-0000-4000-a000-000000000136', 'c0000000-0000-4000-a000-000000000112', true, false, NOW()),
('e0000000-0000-4000-a000-000000000113', 'b0000000-0000-4000-a000-000000000137', 'c0000000-0000-4000-a000-000000000113', true, false, NOW()),
('e0000000-0000-4000-a000-000000000114', 'b0000000-0000-4000-a000-000000000138', 'c0000000-0000-4000-a000-000000000114', true, false, NOW()),
('e0000000-0000-4000-a000-000000000115', 'b0000000-0000-4000-a000-000000000139', 'c0000000-0000-4000-a000-000000000115', true, false, NOW()),
('e0000000-0000-4000-a000-000000000116', 'b0000000-0000-4000-a000-000000000140', 'c0000000-0000-4000-a000-000000000116', true, false, NOW()),
-- Ethernet
('e0000000-0000-4000-a000-000000000121', 'b0000000-0000-4000-a000-000000000141', 'c0000000-0000-4000-a000-000000000121', true, false, NOW()),
('e0000000-0000-4000-a000-000000000122', 'b0000000-0000-4000-a000-000000000142', 'c0000000-0000-4000-a000-000000000122', true, false, NOW()),
('e0000000-0000-4000-a000-000000000123', 'b0000000-0000-4000-a000-000000000143', 'c0000000-0000-4000-a000-000000000123', true, false, NOW()),
-- TDM
('e0000000-0000-4000-a000-000000000131', 'b0000000-0000-4000-a000-000000000144', 'c0000000-0000-4000-a000-000000000131', true, false, NOW()),
('e0000000-0000-4000-a000-000000000132', 'b0000000-0000-4000-a000-000000000145', 'c0000000-0000-4000-a000-000000000132', true, false, NOW()),
('e0000000-0000-4000-a000-000000000133', 'b0000000-0000-4000-a000-000000000146', 'c0000000-0000-4000-a000-000000000133', true, false, NOW()),
-- PRI
('e0000000-0000-4000-a000-000000000141', 'b0000000-0000-4000-a000-000000000147', 'c0000000-0000-4000-a000-000000000141', true, false, NOW()),
-- 800 Number
('e0000000-0000-4000-a000-000000000151', 'b0000000-0000-4000-a000-000000000148', 'c0000000-0000-4000-a000-000000000151', true, false, NOW()),
('e0000000-0000-4000-a000-000000000152', 'b0000000-0000-4000-a000-000000000149', 'c0000000-0000-4000-a000-000000000152', true, false, NOW()),
-- Static IP
('e0000000-0000-4000-a000-000000000161', 'b0000000-0000-4000-a000-000000000150', 'c0000000-0000-4000-a000-000000000161', true, false, NOW()),
('e0000000-0000-4000-a000-000000000162', 'b0000000-0000-4000-a000-000000000151', 'c0000000-0000-4000-a000-000000000162', true, false, NOW()),
('e0000000-0000-4000-a000-000000000163', 'b0000000-0000-4000-a000-000000000152', 'c0000000-0000-4000-a000-000000000163', true, false, NOW()),
-- Wireless Transmission
('e0000000-0000-4000-a000-000000000171', 'b0000000-0000-4000-a000-000000000153', 'c0000000-0000-4000-a000-000000000171', true, false, NOW()),
('e0000000-0000-4000-a000-000000000172', 'b0000000-0000-4000-a000-000000000154', 'c0000000-0000-4000-a000-000000000172', true, false, NOW()),
('e0000000-0000-4000-a000-000000000173', 'b0000000-0000-4000-a000-000000000155', 'c0000000-0000-4000-a000-000000000173', true, false, NOW()),
-- Dedicated Server
('e0000000-0000-4000-a000-000000000181', 'b0000000-0000-4000-a000-000000000156', 'c0000000-0000-4000-a000-000000000181', true, false, NOW()),
('e0000000-0000-4000-a000-000000000182', 'b0000000-0000-4000-a000-000000000157', 'c0000000-0000-4000-a000-000000000182', true, false, NOW()),
('e0000000-0000-4000-a000-000000000183', 'b0000000-0000-4000-a000-000000000158', 'c0000000-0000-4000-a000-000000000183', true, false, NOW()),
-- VPS
('e0000000-0000-4000-a000-000000000191', 'b0000000-0000-4000-a000-000000000159', 'c0000000-0000-4000-a000-000000000191', true, false, NOW()),
('e0000000-0000-4000-a000-000000000192', 'b0000000-0000-4000-a000-000000000160', 'c0000000-0000-4000-a000-000000000192', true, false, NOW()),
('e0000000-0000-4000-a000-000000000193', 'b0000000-0000-4000-a000-000000000161', 'c0000000-0000-4000-a000-000000000193', true, false, NOW()),
('e0000000-0000-4000-a000-000000000194', 'b0000000-0000-4000-a000-000000000162', 'c0000000-0000-4000-a000-000000000194', true, false, NOW()),
-- Colocation
('e0000000-0000-4000-a000-000000000201', 'b0000000-0000-4000-a000-000000000163', 'c0000000-0000-4000-a000-000000000201', true, false, NOW()),
('e0000000-0000-4000-a000-000000000202', 'b0000000-0000-4000-a000-000000000164', 'c0000000-0000-4000-a000-000000000202', true, false, NOW()),
('e0000000-0000-4000-a000-000000000203', 'b0000000-0000-4000-a000-000000000165', 'c0000000-0000-4000-a000-000000000203', true, false, NOW()),
-- Web Hosting
('e0000000-0000-4000-a000-000000000211', 'b0000000-0000-4000-a000-000000000166', 'c0000000-0000-4000-a000-000000000211', true, false, NOW()),
('e0000000-0000-4000-a000-000000000212', 'b0000000-0000-4000-a000-000000000167', 'c0000000-0000-4000-a000-000000000212', true, false, NOW()),
('e0000000-0000-4000-a000-000000000213', 'b0000000-0000-4000-a000-000000000168', 'c0000000-0000-4000-a000-000000000213', true, false, NOW()),
-- Domain
('e0000000-0000-4000-a000-000000000221', 'b0000000-0000-4000-a000-000000000169', 'c0000000-0000-4000-a000-000000000221', true, false, NOW()),
('e0000000-0000-4000-a000-000000000222', 'b0000000-0000-4000-a000-000000000170', 'c0000000-0000-4000-a000-000000000222', true, false, NOW()),
('e0000000-0000-4000-a000-000000000223', 'b0000000-0000-4000-a000-000000000171', 'c0000000-0000-4000-a000-000000000223', true, false, NOW()),
('e0000000-0000-4000-a000-000000000224', 'b0000000-0000-4000-a000-000000000172', 'c0000000-0000-4000-a000-000000000224', true, false, NOW()),
-- ATM
('e0000000-0000-4000-a000-000000000231', 'b0000000-0000-4000-a000-000000000173', 'c0000000-0000-4000-a000-000000000231', true, false, NOW()),
('e0000000-0000-4000-a000-000000000232', 'b0000000-0000-4000-a000-000000000174', 'c0000000-0000-4000-a000-000000000232', true, false, NOW()),
('e0000000-0000-4000-a000-000000000233', 'b0000000-0000-4000-a000-000000000175', 'c0000000-0000-4000-a000-000000000233', true, false, NOW())
) AS v
WHERE NOT EXISTS (SELECT 1 FROM product_offers WHERE id = v.column1::uuid);

-- =========================================================================
-- PART 9: Super Shamel Bundle - Component Offerings
-- =========================================================================

INSERT INTO bundled_product_offerings (id, offer_id, bundled_offer_id, name, quantity, referral_type)
VALUES (
    'f0000000-0000-4000-a000-000000000001',
    'c0000000-0000-4000-a000-000000000001',
    'c0000000-0000-4000-a000-000000000013',
    'FTTH 100Mbps', 1, 'included'
),
(
    'f0000000-0000-4000-a000-000000000002',
    'c0000000-0000-4000-a000-000000000001',
    'c0000000-0000-4000-a000-000000000021',
    'Hatif Tawasol 500', 1, 'included'
),
(
    'f0000000-0000-4000-a000-000000000003',
    'c0000000-0000-4000-a000-000000000001',
    'c0000000-0000-4000-a000-000000000043',
    'Yemen 4G Monthly 10GB', 1, 'included'
)
ON CONFLICT DO NOTHING;

-- =========================================================================
-- PART 10: Service-Spec-to-Product-Spec Links (via service_specification_id)
-- =========================================================================
-- Link service specifications to product specifications
-- Note: requires catalog_product_specifications table from ProductCatalog module
-- This is optional - creates product specs linked to service specs

-- Create corresponding product specifications for each service spec

INSERT INTO catalog_product_specifications (id, tenant_id, name, description, brand, version, product_number, lifecycle_status, service_specification_id, created_at, updated_at)
SELECT
  v.column1::uuid,
  v.column2::uuid,
  v.column3,
  v.column4,
  v.column5,
  v.column6,
  v.column7,
  v.column8,
  v.column9::uuid,
  v.column10,
  v.column11
FROM (VALUES
    ('a0000000-0000-4000-a000-000000000401', 'a0000000-0000-4000-a000-000000000001', 'Super Shamel', 'Super Shamel bundle product specification', 'PTC Yemen', '1.0', 'SPEC-RES-BND-001', 'Active', 'a0000000-0000-4000-a000-000000000100', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000402', 'a0000000-0000-4000-a000-000000000001', 'FTTH Residential', 'FTTH residential product specification', 'Huawei', '1.0', 'SPEC-RES-FTH-001', 'Active', 'a0000000-0000-4000-a000-000000000101', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000403', 'a0000000-0000-4000-a000-000000000001', 'Hatif Tawasol', 'Hatif Tawasol VoIP product specification', 'PTC Yemen', '1.0', 'SPEC-RES-HTF-001', 'Active', 'a0000000-0000-4000-a000-000000000102', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000404', 'a0000000-0000-4000-a000-000000000001', 'Supernet ADSL', 'Supernet ADSL product specification', 'Huawei', '1.0', 'SPEC-RES-ADL-001', 'Active', 'a0000000-0000-4000-a000-000000000103', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000405', 'a0000000-0000-4000-a000-000000000001', 'Yemen 4G', 'Yemen 4G product specification', 'Huawei', '1.0', 'SPEC-RES-4G-001', 'Active', 'a0000000-0000-4000-a000-000000000104', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000406', 'a0000000-0000-4000-a000-000000000001', 'Business FTTH', 'Business FTTH product specification', 'Huawei', '1.0', 'SPEC-BIZ-FTH-001', 'Active', 'a0000000-0000-4000-a000-000000000110', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000407', 'a0000000-0000-4000-a000-000000000001', 'DIA', 'Dedicated Internet Access product specification', 'PTC Yemen', '1.0', 'SPEC-BIZ-DIA-001', 'Active', 'a0000000-0000-4000-a000-000000000111', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000408', 'a0000000-0000-4000-a000-000000000001', 'Dedicated Server', 'Dedicated server hosting product specification', 'Huawei', '1.0', 'SPEC-BIZ-DSR-001', 'Active', 'a0000000-0000-4000-a000-000000000118', NOW(), NOW()),
    ('a0000000-0000-4000-a000-000000000409', 'a0000000-0000-4000-a000-000000000001', 'VPS', 'Virtual Private Server product specification', 'Huawei', '1.0', 'SPEC-BIZ-VPS-001', 'Active', 'a0000000-0000-4000-a000-000000000119', NOW(), NOW())
) AS v
WHERE NOT EXISTS (SELECT 1 FROM catalog_product_specifications WHERE id = v.column1::uuid);

COMMIT;
