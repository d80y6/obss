using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.RecordArrangementPayment;

public sealed class RecordArrangementPaymentCommandHandler : IRequestHandler<RecordArrangementPaymentCommand, Result<CollectionCaseDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordArrangementPaymentCommandHandler> _logger;

    public RecordArrangementPaymentCommandHandler(
        ICollectionCaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ILogger<RecordArrangementPaymentCommandHandler> logger)
    {
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CollectionCaseDto>> Handle(RecordArrangementPaymentCommand request, CancellationToken cancellationToken)
    {
        var allCases = await _caseRepository.GetActiveCasesAsync(cancellationToken);
        var @case = allCases.FirstOrDefault(c =>
            c.PaymentArrangements.Any(pa => pa.Id == request.PaymentArrangementId));

        if (@case is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("PaymentArrangement", request.PaymentArrangementId));

        var arrangement = @case.PaymentArrangements.FirstOrDefault(pa => pa.Id == request.PaymentArrangementId);
        if (arrangement is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("PaymentArrangement", request.PaymentArrangementId));

        try
        {
            arrangement.RecordPayment(request.Amount);
        }
        catch (Exception ex)
        {
            return Result.Failure<CollectionCaseDto>(Error.Validation(ex.Message));
        }

        if (arrangement.Status == Domain.ValueObjects.ArrangementStatus.Completed)
        {
            var remaining = @case.TotalOverdueAmount - request.Amount;
            @case.UpdateOverdueAmount(Math.Max(0, remaining));

            if (@case.TotalOverdueAmount <= 0)
            {
                @case.Resolve();
            }
        }

        var action = Domain.Entities.CollectionAction.Create(
            @case.Id,
            Domain.ValueObjects.CollectionActionType.PaymentArrangement,
            @case.CurrentDunningLevel,
            $"Payment of {request.Amount} recorded against arrangement {request.PaymentArrangementId}",
            "System",
            null);

        @case.AddAction(action);

        await _caseRepository.UpdateAsync(@case, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment of {Amount} recorded on arrangement {ArrangementId}.",
            request.Amount,
            request.PaymentArrangementId);

        return Result.Success(@case.Adapt<CollectionCaseDto>());
    }
}
