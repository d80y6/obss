using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetSubnets;

public sealed record GetSubnetsQuery(
    string? Status,
    int? VLANId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<SubnetDto>>>;
