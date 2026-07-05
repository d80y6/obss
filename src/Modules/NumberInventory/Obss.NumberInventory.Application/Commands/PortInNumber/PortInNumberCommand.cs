using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.PortInNumber;

public sealed record PortInNumberCommand(Guid NumberId, Guid? CustomerId) : IRequest<Result>;
