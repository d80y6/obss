using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.RecordCapacity;

public sealed class RecordCapacityCommandHandler : IRequestHandler<RecordCapacityCommand, Result<CapacityRecordDto>>
{
    private readonly IRepository<CapacityRecord> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordCapacityCommandHandler(IRepository<CapacityRecord> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CapacityRecordDto>> Handle(RecordCapacityCommand request, CancellationToken cancellationToken)
    {
        var capacityType = Enum.Parse<CapacityType>(request.CapacityType);

        var record = new CapacityRecord(
            Guid.NewGuid(),
            request.ElementId,
            request.InterfaceId,
            capacityType,
            request.TotalCapacity,
            request.UsedCapacity);

        await _repository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(record.Adapt<CapacityRecordDto>());
    }
}
