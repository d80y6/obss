using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetUpstreamServices;

public sealed record GetUpstreamServicesQuery(Guid ServiceId) : IRequest<Result<List<Guid>>>;
