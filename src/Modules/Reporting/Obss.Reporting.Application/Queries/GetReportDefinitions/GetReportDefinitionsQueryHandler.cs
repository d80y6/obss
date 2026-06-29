using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetReportDefinitions;

public sealed class GetReportDefinitionsQueryHandler : IRequestHandler<GetReportDefinitionsQuery, Result<IReadOnlyList<ReportDefinitionDto>>>
{
    private readonly IReportRepository _reportRepository;

    public GetReportDefinitionsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<IReadOnlyList<ReportDefinitionDto>>> Handle(GetReportDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var definitions = await _reportRepository.GetAllAsync(cancellationToken);
        return Result.Success(definitions.Adapt<IReadOnlyList<ReportDefinitionDto>>());
    }
}
