using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetDownstreamServices;

public sealed record GetDownstreamServicesQuery(Guid ServiceId) : IRequest<Result<List<Guid>>>;
