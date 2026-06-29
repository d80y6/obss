using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ApiGateway.Application.Abstractions;

public interface IApiRouteRepository : IRepository<ApiRoute>
{
    Task<IReadOnlyList<ApiRoute>> GetActiveRoutesAsync(CancellationToken cancellationToken = default);
    Task<ApiRoute?> GetByPathAndMethodAsync(string path, string method, CancellationToken cancellationToken = default);
}
