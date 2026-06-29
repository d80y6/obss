using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetUnmatchedResources;

public sealed record GetUnmatchedResourcesQuery(Guid TenantId) : IRequest<Result<List<DiscoveryJobDto>>>;
