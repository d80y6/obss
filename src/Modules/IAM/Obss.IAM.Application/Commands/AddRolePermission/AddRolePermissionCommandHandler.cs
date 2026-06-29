using MediatR;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Commands.AddRolePermission;

public sealed class AddRolePermissionCommandHandler : IRequestHandler<AddRolePermissionCommand, Result>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRolePermissionCommandHandler(IRepository<Role> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
            return Result.Failure(Error.NotFound(nameof(Role), request.RoleId));

        var permission = new Permission(
            Guid.NewGuid(),
            request.Code,
            request.Name,
            request.Description,
            request.Module,
            request.Resource,
            request.Action);

        role.AddPermission(permission);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
