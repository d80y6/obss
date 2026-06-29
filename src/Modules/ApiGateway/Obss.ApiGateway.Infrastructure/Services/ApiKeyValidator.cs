using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Domain.Entities;
using Obss.ApiGateway.Domain.Services;

namespace Obss.ApiGateway.Infrastructure.Services;

public sealed class ApiKeyValidator : IApiKeyValidator
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyValidator(IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public (bool Valid, ApiKey? ApiKey) ValidateApiKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return (false, null);

        var hashedKey = ApiKey.HashKey(key);
        var apiKey = _apiKeyRepository.GetByKeyAsync(hashedKey).GetAwaiter().GetResult();

        if (apiKey is null)
            return (false, null);

        if (!apiKey.IsValid())
            return (false, apiKey);

        return (true, apiKey);
    }
}
