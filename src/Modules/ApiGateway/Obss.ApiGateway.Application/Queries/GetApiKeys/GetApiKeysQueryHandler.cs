using Mapster;
using MediatR;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Queries.GetApiKeys;

public sealed class GetApiKeysQueryHandler : IRequestHandler<GetApiKeysQuery, Result<IReadOnlyList<ApiKeyDto>>>
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public GetApiKeysQueryHandler(IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<Result<IReadOnlyList<ApiKeyDto>>> Handle(GetApiKeysQuery request, CancellationToken cancellationToken)
    {
        var keys = await _apiKeyRepository.GetAllAsync(cancellationToken);
        return Result.Success(keys.Adapt<IReadOnlyList<ApiKeyDto>>());
    }
}
