using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.SuspendProduct;

public sealed record SuspendProductCommand(Guid Id) : IRequest<Result>;