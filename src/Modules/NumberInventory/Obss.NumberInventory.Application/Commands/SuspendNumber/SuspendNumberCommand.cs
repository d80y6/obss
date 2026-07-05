using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.SuspendNumber;

public sealed record SuspendNumberCommand(Guid NumberId) : IRequest<Result>;
