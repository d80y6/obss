using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.CreatePaymentArrangement;

public sealed class CreatePaymentArrangementCommandHandler : IRequestHandler<CreatePaymentArrangementCommand, Result<CollectionCaseDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePaymentArrangementCommandHandler> _logger;

    public CreatePaymentArrangementCommandHandler(
        ICollectionCaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreatePaymentArrangementCommandHandler> logger)
    {
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CollectionCaseDto>> Handle(CreatePaymentArrangementCommand request, CancellationToken cancellationToken)
    {
        var @case = await _caseRepository.GetByIdWithDetailsAsync(request.CollectionCaseId, cancellationToken);
        if (@case is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("CollectionCase", request.CollectionCaseId));

        if (!Enum.TryParse<PaymentFrequency>(request.Frequency, true, out var frequency))
            return Result.Failure<CollectionCaseDto>(Error.Validation($"Invalid payment frequency: '{request.Frequency}'."));

        try
        {
            var arrangement = @case.CreatePaymentArrangement(
                request.TotalAmount,
                request.InstallmentCount,
                request.InstallmentAmount,
                frequency,
                request.FirstPaymentDate);

            arrangement.Activate();

            var action = Domain.Entities.CollectionAction.Create(
                @case.Id,
                CollectionActionType.PaymentArrangement,
                @case.CurrentDunningLevel,
                $"Payment arrangement created: {request.InstallmentCount} installments of {request.InstallmentAmount}",
                "System",
                null);

            @case.AddAction(action);
        }
        catch (Exception ex)
        {
            return Result.Failure<CollectionCaseDto>(Error.Conflict(ex.Message));
        }

        await _caseRepository.UpdateAsync(@case, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment arrangement created for case {CaseId}: {Count} installments of {Amount}.",
            request.CollectionCaseId,
            request.InstallmentCount,
            request.InstallmentAmount);

        return Result.Success(@case.Adapt<CollectionCaseDto>());
    }
}
