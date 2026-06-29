using Mapster;
using MediatR;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Application.DTOs;
using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.CreateApiKey;

public sealed class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, Result<ApiKeyDto>>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateApiKeyCommandHandler(
        IApiKeyRepository apiKeyRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _apiKeyRepository = apiKeyRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<ApiKeyDto>> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = ApiKey.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.Name,
            request.Permissions,
            request.AllowedIPs,
            request.RateLimitPerMinute,
            request.PartnerId,
            request.ExpiresAt);

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(apiKey.Adapt<ApiKeyDto>());
    }
}
