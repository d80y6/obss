using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.PortOutNumber;

public sealed record PortOutNumberCommand(Guid NumberId) : IRequest<Result>;
