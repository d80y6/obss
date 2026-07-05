using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.DeleteDunningPolicy;

public sealed record DeleteDunningPolicyCommand(Guid Id) : IRequest<Result>;
