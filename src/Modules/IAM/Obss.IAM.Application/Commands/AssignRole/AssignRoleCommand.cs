using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.AssignRole;

public sealed record AssignRoleCommand(
    Guid UserId,
    Guid RoleId,
    Guid AssignedBy) : IRequest<Result>;
