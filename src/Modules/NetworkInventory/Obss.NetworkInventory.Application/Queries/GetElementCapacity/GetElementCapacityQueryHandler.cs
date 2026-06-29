using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetElementCapacity;

public sealed class GetElementCapacityQueryHandler : IRequestHandler<GetElementCapacityQuery, Result<IReadOnlyList<CapacityRecordDto>>>
{
    private readonly IRepository<CapacityRecord> _repository;

    public GetElementCapacityQueryHandler(IRepository<CapacityRecord> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CapacityRecordDto>>> Handle(GetElementCapacityQuery request, CancellationToken cancellationToken)
    {
        var allRecords = await _repository.GetAllAsync(cancellationToken);
        var records = allRecords
            .Where(r => r.ElementId == request.ElementId)
            .OrderByDescending(r => r.MeasuredAt)
            .ToList()
            .Adapt<List<CapacityRecordDto>>();

        return Result.Success<IReadOnlyList<CapacityRecordDto>>(records);
    }
}
