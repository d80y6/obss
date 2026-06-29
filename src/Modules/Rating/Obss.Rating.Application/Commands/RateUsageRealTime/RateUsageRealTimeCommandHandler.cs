using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Domain.DomainServices;
using Obss.Rating.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.RateUsageRealTime;

public sealed class RateUsageRealTimeCommandHandler : IRequestHandler<RateUsageRealTimeCommand, Result<RealTimeRatingResult>>
{
    private readonly IUsageRecordRepository _usageRecordRepository;
    private readonly IRatingRuleRepository _ratingRuleRepository;
    private readonly IRatingEngine _ratingEngine;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxService _outboxService;
    private readonly ILogger<RateUsageRealTimeCommandHandler> _logger;

    public RateUsageRealTimeCommandHandler(
        IUsageRecordRepository usageRecordRepository,
        IRatingRuleRepository ratingRuleRepository,
        IRatingEngine ratingEngine,
        IUnitOfWork unitOfWork,
        IOutboxService outboxService,
        ILogger<RateUsageRealTimeCommandHandler> logger)
    {
        _usageRecordRepository = usageRecordRepository;
        _ratingRuleRepository = ratingRuleRepository;
        _ratingEngine = ratingEngine;
        _unitOfWork = unitOfWork;
        _outboxService = outboxService;
        _logger = logger;
    }

    public async Task<Result<RealTimeRatingResult>> Handle(RateUsageRealTimeCommand request, CancellationToken cancellationToken)
    {
        var record = await _usageRecordRepository.GetByIdAsync(request.UsageRecordId, cancellationToken);
        if (record is null)
            return Result.Failure<RealTimeRatingResult>(Error.NotFound("UsageRecord", request.UsageRecordId));

        if (record.Status != Domain.ValueObjects.UsageStatus.Unrated)
            return Result.Failure<RealTimeRatingResult>(Error.Validation($"Usage record {request.UsageRecordId} is already rated (status: {record.Status})."));

        var activeRules = await _ratingRuleRepository.GetActiveRulesOrderedByPriorityAsync(cancellationToken);

        var matchingRule = FindMatchingRule(record, activeRules);
        if (matchingRule is null)
            return Result.Failure<RealTimeRatingResult>(Error.Validation("No matching rating rule found for this usage record."));

        var result = _ratingEngine.Rate(record, matchingRule);
        record.MarkAsRated(result.Amount, result.RuleApplied);

        var integrationEvent = new UsageRecordRatedIntegrationEvent(
            record.Id,
            record.SubscriptionId,
            result.Amount,
            record.Currency,
            record.RecordType);

        await _outboxService.AddAsync(integrationEvent, cancellationToken);

        await _usageRecordRepository.UpdateAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Real-time rating: Usage record {RecordId} rated at {Amount} {Currency}",
            record.Id, result.Amount, record.Currency);

        return Result.Success(new RealTimeRatingResult(
            record.Id,
            result.Amount,
            record.Currency,
            result.RuleApplied));
    }

    private static Domain.Entities.RatingRule? FindMatchingRule(
        Domain.Entities.UsageRecord record,
        IReadOnlyList<Domain.Entities.RatingRule> rules)
    {
        return rules.FirstOrDefault(r =>
            r.IsActive &&
            (!r.ProductId.HasValue || r.ProductId == record.ServiceId) &&
            (!r.OfferId.HasValue || r.OfferId == record.SubscriptionId));
    }
}
