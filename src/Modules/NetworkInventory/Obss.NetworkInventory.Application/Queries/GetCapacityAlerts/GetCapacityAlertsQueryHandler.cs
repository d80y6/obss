using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetCapacityAlerts;

public sealed class GetCapacityAlertsQueryHandler : IRequestHandler<GetCapacityAlertsQuery, Result<IReadOnlyList<CapacityRecordDto>>>
{
    private readonly IRepository<CapacityRecord> _repository;

    public GetCapacityAlertsQueryHandler(IRepository<CapacityRecord> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CapacityRecordDto>>> Handle(GetCapacityAlertsQuery request, CancellationToken cancellationToken)
    {
        var allRecords = await _repository.GetAllAsync(cancellationToken);
        var alerts = allRecords
            .GroupBy(r => r.ElementId)
            .Select(g => g.OrderByDescending(r => r.MeasuredAt).First())
            .Where(r => r.UtilizationPercent > 80)
            .OrderByDescending(r => r.UtilizationPercent)
            .ToList()
            .Adapt<List<CapacityRecordDto>>();

        return Result.Success<IReadOnlyList<CapacityRecordDto>>(alerts);
    }
}
