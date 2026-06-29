using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ApiGateway.Application.Abstractions;

public interface IApiKeyRepository : IRepository<ApiKey>
{
    Task<ApiKey?> GetByKeyAsync(string hashedKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiKey>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken = default);
}
