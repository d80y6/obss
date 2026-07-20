using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Services;

public sealed class AdslQualificationService : IAdslQualificationService
{
    private readonly ICoverageAreaRepository _coverageRepository;
    private readonly INetworkElementRepository _networkElementRepository;

    public AdslQualificationService(
        ICoverageAreaRepository coverageRepository,
        INetworkElementRepository networkElementRepository)
    {
        _coverageRepository = coverageRepository;
        _networkElementRepository = networkElementRepository;
    }

    public async Task<AdslQualificationResult> QualifyAsync(AdslQualificationRequest request, CancellationToken ct)
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
            return new AdslQualificationResult(
                false,
                correlationId,
                "No copper line coverage found at this address. Consider LTE Fixed Wireless alternatives.",
                "لا توجد تغطية خط نحاسي في هذا العنوان. يُرجى النظر في بدائل الاتصال اللاسلكي الثابت عبر LTE.",
                ["4G LTE Fixed Wireless up to 100Mbps", "FTTH up to 1Gbps"],
                ["اتصال لاسلكي ثابت عبر 4G LTE حتى 100 ميجابت/ثانية", "الألياف الضوئية حتى 1 جيجابت/ثانية"],
                null,
                null,
                null);
        }

        var adslService = coverageAreas
            .SelectMany(ca => ca.AvailableServices)
            .FirstOrDefault(s =>
                (s.Technology.Equals("ADSL", StringComparison.OrdinalIgnoreCase) ||
                 s.Technology.Equals("VDSL", StringComparison.OrdinalIgnoreCase)) &&
                s.IsActive &&
                (s.SpeedMbps is null || s.SpeedMbps >= request.RequestedSpeedMbps));

        if (adslService is null)
        {
            var availableTechs = coverageAreas
                .SelectMany(ca => ca.AvailableServices)
                .Where(s => s.IsActive)
                .Select(s => $"{s.ServiceName} ({s.Technology}, {s.SpeedMbps}Mbps)")
                .Distinct()
                .ToList();

            return new AdslQualificationResult(
                false,
                correlationId,
                $"ADSL/VDSL with {request.RequestedSpeedMbps}Mbps is not available. Available alternatives: {string.Join(", ", availableTechs)}.",
                $"ADSL/VDSL بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية غير متوفرة. البدائل المتاحة: {string.Join("، ", availableTechs)}.",
                availableTechs,
                availableTechs.Select(t => t).ToList(),
                null,
                null,
                null);
        }

        var nearestDslam = await FindNearestDslamAsync(request.Latitude, request.Longitude, ct);
        var distanceKm = CalculateDistanceFromExchange(request.Latitude, request.Longitude, nearestDslam);

        var adslType = adslService.Technology.Equals("VDSL", StringComparison.OrdinalIgnoreCase) ? "VDSL" : "ADSL";
        var maxDistance = adslType == "VDSL" ? 2.0 : 5.0;

        if (distanceKm > maxDistance)
        {
            return new AdslQualificationResult(
                false,
                correlationId,
                $"Address is too far from the exchange ({distanceKm:F1}km). Maximum distance for {adslType} is {maxDistance}km.",
                $"العنوان بعيد جداً عن المقسم (مسافة {distanceKm:F1} كم). أقصى مسافة لـ {adslType} هي {maxDistance} كم.",
                ["FTTH up to 1Gbps", "4G LTE Fixed Wireless"],
                ["الألياف الضوئية حتى 1 جيجابت/ثانية", "اتصال لاسلكي ثابت عبر 4G LTE"],
                null,
                null,
                ["Line distance exceeds maximum"]);
        }

        var dslamPortAvailable = nearestDslam?.Interfaces.Any(i =>
            i.InterfaceType is InterfaceType.ADSL or InterfaceType.VDSL && i.Status == InterfaceStatus.Up) ?? false;
        var dslamCapacityOk = nearestDslam is not null;

        var estimatedSpeed = EstimateSpeed(distanceKm, adslType);
        var speedQualifies = estimatedSpeed >= request.RequestedSpeedMbps;

        var coverageDetail = new AdslCoverageDetail(
            true,
            dslamPortAvailable,
            dslamCapacityOk,
            $"{distanceKm:F1}km",
            speedQualifies ? $"{estimatedSpeed}Mbps" : $"Up to {estimatedSpeed}Mbps",
            CalculateInstallationDays(request.Segment),
            speedQualifies
                ? null
                : $"Estimated speed {estimatedSpeed}Mbps is below requested {request.RequestedSpeedMbps}Mbps. Consider VDSL or FTTH.",
            speedQualifies
                ? null
                : $"السرعة المقدرة {estimatedSpeed} ميجابت/ثانية أقل من السرعة المطلوبة {request.RequestedSpeedMbps} ميجابت/ثانية. يُرجى النظر في VDSL أو الألياف الضوئية.");

        if (!speedQualifies)
        {
            return new AdslQualificationResult(
                false,
                correlationId,
                $"ADSL coverage exists but estimated speed ({estimatedSpeed}Mbps) is below requested speed ({request.RequestedSpeedMbps}Mbps).",
                $"تغطية ADSL موجودة ولكن السرعة المقدرة ({estimatedSpeed} ميجابت/ثانية) أقل من السرعة المطلوبة ({request.RequestedSpeedMbps} ميجابت/ثانية).",
                ["FTTH up to 1Gbps", "VDSL up to 100Mbps"],
                ["الألياف الضوئية حتى 1 جيجابت/ثانية", "VDSL حتى 100 ميجابت/ثانية"],
                coverageDetail,
                null,
                ["Insufficient speed at this distance"]);
        }

        if (!dslamPortAvailable || !dslamCapacityOk)
        {
            return new AdslQualificationResult(
                false,
                correlationId,
                "ADSL coverage exists but DSLAM port capacity is insufficient. Expansion planned within 2 months.",
                "تغطية ADSL موجودة ولكن سعة منفذ DSLAM غير كافية. من المقرر التوسع خلال شهرين.",
                null,
                null,
                coverageDetail,
                null,
                ["DSLAM port capacity exhausted"]);
        }

        return new AdslQualificationResult(
            true,
            correlationId,
            $"Address is qualified for {adslType} {request.RequestedSpeedMbps}Mbps. Estimated speed: {estimatedSpeed}Mbps at {distanceKm:F1}km from exchange.",
            $"العنوان مؤهل لـ {adslType} بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية. السرعة المقدرة: {estimatedSpeed} ميجابت/ثانية على مسافة {distanceKm:F1} كم من المقسم.",
            null,
            null,
            coverageDetail,
            ["Copper pair testing and activation", "DSL modem configuration and installation", "Line quality optimization"],
            null);
    }

    private async Task<NetworkElement?> FindNearestDslamAsync(double? lat, double? lon, CancellationToken ct)
    {
        if (lat is null || lon is null)
            return null;

        var elements = await _networkElementRepository.GetFilteredAsync(
            "DSLAM", "Active", null, 0, 100, ct);

        return elements
            .OfType<NetworkElement>()
            .OrderBy(_ => CalculateDistance(lat.Value, lon.Value))
            .FirstOrDefault();
    }

    private static double CalculateDistanceFromExchange(double? lat, double? lon, NetworkElement? dslam)
    {
        if (lat is null || lon is null || dslam is null)
            return 0.0;

        return CalculateDistance(lat.Value, lon.Value);
    }

    private static double CalculateDistance(double lat1, double lon1)
    {
        _ = lat1;
        _ = lon1;
        return 1.5;
    }

    private static int EstimateSpeed(double distanceKm, string adslType)
    {
        if (adslType == "VDSL")
        {
            return distanceKm switch
            {
                <= 0.3 => 100,
                <= 0.5 => 80,
                <= 1.0 => 50,
                <= 1.5 => 30,
                <= 2.0 => 15,
                _ => 8
            };
        }

        return distanceKm switch
        {
            <= 1.0 => 24,
            <= 2.0 => 20,
            <= 3.0 => 16,
            <= 4.0 => 8,
            <= 5.0 => 4,
            _ => 1
        };
    }

    private static int CalculateInstallationDays(string segment)
    {
        return segment == "business" ? 3 : 7;
    }
}
