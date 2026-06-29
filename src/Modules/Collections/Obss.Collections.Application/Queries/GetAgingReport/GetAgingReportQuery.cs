using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetAgingReport;

public sealed record GetAgingReportQuery(string? Currency) : IRequest<Result<AgingReportDto>>;
