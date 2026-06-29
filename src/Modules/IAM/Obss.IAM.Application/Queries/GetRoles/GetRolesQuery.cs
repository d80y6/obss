using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<Result<IReadOnlyList<RoleDto>>>;
