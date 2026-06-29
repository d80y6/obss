using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetRoleById;

public sealed record GetRoleByIdQuery(Guid RoleId) : IRequest<Result<RoleDto>>;
