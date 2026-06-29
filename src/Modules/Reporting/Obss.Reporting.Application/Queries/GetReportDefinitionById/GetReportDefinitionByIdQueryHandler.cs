using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.Reporting.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetReportDefinitionById;

public sealed class GetReportDefinitionByIdQueryHandler : IRequestHandler<GetReportDefinitionByIdQuery, Result<ReportDefinitionDto>>
{
    private readonly IReportRepository _reportRepository;

    public GetReportDefinitionByIdQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ReportDefinitionDto>> Handle(GetReportDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var definition = await _reportRepository.GetByIdAsync<Guid>(request.ReportDefinitionId, cancellationToken);

        if (definition is null)
            return Result.Failure<ReportDefinitionDto>(Error.NotFound(nameof(ReportDefinition), request.ReportDefinitionId));

        return Result.Success(definition.Adapt<ReportDefinitionDto>());
    }
}
