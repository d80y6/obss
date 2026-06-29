using MediatR;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Commands.AssignRole;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleCommandHandler(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(Error.NotFound(nameof(User), request.UserId));

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
            return Result.Failure(Error.NotFound(nameof(Role), request.RoleId));

        user.AssignRole(request.RoleId, request.AssignedBy);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
