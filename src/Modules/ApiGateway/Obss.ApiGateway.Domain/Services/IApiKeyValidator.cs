using Obss.ApiGateway.Domain.Entities;

namespace Obss.ApiGateway.Domain.Services;

public interface IApiKeyValidator
{
    (bool Valid, ApiKey? ApiKey) ValidateApiKey(string key);
}
