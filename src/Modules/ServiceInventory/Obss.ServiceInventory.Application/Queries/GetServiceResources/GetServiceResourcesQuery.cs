using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServiceResources;

public sealed record GetServiceResourcesQuery(Guid ServiceId) : IRequest<Result<IReadOnlyList<ServiceResourceDto>>>;
