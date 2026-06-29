using MediatR;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.CheckCapacityThreshold;

public sealed class CheckCapacityThresholdCommandHandler : IRequestHandler<CheckCapacityThresholdCommand, Result<bool>>
{
    private readonly IRepository<CapacityRecord> _repository;

    public CheckCapacityThresholdCommandHandler(IRepository<CapacityRecord> repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(CheckCapacityThresholdCommand request, CancellationToken cancellationToken)
    {
        var allRecords = await _repository.GetAllAsync(cancellationToken);
        var latest = allRecords
            .Where(r => r.ElementId == request.ElementId)
            .OrderByDescending(r => r.MeasuredAt)
            .FirstOrDefault();

        if (latest is null)
            return Result.Success(false);

        return Result.Success(latest.IsOverThreshold(request.Threshold));
    }
}
