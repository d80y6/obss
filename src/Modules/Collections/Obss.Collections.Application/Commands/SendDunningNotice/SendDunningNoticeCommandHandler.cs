using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.SendDunningNotice;

public sealed class SendDunningNoticeCommandHandler : IRequestHandler<SendDunningNoticeCommand, Result<CollectionCaseDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;
    private readonly IDunningPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendDunningNoticeCommandHandler> _logger;

    public SendDunningNoticeCommandHandler(
        ICollectionCaseRepository caseRepository,
        IDunningPolicyRepository policyRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendDunningNoticeCommandHandler> logger)
    {
        _caseRepository = caseRepository;
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CollectionCaseDto>> Handle(SendDunningNoticeCommand request, CancellationToken cancellationToken)
    {
        var @case = await _caseRepository.GetByIdWithDetailsAsync(request.CaseId, cancellationToken);
        if (@case is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("CollectionCase", request.CaseId));

        var policy = await _policyRepository.GetActivePolicyAsync(cancellationToken);
        if (policy is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("DunningPolicy", "active"));

        var nextLevel = @case.CurrentDunningLevel + 1;
        if (nextLevel > policy.MaxDunningLevel)
            return Result.Failure<CollectionCaseDto>(Error.Validation("Maximum dunning level reached for this case."));

        var levelNames = new Dictionary<int, string>
        {
            { 1, "1st Notice" },
            { 2, "2nd Notice" },
            { 3, "Final Notice" },
            { 4, "Collection Agency Referral" },
            { 5, "Legal Action Notice" }
        };

        var noticeName = levelNames.GetValueOrDefault(nextLevel, $"Dunning Level {nextLevel}");
        var fee = policy.GetFeeForLevel(nextLevel);

        var action = CollectionAction.Create(
            @case.Id,
            CollectionActionType.DunningNotice,
            nextLevel,
            $"{noticeName} sent. {(fee > 0 ? $"Dunning fee of {fee} applied." : "")}",
            "System",
            DateTime.UtcNow.AddDays(policy.DaysBetweenActions));

        @case.AddAction(action);
        @case.AdvanceDunningLevel();

        if (fee > 0)
        {
            @case.UpdateOverdueAmount(@case.TotalOverdueAmount + fee);
        }

        await _caseRepository.UpdateAsync(@case, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Dunning notice level {Level} ({Name}) sent for case {CaseId}.",
            nextLevel,
            noticeName,
            request.CaseId);

        return Result.Success(@case.Adapt<CollectionCaseDto>());
    }
}
