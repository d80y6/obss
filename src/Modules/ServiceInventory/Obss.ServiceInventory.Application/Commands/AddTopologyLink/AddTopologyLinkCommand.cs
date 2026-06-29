using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.AddTopologyLink;

public sealed record AddTopologyLinkCommand(
    Guid ServiceTopologyId,
    Guid SourceServiceId,
    Guid TargetServiceId,
    string LinkType,
    string Direction,
    string? Attributes) : IRequest<Result<TopologyLinkDto>>;
