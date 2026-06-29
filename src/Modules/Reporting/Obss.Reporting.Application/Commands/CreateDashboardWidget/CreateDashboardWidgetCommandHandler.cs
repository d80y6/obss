using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.Reporting.Domain.Entities;
using Obss.Reporting.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.CreateDashboardWidget;

public sealed class CreateDashboardWidgetCommandHandler : IRequestHandler<CreateDashboardWidgetCommand, Result<DashboardWidgetDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDashboardWidgetCommandHandler(IReportRepository reportRepository, IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DashboardWidgetDto>> Handle(CreateDashboardWidgetCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<WidgetType>(request.WidgetType, out var widgetType))
            return Result.Failure<DashboardWidgetDto>(Error.Validation($"Invalid widget type: '{request.WidgetType}'."));

        if (!Enum.TryParse<WidgetSize>(request.Size, out var size))
            return Result.Failure<DashboardWidgetDto>(Error.Validation($"Invalid widget size: '{request.Size}'."));

        var widget = DashboardWidget.Create(
            request.TenantId,
            widgetType,
            request.Title,
            request.Configuration,
            request.Position,
            size,
            request.DataSource,
            request.Query,
            request.RefreshInterval);

        await _reportRepository.AddWidgetAsync(widget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(widget.Adapt<DashboardWidgetDto>());
    }
}
