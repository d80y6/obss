using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.RemoveRolePermission;

public sealed record RemoveRolePermissionCommand(
    Guid RoleId,
    Guid PermissionId) : IRequest<Result>;
