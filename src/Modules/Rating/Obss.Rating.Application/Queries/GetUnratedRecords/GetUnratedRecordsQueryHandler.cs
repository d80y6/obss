using Mapster;
using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetUnratedRecords;

public sealed class GetUnratedRecordsQueryHandler : IRequestHandler<GetUnratedRecordsQuery, Result<IReadOnlyList<UsageRecordDto>>>
{
    private readonly IUsageRecordRepository _usageRecordRepository;

    public GetUnratedRecordsQueryHandler(IUsageRecordRepository usageRecordRepository)
    {
        _usageRecordRepository = usageRecordRepository;
    }

    public async Task<Result<IReadOnlyList<UsageRecordDto>>> Handle(GetUnratedRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _usageRecordRepository.GetUnratedRecordsAsync(cancellationToken);
        return Result.Success(records.Adapt<IReadOnlyList<UsageRecordDto>>());
    }
}
