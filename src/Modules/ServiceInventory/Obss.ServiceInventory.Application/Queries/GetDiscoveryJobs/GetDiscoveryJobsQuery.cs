using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetDiscoveryJobs;

public sealed record GetDiscoveryJobsQuery(Guid TenantId) : IRequest<Result<List<DiscoveryJobDto>>>;
