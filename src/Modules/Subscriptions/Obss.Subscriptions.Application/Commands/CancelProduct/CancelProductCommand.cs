using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.CancelProduct;

public sealed record CancelProductCommand(Guid Id) : IRequest<Result>;