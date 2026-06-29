using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServiceTopology;

public sealed record GetServiceTopologyQuery(Guid ServiceId) : IRequest<Result<ServiceTopologyDto>>;
