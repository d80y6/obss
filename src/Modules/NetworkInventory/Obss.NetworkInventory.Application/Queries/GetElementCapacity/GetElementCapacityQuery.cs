using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetElementCapacity;

public sealed record GetElementCapacityQuery(Guid ElementId) : IRequest<Result<IReadOnlyList<CapacityRecordDto>>>;
