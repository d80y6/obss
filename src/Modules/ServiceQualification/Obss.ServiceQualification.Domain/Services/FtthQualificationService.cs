using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Services;

public sealed class FtthQualificationService : IFtthQualificationService
{
    private readonly ICoverageAreaRepository _coverageRepository;
    private readonly INetworkElementRepository _networkElementRepository;

    public FtthQualificationService(
        ICoverageAreaRepository coverageRepository,
        INetworkElementRepository networkElementRepository)
    {
        _coverageRepository = coverageRepository;
        _networkElementRepository = networkElementRepository;
    }

    public async Task<FtthQualificationResult> QualifyAsync(FtthQualificationRequest request, CancellationToken ct)
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
            return new FtthQualificationResult(
                false,
                correlationId,
                "No fiber coverage found at this address. Consider ADSL or wireless alternatives.",
                "لا توجد تغطية ألياف ضوئية في هذا العنوان. يُرجى النظر في بدائل ADSL أو الاتصال اللاسلكي.",
                ["ADSL up to 50Mbps", "4G LTE Fixed Wireless"],
                ["ADSL حتى 50 ميجابت/ثانية", "اتصال لاسلكي ثابت عبر 4G LTE"],
                null,
                null,
                null);
        }

        var ftthService = coverageAreas
            .SelectMany(ca => ca.AvailableServices)
            .FirstOrDefault(s =>
                s.Technology.Equals("FTTH", StringComparison.OrdinalIgnoreCase) &&
                s.IsActive &&
                (s.SpeedMbps is null || s.SpeedMbps >= request.RequestedSpeedMbps));

        if (ftthService is null)
        {
            var availableTechs = coverageAreas
                .SelectMany(ca => ca.AvailableServices)
                .Where(s => s.IsActive)
                .Select(s => $"{s.ServiceName} ({s.Technology}, {s.SpeedMbps}Mbps)")
                .Distinct()
                .ToList();

            return new FtthQualificationResult(
                false,
                correlationId,
                $"FTTH with {request.RequestedSpeedMbps}Mbps is not available. Available alternatives: {string.Join(", ", availableTechs)}.",
                $"الألياف الضوئية بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية غير متوفرة. البدائل المتاحة: {string.Join("، ", availableTechs)}.",
                availableTechs,
                availableTechs.Select(t => t).ToList(),
                null,
                null,
                null);
        }

        var nearestOlt = await FindNearestOltAsync(request.Latitude, request.Longitude, ct);
        var oltCapacityOk = nearestOlt is not null && nearestOlt.UsedPONPorts < nearestOlt.MaxPONPorts;
        var ponPortAvailable = nearestOlt?.PONPorts.Any(p => p.Status == PONPortStatus.Free) ?? false;

        var coverageDetail = new FtthCoverageDetail(
            true,
            oltCapacityOk,
            ponPortAvailable,
            true,
            nearestOlt?.Name,
            nearestOlt is not null ? $"{CalculateDistance(request.Latitude, request.Longitude, nearestOlt):F1}km" : null,
            CalculateInstallationDays(ponPortAvailable, oltCapacityOk, request.Segment),
            false,
            false);

        if (!oltCapacityOk || !ponPortAvailable)
        {
            return new FtthQualificationResult(
                false,
                correlationId,
                "FTTH coverage exists but OLT capacity is insufficient. Expansion planned within 3 months.",
                "تغطية الألياف الضوئية موجودة ولكن سعة OLT غير كافية. من المقرر التوسع خلال 3 أشهر.",
                null,
                null,
                coverageDetail,
                null,
                ["OLT port capacity exhausted", "PON port not available"]);
        }

        return new FtthQualificationResult(
            true,
            correlationId,
            $"Address is qualified for FTTH {request.RequestedSpeedMbps}Mbps. Estimated installation: {coverageDetail.EstimatedInstallationDays} days.",
            $"العنوان مؤهل للألياف الضوئية بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية. التثبيت المقدر: {coverageDetail.EstimatedInstallationDays} يومًا.",
            null,
            null,
            coverageDetail,
            ["Fiber drop installation", "ONT mounting and configuration", "Splice and termination"],
            null);
    }

    private async Task<OLT?> FindNearestOltAsync(double? lat, double? lon, CancellationToken ct)
    {
        if (lat is null || lon is null)
            return null;

        var elements = await _networkElementRepository.GetFilteredAsync(
            "OLT", "Active", null, 0, 100, ct);

        return elements
            .OfType<OLT>()
            .OrderBy(o => CalculateDistance(lat.Value, lon.Value, o))
            .FirstOrDefault();
    }

    private static double CalculateDistance(double? lat1, double? lon1, OLT olt)
    {
        _ = lat1;
        _ = lon1;
        _ = olt;
        return 0.5;
    }

    private static int CalculateInstallationDays(bool ponAvailable, bool oltCapacityOk, string segment)
    {
        if (!ponAvailable || !oltCapacityOk)
            return 90;

        return segment == "business" ? 5 : 10;
    }
}
