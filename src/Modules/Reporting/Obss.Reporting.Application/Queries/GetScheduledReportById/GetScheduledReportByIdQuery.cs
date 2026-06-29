using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetScheduledReportById;

public sealed record GetScheduledReportByIdQuery(Guid ScheduleId) : IRequest<Result<ScheduledReportDto>>;
