using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetOverallNetworkCapacity;

public sealed class GetOverallNetworkCapacityQueryHandler : IRequestHandler<GetOverallNetworkCapacityQuery, Result<IReadOnlyList<CapacityRecordDto>>>
{
    private readonly IRepository<CapacityRecord> _repository;

    public GetOverallNetworkCapacityQueryHandler(IRepository<CapacityRecord> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CapacityRecordDto>>> Handle(GetOverallNetworkCapacityQuery request, CancellationToken cancellationToken)
    {
        var allRecords = await _repository.GetAllAsync(cancellationToken);
        var latest = allRecords
            .GroupBy(r => r.ElementId)
            .Select(g => g.OrderByDescending(r => r.MeasuredAt).First())
            .ToList()
            .Adapt<List<CapacityRecordDto>>();

        return Result.Success<IReadOnlyList<CapacityRecordDto>>(latest);
    }
}
