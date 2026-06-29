using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetReportExecutions;

public sealed class GetReportExecutionsQueryHandler : IRequestHandler<GetReportExecutionsQuery, Result<IReadOnlyList<ReportExecutionDto>>>
{
    private readonly IReportRepository _reportRepository;

    public GetReportExecutionsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<IReadOnlyList<ReportExecutionDto>>> Handle(GetReportExecutionsQuery request, CancellationToken cancellationToken)
    {
        var executions = await _reportRepository.GetExecutionsByReportDefinitionIdAsync(request.ReportDefinitionId, cancellationToken);
        return Result.Success(executions.Adapt<IReadOnlyList<ReportExecutionDto>>());
    }
}
