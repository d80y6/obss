using Mapster;
using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetUsageBySubscription;

public sealed class GetUsageBySubscriptionQueryHandler : IRequestHandler<GetUsageBySubscriptionQuery, Result<IReadOnlyList<UsageRecordDto>>>
{
    private readonly IUsageRecordRepository _usageRecordRepository;

    public GetUsageBySubscriptionQueryHandler(IUsageRecordRepository usageRecordRepository)
    {
        _usageRecordRepository = usageRecordRepository;
    }

    public async Task<Result<IReadOnlyList<UsageRecordDto>>> Handle(GetUsageBySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var records = await _usageRecordRepository.GetBySubscriptionAsync(
            request.SubscriptionId,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(records.Adapt<IReadOnlyList<UsageRecordDto>>());
    }
}
