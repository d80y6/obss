using System.Text.RegularExpressions;
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Services;

public sealed partial class BusinessServiceQualificationService : IBusinessServiceQualificationService
{
    private readonly ICoverageAreaRepository _coverageRepository;
    private readonly INetworkElementRepository _networkElementRepository;

    public BusinessServiceQualificationService(
        ICoverageAreaRepository coverageRepository,
        INetworkElementRepository networkElementRepository)
    {
        _coverageRepository = coverageRepository;
        _networkElementRepository = networkElementRepository;
    }

    [GeneratedRegex(@"^(?=.{1,253}$)(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)\.)+(?:[a-zA-Z]{2,63})$")]
    private static partial Regex DomainNameRegex();

    public async Task<BusinessServiceQualificationResult> QualifyAsync(
        BusinessServiceQualificationRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();

        if (request.BusinessServiceType is "DIA" or "Ethernet" or "CloudConnect" && request.BandwidthMbps is null)
        {
            return new BusinessServiceQualificationResult(
                false,
                correlationId,
                $"Bandwidth is required for {request.BusinessServiceType} service qualification.",
                $"عرض النطاق مطلوب لتأهيل خدمة {request.BusinessServiceType}.",
                null,
                null,
                null,
                null,
                null);
        }

        if (request.BusinessServiceType == "Colocation" && request.DomainName is not null)
        {
            return new BusinessServiceQualificationResult(
                false,
                correlationId,
                "Domain name is not applicable for colocation service.",
                "اسم النطاق لا ينطبق على خدمة الاستضافة المشتركة.",
                null,
                null,
                null,
                null,
                null);
        }

        var address = GeographicAddress.Create(
            request.Address,
            request.City ?? string.Empty,
            request.State,
            null,
            "SA");

        var coverageAreas = await _coverageRepository.GetByAddressAsync(address, ct);
        var hasFiber = false;
        var hasCopper = false;

        if (coverageAreas.Count > 0)
        {
            var services = coverageAreas.SelectMany(ca => ca.AvailableServices).Where(s => s.IsActive).ToList();
            hasFiber = services.Any(s => s.Technology.Equals("FTTH", StringComparison.OrdinalIgnoreCase) ||
                                         s.Technology.Equals("Fiber", StringComparison.OrdinalIgnoreCase));
            hasCopper = services.Any(s => s.Technology.Equals("ADSL", StringComparison.OrdinalIgnoreCase) ||
                                          s.Technology.Equals("VDSL", StringComparison.OrdinalIgnoreCase));
        }

        if ((request.BusinessServiceType is "DIA" or "Ethernet" or "CloudConnect") && !hasFiber && !hasCopper)
        {
            return new BusinessServiceQualificationResult(
                false,
                correlationId,
                $"No fiber or copper infrastructure found at this address for {request.BusinessServiceType} service.",
                $"لا توجد بنية تحتية من الألياف الضوئية أو النحاس في هذا العنوان لخدمة {request.BusinessServiceType}.",
                ["LTE Fixed Wireless for business"],
                ["اتصال لاسلكي ثابت عبر LTE للأعمال"],
                null,
                null,
                null);
        }

        if (request.BusinessServiceType is "Colocation" or "DedicatedServer")
        {
            var dataCenterSpaceOk = await CheckDataCenterSpaceAsync(ct);

            if (!dataCenterSpaceOk)
            {
                return new BusinessServiceQualificationResult(
                    false,
                    correlationId,
                    $"No data center space available for {request.BusinessServiceType} service. All racks are fully utilized.",
                    $"لا توجد مساحة متاحة في مركز البيانات لخدمة {request.BusinessServiceType}. جميع الرفوف مستغلة بالكامل.",
                    ["Cloud Connect service", "DIA over fiber"],
                    ["خدمة الاتصال السحابي", "DIA عبر الألياف الضوئية"],
                    null,
                    null,
                    ["Data center capacity exhausted"]);
            }
        }

        var domainNameAvailable = true;
        if (request.DomainName is not null)
        {
            domainNameAvailable = DomainNameRegex().IsMatch(request.DomainName);
            if (!domainNameAvailable)
            {
                return new BusinessServiceQualificationResult(
                    false,
                    correlationId,
                    $"The domain name '{request.DomainName}' is not valid or not available.",
                    $"اسم النطاق '{request.DomainName}' غير صالح أو غير متاح.",
                    null,
                    null,
                    null,
                    null,
                    null);
            }
        }

        var oltAvailable = await FindBusinessOltAsync(request.Latitude, request.Longitude, ct);
        var capacityAvailable = oltAvailable is not null || hasFiber;

        var installationDays = request.BusinessServiceType switch
        {
            "DIA" => 15,
            "Ethernet" => 10,
            "TDM" => 20,
            "Colocation" => 5,
            "DedicatedServer" => 3,
            "CloudConnect" => 7,
            _ => 10
        };

        var (additionalInfo, additionalInfoAr) = request.BusinessServiceType switch
        {
            "DIA" => ("Dedicated Internet Access requires a fiber circuit and static IP block.", "الوصول المخصص للإنترنت يتطلب دائرة ألياف ضوئية وحزمة عناوين IP ثابتة."),
            "Colocation" => ("Space includes power, cooling, and 24/7 physical security.", "المساحة تشمل الطاقة والتبريد والأمن المادي على مدار الساعة."),
            "DedicatedServer" => ("Server hardware, OS license, and managed support included.", "تشمل أجهزة الخادم وترخيص نظام التشغيل والدعم المُدار."),
            _ => (null, null)
        };

        var coverageDetail = new BusinessCoverageDetail(
            hasFiber,
            hasCopper,
            capacityAvailable,
            request.BusinessServiceType is "Colocation" or "DedicatedServer",
            domainNameAvailable,
            installationDays,
            additionalInfo,
            additionalInfoAr);

        var requiredWork = request.BusinessServiceType switch
        {
            "DIA" => new[] { "Fiber circuit installation", "Edge router configuration", "BGP peering setup", "SLA monitoring configuration" },
            "Ethernet" => new[] { "Ethernet circuit provisioning", "VLAN configuration", "CLE code assignment" },
            "TDM" => new[] { "T1/E1 circuit provisioning", "Smartjack installation", "Cross-connect in serving wire center" },
            "Colocation" => new[] { "Rack space allocation", "Power circuit provisioning", "Cross-connect fiber patch" },
            "DedicatedServer" => new[] { "Server racking and stacking", "OS installation and hardening", "Network configuration", "Backup setup" },
            "CloudConnect" => new[] { "Virtual circuit provisioning", "Cloud router configuration", "AWS/Azure/GCP peering setup" },
            _ => new[] { "Service assessment and design" }
        };

        return new BusinessServiceQualificationResult(
            true,
            correlationId,
            $"Address is qualified for {request.BusinessServiceType} business service. Estimated installation: {installationDays} days.",
            $"العنوان مؤهل لخدمة الأعمال {request.BusinessServiceType}. التثبيت المقدر: {installationDays} يومًا.",
            null,
            null,
            coverageDetail,
            requiredWork.ToList(),
            null);
    }

    private async Task<bool> CheckDataCenterSpaceAsync(CancellationToken ct)
    {
        var elements = await _networkElementRepository.GetFilteredAsync(
            "Server", "Active", null, 0, 100, ct);

        return elements.Count < 90;
    }

    private async Task<NetworkElement?> FindBusinessOltAsync(double? lat, double? lon, CancellationToken ct)
    {
        if (lat is null || lon is null)
            return null;

        var elements = await _networkElementRepository.GetFilteredAsync(
            "OLT", "Active", null, 0, 100, ct);

        return elements
            .OfType<NetworkElement>()
            .OrderBy(_ => CalculateDistance(lat.Value, lon.Value))
            .FirstOrDefault();
    }

    private static double CalculateDistance(double lat, double lon)
    {
        _ = lat;
        _ = lon;
        return 0.5;
    }
}
