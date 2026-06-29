using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.ReconcilePayment;

public sealed class ReconcilePaymentCommandHandler : IRequestHandler<ReconcilePaymentCommand, Result>
{
    private readonly IPaymentReconciliationRepository _reconciliationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReconcilePaymentCommandHandler> _logger;

    public ReconcilePaymentCommandHandler(
        IPaymentReconciliationRepository reconciliationRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReconcilePaymentCommandHandler> logger)
    {
        _reconciliationRepository = reconciliationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ReconcilePaymentCommand request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetWithItemsAsync(request.ReconciliationId, cancellationToken);

        if (reconciliation is null)
            return Result.Failure(Error.NotFound("PaymentReconciliation", request.ReconciliationId));

        reconciliation.MarkItemMatched(request.ItemId, request.MatchedInvoiceId, request.MatchedPaymentId);

        await _reconciliationRepository.UpdateAsync(reconciliation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reconciliation item {ItemId} matched to invoice {InvoiceId} / payment {PaymentId}.",
            request.ItemId, request.MatchedInvoiceId, request.MatchedPaymentId);

        return Result.Success();
    }
}
