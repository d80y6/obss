using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.Reporting.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.ScheduleReport;

public sealed class ScheduleReportCommandHandler : IRequestHandler<ScheduleReportCommand, Result<ScheduledReportDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleReportCommandHandler(IReportRepository reportRepository, IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ScheduledReportDto>> Handle(ScheduleReportCommand request, CancellationToken cancellationToken)
    {
        var reportDefinition = await _reportRepository.GetByIdAsync<Guid>(request.ReportDefinitionId, cancellationToken);

        if (reportDefinition is null)
            return Result.Failure<ScheduledReportDto>(Error.NotFound(nameof(ReportDefinition), request.ReportDefinitionId));

        var scheduledReport = ScheduledReport.Create(
            request.TenantId,
            request.ReportDefinitionId,
            request.CronExpression,
            request.Recipients);

        await _reportRepository.AddScheduledReportAsync(scheduledReport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(scheduledReport.Adapt<ScheduledReportDto>());
    }
}
