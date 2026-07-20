namespace Obss.ServiceQualification.Domain.Services;

public sealed class QualificationEngine
{
    private readonly IReadOnlyList<QualificationRule> _rules;

    public QualificationEngine(IEnumerable<QualificationRule> rules)
    {
        _rules = rules.ToList();
    }

    public QualificationRule? GetRule(string serviceType, string segment)
    {
        return _rules.FirstOrDefault(r =>
            r.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase) &&
            r.Segment.Equals(segment, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<QualificationRule> GetRulesForSegment(string segment)
    {
        return _rules
            .Where(r => r.Segment.Equals(segment, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<QualificationRule> GetRulesForServiceType(string serviceType)
    {
        return _rules
            .Where(r => r.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool IsEligible(UnifiedQualificationRequest request)
    {
        var rule = GetRule(request.ServiceType, request.Segment);
        return rule?.EligibilityCheck(request) ?? true;
    }

    public IReadOnlyList<QualificationRule> GetAllRules() => _rules;

    public static QualificationEngine CreateDefault()
    {
        return new QualificationEngine(new List<QualificationRule>
        {
            new(
                "FTTH",
                "residential",
                static req => req.SpeedMbps is null or (<= 1000 and >= 10),
                20,
                10,
                1000,
                false,
                "Fiber-to-the-Home delivers high-speed internet over optical fiber directly to the residence.",
                "الألياف الضوئية إلى المنزل توفر إنترنت عالي السرعة عبر الألياف البصرية مباشرة إلى المسكن."),
            new(
                "FTTH",
                "business",
                static req => req.SpeedMbps is null or (<= 10000 and >= 50),
                20,
                50,
                10000,
                false,
                "Fiber-to-the-Home business service provides dedicated high-speed fiber connectivity.",
                "خدمة الأعمال عبر الألياف الضوئية توفر اتصال ألياف مخصص عالي السرعة."),
            new(
                "ADSL",
                "residential",
                static req => req.SpeedMbps is null or (<= 24 and >= 1),
                5,
                1,
                24,
                false,
                "ADSL delivers internet over existing copper telephone lines.",
                "ADSL يوفر الإنترنت عبر خطوط الهاتف النحاسية الحالية."),
            new(
                "ADSL",
                "business",
                static req => req.SpeedMbps is null or (<= 24 and >= 2),
                5,
                2,
                24,
                false,
                "ADSL business service provides internet over copper lines with static IP option.",
                "خدمة الأعمال ADSL توفر الإنترنت عبر الخطوط النحاسية مع خيار IP ثابت."),
            new(
                "VDSL",
                "residential",
                static req => req.SpeedMbps is null or (<= 100 and >= 10),
                2,
                10,
                100,
                false,
                "VDSL delivers high-speed internet over copper lines with faster speeds than ADSL.",
                "VDSL يوفر إنترنت عالي السرعة عبر الخطوط النحاسية بسرعات أسرع من ADSL."),
            new(
                "VDSL",
                "business",
                static req => req.SpeedMbps is null or (<= 100 and >= 20),
                2,
                20,
                100,
                false,
                "VDSL business service offers high-speed connectivity over copper with SLA options.",
                "خدمة الأعمال VDSL توفر اتصال عالي السرعة عبر النحاس مع خيارات اتفاقية مستوى الخدمة."),
            new(
                "LTE",
                "residential",
                static req => req.SpeedMbps is null or (<= 150 and >= 2),
                null,
                2,
                150,
                true,
                "4G LTE Fixed Wireless delivers internet via cellular network with a rooftop antenna.",
                "الاتصال اللاسلكي الثابت عبر 4G LTE يوفر الإنترنت عبر الشبكة الخلوية بهوائي سطحي."),
            new(
                "LTE",
                "business",
                static req => req.SpeedMbps is null or (<= 150 and >= 5),
                null,
                5,
                150,
                true,
                "4G LTE business wireless provides connectivity with priority data and static IP.",
                "الاتصال اللاسلكي للأعمال عبر 4G LTE يوفر اتصالاً مع أولوية البيانات وIP ثابت."),
            new(
                "WIFI",
                "residential",
                static _ => true,
                null,
                null,
                null,
                false,
                "WiFi hotspot provides internet access in public areas via wireless access points.",
                "نقطة اتصال واي فاي توفر الوصول للإنترنت في المناطق العامة عبر نقاط الوصول اللاسلكية."),
            new(
                "TELEPHONY",
                "residential",
                static req => !string.IsNullOrWhiteSpace(req.TelephoneNumber),
                null,
                null,
                null,
                false,
                "Traditional telephone service (POTS) delivered over copper lines.",
                "خدمة الهاتف التقليدية (POTS) المقدمة عبر الخطوط النحاسية."),
            new(
                "TELEPHONY",
                "business",
                static req => !string.IsNullOrWhiteSpace(req.TelephoneNumber),
                null,
                null,
                null,
                false,
                "Business telephone service with hunt groups, IVR, and PBX integration.",
                "خدمة الهاتف للأعمال مع مجموعات الصيد والرد الصوتي التفاعلي والتكامل مع المقسمات."),
            new(
                "DIA",
                "business",
                static req => req.SpeedMbps is >= 10 and <= 10000,
                null,
                10,
                10000,
                false,
                "Dedicated Internet Access provides symmetric, uncontended bandwidth with SLA.",
                "الوصول المخصص للإنترنت يوفر نطاق ترددي متماثل وغير مشترك مع اتفاقية مستوى خدمة."),
            new(
                "ETHERNET",
                "business",
                static req => req.SpeedMbps is >= 10 and <= 100000,
                null,
                10,
                100000,
                false,
                "Ethernet service provides Layer 2 connectivity between business locations.",
                "خدمة الإيثرنت توفر اتصال من الطبقة الثانية بين مواقع الأعمال."),
            new(
                "COLOCATION",
                "business",
                static _ => true,
                null,
                null,
                null,
                false,
                "Colocation provides secure rack space in carrier-grade data centers.",
                "الاستضافة المشتركة توفر مساحة رفوف آمنة في مراكز بيانات بجودة اتصال عالية."),
            new(
                "DEDICATEDSERVER",
                "business",
                static _ => true,
                null,
                null,
                null,
                false,
                "Dedicated servers provide single-tenant hardware with full root access.",
                "الخوادم المخصصة توفر أجهزة ذات مستأجر واحد مع وصول كامل لجذر النظام."),
            new(
                "CLOUDCONNECT",
                "business",
                static req => req.SpeedMbps is >= 50 and <= 10000,
                null,
                50,
                10000,
                false,
                "Cloud Connect provides direct private connectivity to major cloud providers.",
                "الاتصال السحابي يوفر اتصال خاص مباشر لمزودي الخدمات السحابية الرئيسيين.")
        });
    }
}
