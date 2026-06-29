using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.CreateDashboardWidget;

public sealed record CreateDashboardWidgetCommand(
    string TenantId,
    string WidgetType,
    string Title,
    string Configuration,
    int Position,
    string Size,
    string DataSource,
    string Query,
    int? RefreshInterval) : IRequest<Result<DashboardWidgetDto>>;
