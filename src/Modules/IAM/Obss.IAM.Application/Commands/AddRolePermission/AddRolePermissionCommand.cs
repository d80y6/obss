using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.AddRolePermission;

public sealed record AddRolePermissionCommand(
    Guid RoleId,
    string Code,
    string Name,
    string? Description,
    string Module,
    string Resource,
    string Action) : IRequest<Result>;
