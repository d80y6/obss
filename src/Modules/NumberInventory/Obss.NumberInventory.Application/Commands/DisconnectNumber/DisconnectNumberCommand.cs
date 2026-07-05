using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.DisconnectNumber;

public sealed record DisconnectNumberCommand(Guid NumberId) : IRequest<Result>;
