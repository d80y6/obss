using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.ReserveNumber;

public sealed record ReserveNumberCommand(Guid NumberId) : IRequest<Result>;
