using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description) : IRequest<Result<RoleDto>>;
