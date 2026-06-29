using MediatR;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Queries.GetApiRoutes;

public sealed record GetApiRoutesQuery : IRequest<Result<IReadOnlyList<ApiRouteDto>>>;
