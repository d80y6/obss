using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.Reporting.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.ExecuteReport;

public sealed class ExecuteReportCommandHandler : IRequestHandler<ExecuteReportCommand, Result<ReportExecutionDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportGenerator _reportGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExecuteReportCommandHandler> _logger;

    public ExecuteReportCommandHandler(
        IReportRepository reportRepository,
        IReportGenerator reportGenerator,
        IUnitOfWork unitOfWork,
        ILogger<ExecuteReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _reportGenerator = reportGenerator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ReportExecutionDto>> Handle(ExecuteReportCommand request, CancellationToken cancellationToken)
    {
        var reportDefinition = await _reportRepository.GetByIdAsync<Guid>(request.ReportDefinitionId, cancellationToken);

        if (reportDefinition is null)
            return Result.Failure<ReportExecutionDto>(Error.NotFound(nameof(ReportDefinition), request.ReportDefinitionId));

        if (!reportDefinition.IsActive)
            return Result.Failure<ReportExecutionDto>(Error.Validation("Report definition is deactivated."));

        var execution = new ReportExecution(Guid.NewGuid(), request.ReportDefinitionId, request.ExecutedBy);
        await _reportRepository.AddExecutionAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            execution.Start();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var (filePath, fileSize) = await _reportGenerator.GenerateAsync(reportDefinition, cancellationToken);

            execution.Complete(filePath, fileSize);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {ReportDefinitionId} executed successfully.", request.ReportDefinitionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report {ReportDefinitionId} execution failed.", request.ReportDefinitionId);
            execution.Fail(ex.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<ReportExecutionDto>(Error.Validation($"Report execution failed: {ex.Message}"));
        }

        return Result.Success(execution.Adapt<ReportExecutionDto>());
    }
}
