using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.Rating.Domain.DomainServices;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.Events;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.SubmitUsage;

public sealed class SubmitUsageCommandHandler : IRequestHandler<SubmitUsageCommand, Result<UsageRecordDto>>
{
    private readonly IUsageRecordRepository _usageRecordRepository;
    private readonly IRatingRuleRepository _ratingRuleRepository;
    private readonly IRatingEngine _ratingEngine;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly IOutboxService _outboxService;
    private readonly ILogger<SubmitUsageCommandHandler> _logger;

    public SubmitUsageCommandHandler(
        IUsageRecordRepository usageRecordRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        IRatingRuleRepository ratingRuleRepository,
        IRatingEngine ratingEngine,
        IOutboxService outboxService,
        ILogger<SubmitUsageCommandHandler> logger)
    {
        _usageRecordRepository = usageRecordRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _ratingRuleRepository = ratingRuleRepository;
        _ratingEngine = ratingEngine;
        _outboxService = outboxService;
        _logger = logger;
    }

    public async Task<Result<UsageRecordDto>> Handle(SubmitUsageCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
            return Result.Failure<UsageRecordDto>(Error.Unauthorized("Tenant context is required."));

        if (!Enum.TryParse<RecordType>(request.RecordType, true, out var recordType))
            return Result.Failure<UsageRecordDto>(Error.Validation($"Invalid record type: '{request.RecordType}'. Must be Voice, Data, SMS, or Session."));

        if (request.EndTime <= request.StartTime)
            return Result.Failure<UsageRecordDto>(Error.Validation("End time must be after start time."));

        var record = UsageRecord.Create(
            tenantId,
            request.SubscriptionId,
            request.ServiceId,
            recordType,
            request.UsageType,
            request.StartTime,
            request.EndTime,
            request.Duration,
            request.Volume,
            request.SourceIdentifier,
            request.DestinationIdentifier,
            request.Currency);

        await _usageRecordRepository.AddAsync(record, cancellationToken);

        if (request.RateImmediately)
        {
            var rateResult = await RateRealTime(record, cancellationToken);
            if (rateResult.IsFailure)
            {
                _logger.LogWarning("Real-time rating failed for record {RecordId}: {Error}", record.Id, rateResult.Error.Description);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(record.Adapt<UsageRecordDto>());
    }

    private async Task<Result> RateRealTime(UsageRecord record, CancellationToken cancellationToken)
    {
        var activeRules = await _ratingRuleRepository.GetActiveRulesOrderedByPriorityAsync(cancellationToken);

        var matchingRule = activeRules.FirstOrDefault(r =>
            r.IsActive &&
            (!r.ProductId.HasValue || r.ProductId == record.ServiceId) &&
            (!r.OfferId.HasValue || r.OfferId == record.SubscriptionId));

        if (matchingRule is null)
        {
            record.MarkAsError("No matching rating rule found for real-time rating.");
            await _usageRecordRepository.UpdateAsync(record, cancellationToken);
            return Result.Failure(Error.Validation("No matching rating rule found for real-time rating."));
        }

        var result = _ratingEngine.Rate(record, matchingRule);
        record.MarkAsRated(result.Amount, result.RuleApplied);

        var integrationEvent = new UsageRecordRatedIntegrationEvent(
            record.Id,
            record.SubscriptionId,
            result.Amount,
            record.Currency,
            record.RecordType);

        await _outboxService.AddAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Real-time rating: Usage record {RecordId} rated at {Amount} {Currency}",
            record.Id, result.Amount, record.Currency);

        return Result.Success();
    }
}
