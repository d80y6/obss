using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceQualification.Domain.ValueObjects;

public sealed class CoverageService : ValueObject
{
    public string ServiceName { get; }
    public int? SpeedMbps { get; }
    public string Technology { get; }
    public decimal? MonthlyPrice { get; }
    public bool IsActive { get; }

    private CoverageService(
        string serviceName,
        int? speedMbps,
        string technology,
        decimal? monthlyPrice,
        bool isActive)
    {
        ServiceName = serviceName;
        SpeedMbps = speedMbps;
        Technology = technology;
        MonthlyPrice = monthlyPrice;
        IsActive = isActive;
    }

    public static CoverageService Create(
        string serviceName,
        int? speedMbps,
        string technology,
        decimal? monthlyPrice,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be empty", nameof(serviceName));
        if (string.IsNullOrWhiteSpace(technology))
            throw new ArgumentException("Technology cannot be empty", nameof(technology));

        return new CoverageService(serviceName, speedMbps, technology, monthlyPrice, isActive);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ServiceName;
        yield return SpeedMbps ?? 0;
        yield return Technology;
        yield return MonthlyPrice ?? 0m;
        yield return IsActive;
    }

    public override string ToString() => $"{ServiceName} ({Technology}, {SpeedMbps ?? 0}Mbps)";
}
