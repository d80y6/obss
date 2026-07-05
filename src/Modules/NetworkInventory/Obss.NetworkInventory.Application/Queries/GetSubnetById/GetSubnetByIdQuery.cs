using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetSubnetById;

public sealed record GetSubnetByIdQuery(Guid Id) : IRequest<Result<SubnetDto>>;
