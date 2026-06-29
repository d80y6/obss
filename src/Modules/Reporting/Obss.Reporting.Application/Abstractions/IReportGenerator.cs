using Obss.Reporting.Domain.Entities;

namespace Obss.Reporting.Application.Abstractions;

public interface IReportGenerator
{
    Task<(string FilePath, long FileSize)> GenerateAsync(ReportDefinition reportDefinition, CancellationToken cancellationToken = default);
}
