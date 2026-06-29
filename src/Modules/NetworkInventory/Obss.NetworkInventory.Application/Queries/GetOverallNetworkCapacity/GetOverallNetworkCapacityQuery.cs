using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetOverallNetworkCapacity;

public sealed record GetOverallNetworkCapacityQuery() : IRequest<Result<IReadOnlyList<CapacityRecordDto>>>;
