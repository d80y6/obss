namespace Obss.ServiceQualification.Domain.Services;

public sealed class UnifiedQualificationService : IUnifiedQualificationService
{
    private readonly IFtthQualificationService _ftthService;
    private readonly IAdslQualificationService _adslService;
    private readonly ILteQualificationService _lteService;
    private readonly IWifiQualificationService _wifiService;
    private readonly ITelephonyQualificationService _telephonyService;
    private readonly IBusinessServiceQualificationService _businessService;
    private readonly QualificationEngine _engine;

    public UnifiedQualificationService(
        IFtthQualificationService ftthService,
        IAdslQualificationService adslService,
        ILteQualificationService lteService,
        IWifiQualificationService wifiService,
        ITelephonyQualificationService telephonyService,
        IBusinessServiceQualificationService businessService,
        QualificationEngine engine)
    {
        _ftthService = ftthService;
        _adslService = adslService;
        _lteService = lteService;
        _wifiService = wifiService;
        _telephonyService = telephonyService;
        _businessService = businessService;
        _engine = engine;
    }

    public async Task<UnifiedQualificationResult> QualifyAsync(UnifiedQualificationRequest request, CancellationToken ct)
    {
        var rule = _engine.GetRule(request.ServiceType, request.Segment);

        if (rule is not null && !rule.EligibilityCheck(request))
        {
            return new UnifiedQualificationResult(
                false,
                Guid.NewGuid().ToString(),
                request.ServiceType,
                $"Service type '{request.ServiceType}' is not eligible for segment '{request.Segment}' per eligibility rules.",
                $"خدمة '{request.ServiceType}' غير مؤهلة للقطاع '{request.Segment}' وفقاً لقواعد الأهلية.",
                null,
                null,
                null,
                null,
                null);
        }

        var normalizedType = request.ServiceType.ToUpperInvariant();

        return normalizedType switch
        {
            "FTTH" => await MapFtthResultAsync(request, ct),
            "ADSL" or "VDSL" => await MapAdslResultAsync(request, ct),
            "LTE" or "4G" or "FIXED_WIRELESS" => await MapLteResultAsync(request, ct),
            "WIFI" or "HOTSPOT" => await MapWifiResultAsync(request, ct),
            "TELEPHONY" or "POTS" or "VOIP" => await MapTelephonyResultAsync(request, ct),
            "DIA" or "ETHERNET" or "TDM" or "COLOCATION" or "DEDICATEDSERVER" or "CLOUDCONNECT" => await MapBusinessResultAsync(request, ct),
            _ => new UnifiedQualificationResult(
                false,
                Guid.NewGuid().ToString(),
                request.ServiceType,
                $"Unknown service type '{request.ServiceType}'. Supported types: FTTH, ADSL, VDSL, LTE, WIFI, TELEPHONY, DIA, ETHERNET, TDM, COLOCATION, DEDICATEDSERVER, CLOUDCONNECT.",
                $"نوع الخدمة '{request.ServiceType}' غير معروف. الأنواع المدعومة: FTTH, ADSL, VDSL, LTE, WIFI, TELEPHONY, DIA, ETHERNET, TDM, COLOCATION, DEDICATEDSERVER, CLOUDCONNECT.",
                null,
                null,
                null,
                null,
                null)
        };
    }

    private async Task<UnifiedQualificationResult> MapFtthResultAsync(UnifiedQualificationRequest request, CancellationToken ct)
    {
        var ftthRequest = new FtthQualificationRequest(
            request.Address,
            request.City,
            request.State,
            request.Latitude,
            request.Longitude,
            request.SpeedMbps ?? 100,
            request.Segment);

        var result = await _ftthService.QualifyAsync(ftthRequest, ct);

        return new UnifiedQualificationResult(
            result.IsQualified,
            result.CorrelationId,
            "FTTH",
            result.Explanation ?? string.Empty,
            result.ExplanationAr ?? string.Empty,
            result.Alternatives,
            result.AlternativesAr,
            result.CoverageDetail is not null
                ? new UnifiedCoverageDetail(
                    result.CoverageDetail.FiberAtPremises,
                    result.CoverageDetail.OltCapacityAvailable,
                    result.CoverageDetail.PonPortAvailable,
                    result.CoverageDetail.EstimatedDistance,
                    result.CoverageDetail.EstimatedInstallationDays,
                    null,
                    null)
                : null,
            result.RequiredWork,
            result.CapacityConflicts);
    }

    private async Task<UnifiedQualificationResult> MapAdslResultAsync(UnifiedQualificationRequest request, CancellationToken ct)
    {
        var adslRequest = new AdslQualificationRequest(
            request.Address,
            request.City,
            request.State,
            request.Latitude,
            request.Longitude,
            request.SpeedMbps ?? 20,
            request.Segment);

        var result = await _adslService.QualifyAsync(adslRequest, ct);

        return new UnifiedQualificationResult(
            result.IsQualified,
            result.CorrelationId,
            request.ServiceType.ToUpperInvariant(),
            result.Explanation ?? string.Empty,
            result.ExplanationAr ?? string.Empty,
            result.Alternatives,
            result.AlternativesAr,
            result.CoverageDetail is not null
                ? new UnifiedCoverageDetail(
                    result.CoverageDetail.CopperLineAvailable,
                    result.CoverageDetail.DslamCapacityAvailable,
                    result.CoverageDetail.DslamPortAvailable,
                    result.CoverageDetail.EstimatedSpeed,
                    result.CoverageDetail.EstimatedInstallationDays,
                    result.CoverageDetail.AdditionalInfo,
                    result.CoverageDetail.AdditionalInfoAr)
                : null,
            result.RequiredWork,
            result.CapacityConflicts);
    }

