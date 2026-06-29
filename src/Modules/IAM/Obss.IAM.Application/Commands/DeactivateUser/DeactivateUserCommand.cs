using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest<Result>;
