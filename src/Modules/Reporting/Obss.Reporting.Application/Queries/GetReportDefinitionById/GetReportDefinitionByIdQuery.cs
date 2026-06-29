using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetReportDefinitionById;

public sealed record GetReportDefinitionByIdQuery(Guid ReportDefinitionId) : IRequest<Result<ReportDefinitionDto>>;
