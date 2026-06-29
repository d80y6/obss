using Obss.IAM.Application.Abstractions;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.IAM.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : EfRepository<Role>, IRoleRepository
{
    public RoleRepository(IamDbContext context)
        : base(context)
    {
    }
}
