using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetReportDefinitions;

public sealed record GetReportDefinitionsQuery : IRequest<Result<IReadOnlyList<ReportDefinitionDto>>>;
