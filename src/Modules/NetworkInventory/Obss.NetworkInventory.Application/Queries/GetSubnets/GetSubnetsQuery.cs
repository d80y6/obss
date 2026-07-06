using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetSubnets;

public sealed record GetSubnetsQuery(
    string? Status,
    int? VLANId,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<SubnetDto>>>;
