using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;
