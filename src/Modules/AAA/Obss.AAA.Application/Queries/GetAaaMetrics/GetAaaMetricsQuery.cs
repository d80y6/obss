using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaMetrics;

public sealed record GetAaaMetricsQuery : IRequest<Result<AaaMetricsDto>>;
