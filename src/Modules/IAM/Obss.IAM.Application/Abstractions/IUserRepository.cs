using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Abstractions;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetFilteredAsync(
        string? tenantId,
        bool? isActive,
        string? searchTerm,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, string tenantId, CancellationToken cancellationToken = default);
}
