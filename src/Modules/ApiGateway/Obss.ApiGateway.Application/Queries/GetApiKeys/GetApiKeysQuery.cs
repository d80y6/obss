using MediatR;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Queries.GetApiKeys;

public sealed record GetApiKeysQuery : IRequest<Result<IReadOnlyList<ApiKeyDto>>>;
