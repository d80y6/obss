using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.UpdateLinkStatus;

public sealed record UpdateLinkStatusCommand(
    Guid Id,
    string Status,
    string? Reason) : IRequest<Result>;
