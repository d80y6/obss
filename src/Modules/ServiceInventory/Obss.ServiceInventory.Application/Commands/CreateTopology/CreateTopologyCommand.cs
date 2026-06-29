using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.CreateTopology;

public sealed record CreateTopologyCommand(
    Guid ServiceId,
    string TopologyType) : IRequest<Result<ServiceTopologyDto>>;
