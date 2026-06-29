using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.RegisterONT;

public sealed record RegisterONTCommand(
    Guid OLTId,
    int PortNumber) : IRequest<Result>;
