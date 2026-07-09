using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.ActivateProduct;

public sealed record ActivateProductCommand(Guid Id) : IRequest<Result>;