using Mapster;
using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    private readonly IRepository<Role> _roleRepository;

    public GetRoleByIdQueryHandler(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
            return Result.Failure<RoleDto>(Error.NotFound(nameof(Role), request.RoleId));

        return Result.Success(role.Adapt<RoleDto>());
    }
}
