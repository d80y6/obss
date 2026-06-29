using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetDashboardConfig;

public sealed record GetDashboardConfigQuery(
    string? TenantId) : IRequest<Result<IReadOnlyList<DashboardWidgetDto>>>;
