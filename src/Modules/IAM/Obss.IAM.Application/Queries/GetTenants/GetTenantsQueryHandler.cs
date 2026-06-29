using Mapster;
using MediatR;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetTenants;

public sealed class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, Result<IReadOnlyList<TenantDto>>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<IReadOnlyList<TenantDto>>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        var result = tenants.Adapt<List<TenantDto>>();
        return Result.Success<IReadOnlyList<TenantDto>>(result);
    }
}