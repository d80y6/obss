using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Domain.DomainServices;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.RateUsage;

public sealed class RateUsageCommandHandler : IRequestHandler<RateUsageCommand, Result<int>>
{
    private readonly IUsageRecordRepository _usageRecordRepository;
    private readonly IRatingRuleRepository _ratingRuleRepository;
    private readonly IRatingEngine _ratingEngine;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RateUsageCommandHandler> _logger;

    public RateUsageCommandHandler(
        IUsageRecordRepository usageRecordRepository,
        IRatingRuleRepository ratingRuleRepository,
        IRatingEngine ratingEngine,
        IUnitOfWork unitOfWork,
        ILogger<RateUsageCommandHandler> logger)
    {
        _usageRecordRepository = usageRecordRepository;
        _ratingRuleRepository = ratingRuleRepository;
        _ratingEngine = ratingEngine;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RateUsageCommand request, CancellationToken cancellationToken)
    {
        var unratedRecords = await _usageRecordRepository.GetUnratedRecordsAsync(cancellationToken);

        if (unratedRecords.Count == 0)
            return Result.Success(0);

        var activeRules = await _ratingRuleRepository.GetActiveRulesOrderedByPriorityAsync(cancellationToken);

        var ratedCount = 0;

        foreach (var record in unratedRecords)
        {
            try
            {
                var matchingRule = FindMatchingRule(record, activeRules);

                if (matchingRule is null)
                {
                    record.MarkAsError("No matching rating rule found.");
                    await _usageRecordRepository.UpdateAsync(record, cancellationToken);
                    continue;
                }

                var result = _ratingEngine.Rate(record, matchingRule);
                record.MarkAsRated(result.Amount, result.RuleApplied);
                await _usageRecordRepository.UpdateAsync(record, cancellationToken);
                ratedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rate record {RecordId}", record.Id);
                record.MarkAsError($"Rating failed: {ex.Message}");
                await _usageRecordRepository.UpdateAsync(record, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ratedCount);
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
