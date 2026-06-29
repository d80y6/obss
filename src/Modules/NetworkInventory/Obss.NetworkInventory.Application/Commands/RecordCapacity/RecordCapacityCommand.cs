using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.RecordCapacity;

public sealed record RecordCapacityCommand(
    Guid ElementId,
    Guid? InterfaceId,
    string CapacityType,
    decimal TotalCapacity,
    decimal UsedCapacity) : IRequest<Result<CapacityRecordDto>>;