    private async Task<UnifiedQualificationResult> MapLteResultAsync(UnifiedQualificationRequest request, CancellationToken ct)
    {
        var lteRequest = new LteQualificationRequest(
            request.Address,
            request.City,
            request.State,
            request.Latitude,
            request.Longitude,
            request.SpeedMbps ?? 50,
            request.Segment);

        var result = await _lteService.QualifyAsync(lteRequest, ct);

        return new UnifiedQualificationResult(
            result.IsQualified,
            result.CorrelationId,
            "LTE",
            result.Explanation ?? string.Empty,
            result.ExplanationAr ?? string.Empty,
            result.Alternatives,
            result.AlternativesAr,
            result.CoverageDetail is not null
                ? new UnifiedCoverageDetail(
                    result.CoverageDetail.CoverageAvailable,
                    result.CoverageDetail.CellCapacityAvailable,
                    true,
                    result.CoverageDetail.EstimatedSpeed,
                    result.CoverageDetail.EstimatedInstallationDays,
                    result.CoverageDetail.AdditionalInfo,
                    result.CoverageDetail.AdditionalInfoAr)
                : null,
            result.RequiredWork,
            result.CapacityConflicts);
    }

    private async Task<UnifiedQualificationResult> MapWifiResultAsync(UnifiedQualificationRequest request, CancellationToken ct)
    {
        var wifiRequest = new WifiQualificationRequest(
            request.Address,
            request.City,
            request.State,
            request.Latitude,
            request.Longitude,
            request.Segment);

        var result = await _wifiService.QualifyAsync(wifiRequest, ct);

        return new UnifiedQualificationResult(
            result.IsQualified,
            result.CorrelationId,
            "WIFI",
            result.Explanation ?? string.Empty,
            result.ExplanationAr ?? string.Empty,
            result.Alternatives,
            result.AlternativesAr,
            result.CoverageDetail is not null
                ? new UnifiedCoverageDetail(
                    result.CoverageDetail.HotspotAvailable,
                    result.CoverageDetail.AccessPointCapacityAvailable,
                    true,
                    null,
                    result.CoverageDetail.EstimatedInstallationDays,
                    result.CoverageDetail.AdditionalInfo,
                    result.CoverageDetail.AdditionalInfoAr)
                : null,
            result.RequiredWork,
            result.CapacityConflicts);
    }

    private async Task<UnifiedQualificationResult> MapTelephonyResultAsync(UnifiedQualificationRequest request, CancellationToken ct)
    {
        var telephonyRequest = new TelephonyQualificationRequest(
            request.Address,
            request.City,
            request.State,
            request.TelephoneNumber ?? string.Empty,
            request.Segment);

        var result = await _telephonyService.QualifyAsync(telephonyRequest, ct);

        return new UnifiedQualificationResult(
            result.IsQualified,
            result.CorrelationId,
            "TELEPHONY",
            result.Explanation ?? string.Empty,
            result.ExplanationAr ?? string.Empty,
            result.Alternatives,
            result.AlternativesAr,
            result.CoverageDetail is not null
                ? new UnifiedCoverageDetail(
                    result.CoverageDetail.NumberAvailable,
                    result.CoverageDetail.SoftswitchCapacityAvailable,
                    true,
                    null,
                    result.CoverageDetail.EstimatedInstallationDays,
                    result.CoverageDetail.AdditionalInfo,
                    result.CoverageDetail.AdditionalInfoAr)
                : null,
            result.RequiredWork,
            result.CapacityConflicts);
    }

    private async Task<UnifiedQualificationResult> MapBusinessResultAsync(UnifiedQualificationRequest request, CancellationToken ct)
    {
        var businessRequest = new BusinessServiceQualificationRequest(
            request.Address,
            request.City,
            request.State,
            request.Latitude,
            request.Longitude,
            request.ServiceType.ToUpperInvariant(),
            request.SpeedMbps,
            request.DomainName,
            request.Segment);

        var result = await _businessService.QualifyAsync(businessRequest, ct);

        return new UnifiedQualificationResult(
            result.IsQualified,
            result.CorrelationId,
            request.ServiceType.ToUpperInvariant(),
            result.Explanation ?? string.Empty,
            result.ExplanationAr ?? string.Empty,
            result.Alternatives,
            result.AlternativesAr,
            result.CoverageDetail is not null
                ? new UnifiedCoverageDetail(
                    result.CoverageDetail.FiberAvailable || result.CoverageDetail.CopperAvailable,
                    result.CoverageDetail.CapacityAvailable,
                    result.CoverageDetail.FiberAvailable,
                    null,
                    result.CoverageDetail.EstimatedInstallationDays,
                    result.CoverageDetail.AdditionalInfo,
                    result.CoverageDetail.AdditionalInfoAr)
                : null,
            result.RequiredWork,
            result.CapacityConflicts);
    }
}
