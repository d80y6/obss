using MediatR;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    string Name,
    List<string>? Permissions,
    List<string>? AllowedIPs,
    int RateLimitPerMinute,
    Guid? PartnerId,
    DateTime? ExpiresAt) : IRequest<Result<ApiKeyDto>>;
