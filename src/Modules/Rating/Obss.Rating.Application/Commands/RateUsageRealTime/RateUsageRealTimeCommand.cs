using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.RateUsageRealTime;

public sealed record RateUsageRealTimeCommand(Guid UsageRecordId) : IRequest<Result<RealTimeRatingResult>>;

public sealed record RealTimeRatingResult(
    Guid RecordId,
    decimal Amount,
    string Currency,
    Guid RuleApplied);
