using MediatR;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Commands.RemoveRolePermission;

public sealed class RemoveRolePermissionCommandHandler : IRequestHandler<RemoveRolePermissionCommand, Result>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRolePermissionCommandHandler(
        IRepository<Role> roleRepository,
        IRepository<Permission> permissionRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
            return Result.Failure(Error.NotFound(nameof(Role), request.RoleId));

        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);

        if (permission is null)
            return Result.Failure(Error.NotFound(nameof(Permission), request.PermissionId));

        role.RemovePermission(permission);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
