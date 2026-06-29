using Mapster;
using MediatR;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetRoles;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);

        var result = roles.Adapt<List<RoleDto>>();
        return Result.Success<IReadOnlyList<RoleDto>>(result);
    }
}
