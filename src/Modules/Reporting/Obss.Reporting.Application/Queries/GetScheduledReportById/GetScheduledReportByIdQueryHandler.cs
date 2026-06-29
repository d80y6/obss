using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.Reporting.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetScheduledReportById;

public sealed class GetScheduledReportByIdQueryHandler : IRequestHandler<GetScheduledReportByIdQuery, Result<ScheduledReportDto>>
{
    private readonly IReportRepository _reportRepository;

    public GetScheduledReportByIdQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ScheduledReportDto>> Handle(GetScheduledReportByIdQuery request, CancellationToken cancellationToken)
    {
        var scheduledReport = await _reportRepository.GetScheduledReportByIdAsync(request.ScheduleId, cancellationToken);

        if (scheduledReport is null)
            return Result.Failure<ScheduledReportDto>(Error.NotFound(nameof(ScheduledReport), request.ScheduleId));

        return Result.Success(scheduledReport.Adapt<ScheduledReportDto>());
    }
}
