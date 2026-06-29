using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ApiGateway.Application.Abstractions;

public interface IPartnerRepository : IRepository<Partner>
{
    Task<Partner?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
