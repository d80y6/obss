using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string TenantId,
    string Name,
    string? Description) : IRequest<Result<RoleDto>>;
