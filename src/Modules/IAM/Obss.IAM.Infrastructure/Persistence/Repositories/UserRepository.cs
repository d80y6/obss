using Microsoft.EntityFrameworkCore;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.IAM.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(IamDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetFilteredAsync(
        string? tenantId,
        bool? isActive,
        string? searchTerm,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            var tid = TenantId.Create(tenantId);
            query = query.Where(u => u.TenantId == tid);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Username.Contains(searchTerm) ||
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm));
        }

        query = query
            .OrderBy(u => u.Username)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailObj = Email.Create(email);
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == emailObj, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, string tenantId, CancellationToken cancellationToken = default)
    {
        var tid = TenantId.Create(tenantId);
        return await DbSet
            .FirstOrDefaultAsync(u => u.Username == username && u.TenantId == tid, cancellationToken);
    }
}
