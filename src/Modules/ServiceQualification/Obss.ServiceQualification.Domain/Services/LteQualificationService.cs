using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Services;

public sealed class LteQualificationService : ILteQualificationService
{
    private readonly ICoverageAreaRepository _coverageRepository;
    private readonly INetworkElementRepository _networkElementRepository;

    public LteQualificationService(
        ICoverageAreaRepository coverageRepository,
        INetworkElementRepository networkElementRepository)
    {
        _coverageRepository = coverageRepository;
        _networkElementRepository = networkElementRepository;
    }

    public async Task<LteQualificationResult> QualifyAsync(LteQualificationRequest request, CancellationToken ct)
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
            return new LteQualificationResult(
                false,
                correlationId,
                "No LTE coverage found at this address. Consider FTTH or ADSL alternatives.",
                "لا توجد تغطية LTE في هذا العنوان. يُرجى النظر في بدائل الألياف الضوئية أو ADSL.",
                ["FTTH up to 1Gbps", "ADSL up to 24Mbps"],
                ["الألياف الضوئية حتى 1 جيجابت/ثانية", "ADSL حتى 24 ميجابت/ثانية"],
                null,
                null,
                null);
        }

        var lteService = coverageAreas
            .SelectMany(ca => ca.AvailableServices)
            .FirstOrDefault(s =>
                s.Technology.Equals("LTE", StringComparison.OrdinalIgnoreCase) &&
                s.IsActive &&
                (s.SpeedMbps is null || s.SpeedMbps >= request.RequestedSpeedMbps));

        if (lteService is null)
        {
            var availableTechs = coverageAreas
                .SelectMany(ca => ca.AvailableServices)
                .Where(s => s.IsActive)
                .Select(s => $"{s.ServiceName} ({s.Technology}, {s.SpeedMbps}Mbps)")
                .Distinct()
                .ToList();

            return new LteQualificationResult(
                false,
                correlationId,
                $"LTE with {request.RequestedSpeedMbps}Mbps is not available at this address. Available alternatives: {string.Join(", ", availableTechs)}.",
                $"LTE بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية غير متوفرة في هذا العنوان. البدائل المتاحة: {string.Join("، ", availableTechs)}.",
                availableTechs,
                availableTechs.Select(t => t).ToList(),
                null,
                null,
                null);
        }

        var nearestCell = await FindNearestCellAsync(request.Latitude, request.Longitude, ct);
        var signalStrength = EstimateSignalStrength(request.Latitude, request.Longitude, nearestCell);
        var cellCapacityOk = nearestCell is not null;

        var estimatedSpeed = signalStrength switch
        {
            "Excellent" => 150,
            "Good" => 75,
            "Fair" => 30,
            "Weak" => 10,
            _ => 5
        };

        var speedQualifies = estimatedSpeed >= request.RequestedSpeedMbps;

        var coverageDetail = new LteCoverageDetail(
            true,
            cellCapacityOk,
            signalStrength,
            $"{estimatedSpeed}Mbps",
            request.Segment == "business" ? 1 : 2,
            speedQualifies
                ? null
                : $"Estimated LTE speed ({estimatedSpeed}Mbps) is below requested speed ({request.RequestedSpeedMbps}Mbps). Signal: {signalStrength}.",
            speedQualifies
                ? null
                : $"سرعة LTE المقدرة ({estimatedSpeed} ميجابت/ثانية) أقل من السرعة المطلوبة ({request.RequestedSpeedMbps} ميجابت/ثانية). قوة الإشارة: {signalStrength}.");

        if (!speedQualifies)
        {
            return new LteQualificationResult(
                false,
                correlationId,
                $"LTE coverage exists but estimated speed ({estimatedSpeed}Mbps) is below requested ({request.RequestedSpeedMbps}Mbps). Signal strength: {signalStrength}.",
                $"تغطية LTE موجودة ولكن السرعة المقدرة ({estimatedSpeed} ميجابت/ثانية) أقل من المطلوب ({request.RequestedSpeedMbps} ميجابت/ثانية). قوة الإشارة: {signalStrength}.",
                ["FTTH up to 1Gbps", "VDSL up to 100Mbps"],
                ["الألياف الضوئية حتى 1 جيجابت/ثانية", "VDSL حتى 100 ميجابت/ثانية"],
                coverageDetail,
                null,
                ["Weak signal strength", "Insufficient speed at this location"]);
        }

        if (!cellCapacityOk)
        {
            return new LteQualificationResult(
                false,
                correlationId,
                "LTE coverage exists but cell is at full capacity. Expansion planned within 1 month.",
                "تغطية LTE موجودة ولكن سعة الخلية ممتلئة. من المقرر التوسع خلال شهر.",
                null,
                null,
                coverageDetail,
                null,
                ["Cell capacity exhausted"]);
        }

        return new LteQualificationResult(
            true,
            correlationId,
            $"Address is qualified for LTE {request.RequestedSpeedMbps}Mbps. Signal strength: {signalStrength}. Estimated speed: {estimatedSpeed}Mbps.",
            $"العنوان مؤهل لـ LTE بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية. قوة الإشارة: {signalStrength}. السرعة المقدرة: {estimatedSpeed} ميجابت/ثانية.",
            null,
            null,
            coverageDetail,
            ["CPE installation and configuration", "Antenna alignment and optimization", "Signal testing and validation"],
            null);
    }

    private async Task<NetworkElement?> FindNearestCellAsync(double? lat, double? lon, CancellationToken ct)
    {
        if (lat is null || lon is null)
            return null;

        var elements = await _networkElementRepository.GetFilteredAsync(
            "LTE_ENB", "Active", null, 0, 100, ct);

        return elements
            .OfType<NetworkElement>()
            .OrderBy(_ => CalculateDistance(lat.Value, lon.Value))
            .FirstOrDefault();
    }

    private static string EstimateSignalStrength(double? lat, double? lon, NetworkElement? cell)
    {
        _ = lat;
        _ = lon;
        _ = cell;
        return "Good";
    }

    private static double CalculateDistance(double lat, double lon)
    {
        _ = lat;
        _ = lon;
        return 0.5;
    }
}
