using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.Reporting.Domain.Entities;
using Obss.Reporting.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.CreateReportDefinition;

public sealed class CreateReportDefinitionCommandHandler : IRequestHandler<CreateReportDefinitionCommand, Result<ReportDefinitionDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReportDefinitionCommandHandler(IReportRepository reportRepository, IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReportDefinitionDto>> Handle(CreateReportDefinitionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ReportType>(request.ReportType, out var reportType))
            return Result.Failure<ReportDefinitionDto>(Error.Validation($"Invalid report type: '{request.ReportType}'."));

        if (!Enum.TryParse<OutputFormat>(request.OutputFormat, out var outputFormat))
            return Result.Failure<ReportDefinitionDto>(Error.Validation($"Invalid output format: '{request.OutputFormat}'."));

        var reportDefinition = ReportDefinition.Create(
            request.TenantId,
            request.Name,
            request.Description,
            reportType,
            request.DataSource,
            request.Query,
            outputFormat,
            request.Schedule);

        await _reportRepository.AddAsync(reportDefinition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(reportDefinition.Adapt<ReportDefinitionDto>());
    }
}
