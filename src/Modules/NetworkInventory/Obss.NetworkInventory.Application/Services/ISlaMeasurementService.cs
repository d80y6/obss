using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Application.Services;

public sealed record SlaComplianceResult(
    string ServiceId,
    double CompliancePercentage,
    int TotalMetrics,
    int BreachedMetrics,
    string? ServiceType);

public sealed record SlaReport(
    string ServiceId,
    DateTime From,
    DateTime To,
    double CompliancePercentage,
    decimal CreditsEarned,
    IReadOnlyList<SlaComplianceResult> Details);

public interface ISlaMeasurementService
{
    Task<SlaComplianceResult> CalculateComplianceAsync(
        string serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<SlaReport> GenerateReportAsync(
        string serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<decimal> CalculateCreditsAsync(
        string serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
