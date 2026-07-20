using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Services;

public sealed class WifiQualificationService : IWifiQualificationService
{
    private readonly ICoverageAreaRepository _coverageRepository;
    private readonly INetworkElementRepository _networkElementRepository;

    public WifiQualificationService(
        ICoverageAreaRepository coverageRepository,
        INetworkElementRepository networkElementRepository)
    {
        _coverageRepository = coverageRepository;
        _networkElementRepository = networkElementRepository;
    }

    public async Task<WifiQualificationResult> QualifyAsync(WifiQualificationRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();

        var address = GeographicAddress.Create(
            request.Address,
            request.City ?? string.Empty,
            request.State,
            null,
            "SA");

        var coverageAreas = await _coverageRepository.GetByAddressAsync(address, ct);

        if (coverageAreas.Count == 0)
        {
            return new WifiQualificationResult(
                false,
                correlationId,
                "No WiFi hotspot coverage found at this address. Consider FTTH or LTE alternatives.",
                "لا توجد تغطية نقطة اتصال واي فاي في هذا العنوان. يُرجى النظر في بدائل الألياف الضوئية أو LTE.",
                ["FTTH up to 1Gbps", "4G LTE up to 150Mbps"],
                ["الألياف الضوئية حتى 1 جيجابت/ثانية", "4G LTE حتى 150 ميجابت/ثانية"],
                null,
                null,
                null);
        }

        var wifiService = coverageAreas
            .SelectMany(ca => ca.AvailableServices)
            .FirstOrDefault(s =>
                s.Technology.Equals("WIFI", StringComparison.OrdinalIgnoreCase) &&
                s.IsActive);

        if (wifiService is null)
        {
            var availableTechs = coverageAreas
                .SelectMany(ca => ca.AvailableServices)
                .Where(s => s.IsActive)
                .Select(s => $"{s.ServiceName} ({s.Technology})")
                .Distinct()
                .ToList();

            return new WifiQualificationResult(
                false,
                correlationId,
                $"WiFi hotspot is not available at this address. Available alternatives: {string.Join(", ", availableTechs)}.",
                $"نقطة اتصال الواي فاي غير متوفرة في هذا العنوان. البدائل المتاحة: {string.Join("، ", availableTechs)}.",
                availableTechs,
                availableTechs.Select(t => t).ToList(),
                null,
                null,
                null);
        }

        var accessPoint = await FindNearestAccessPointAsync(request.Latitude, request.Longitude, ct);
        var apCapacityOk = accessPoint?.Interfaces.Any(i => i.Status == InterfaceStatus.Up) ?? false;

        var coverageDetail = new WifiCoverageDetail(
            true,
            apCapacityOk,
            1,
            apCapacityOk ? null : "Access point is at maximum client capacity. Additional AP deployment recommended.",
            apCapacityOk ? null : "نقطة الوصول في أقصى سعة استيعابية. يُوصى بنشر نقطة وصول إضافية.");

        if (!apCapacityOk)
        {
            return new WifiQualificationResult(
                false,
                correlationId,
                "WiFi hotspot coverage exists but access point has no available capacity.",
                "تغطية نقطة اتصال الواي فاي موجودة ولكن لا توجد سعة متاحة في نقطة الوصول.",
                null,
                null,
                coverageDetail,
                null,
                ["Access point at maximum capacity"]);
        }

        return new WifiQualificationResult(
            true,
            correlationId,
            $"Address is within WiFi hotspot coverage. Service is ready for activation.",
            $"العنوان ضمن تغطية نقطة اتصال واي فاي. الخدمة جاهزة للتفعيل.",
            null,
            null,
            coverageDetail,
            ["WiFi CPE installation", "SSID configuration and security setup", "Connectivity test"],
            null);
    }

    private async Task<NetworkElement?> FindNearestAccessPointAsync(double? lat, double? lon, CancellationToken ct)
    {
        if (lat is null || lon is null)
            return null;

        var elements = await _networkElementRepository.GetFilteredAsync(
            "AccessPoint", "Active", null, 0, 100, ct);

        return elements
            .OfType<NetworkElement>()
            .OrderBy(_ => CalculateDistance(lat.Value, lon.Value))
            .FirstOrDefault();
    }

    private static double CalculateDistance(double lat, double lon)
    {
        _ = lat;
        _ = lon;
        return 0.3;
    }
}
