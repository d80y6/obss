using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public sealed class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        var currentTenant = context.GetService<ICurrentTenant>();
        return new TenantModelCacheKey(context, currentTenant?.TenantId ?? "__default__", designTime);
    }

    public object Create(DbContext context)
    {
        return Create(context, false);
    }
}

public sealed class TenantModelCacheKey
{
    private readonly Type _contextType;
    private readonly string _tenantId;
    private readonly bool _designTime;

    public TenantModelCacheKey(DbContext context, string tenantId, bool designTime)
    {
        _contextType = context.GetType();
        _tenantId = tenantId;
        _designTime = designTime;
    }

    public override bool Equals(object? obj)
    {
        return obj is TenantModelCacheKey other
            && _contextType == other._contextType
            && _tenantId == other._tenantId
            && _designTime == other._designTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_contextType, _tenantId, _designTime);
    }
}
