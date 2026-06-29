using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.AutoReconcile;

public sealed class AutoReconcileCommandHandler : IRequestHandler<AutoReconcileCommand, Result>
{
    private readonly IPaymentReconciliationRepository _reconciliationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AutoReconcileCommandHandler> _logger;

    public AutoReconcileCommandHandler(
        IPaymentReconciliationRepository reconciliationRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ILogger<AutoReconcileCommandHandler> logger)
    {
        _reconciliationRepository = reconciliationRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AutoReconcileCommand request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetWithItemsAsync(request.ReconciliationId, cancellationToken);

        if (reconciliation is null)
            return Result.Failure(Error.NotFound("PaymentReconciliation", request.ReconciliationId));

        var payments = await _paymentRepository.GetAllAsync(cancellationToken);

        reconciliation.AutoReconcile(item =>
        {
            var matchedPayment = payments.FirstOrDefault(p =>
                p.Amount == item.Amount &&
                p.Currency == item.Currency &&
                (p.PaymentReference.Contains(item.ExternalReference, StringComparison.OrdinalIgnoreCase) ||
                 item.ExternalReference.Contains(p.PaymentReference, StringComparison.OrdinalIgnoreCase)));

            if (matchedPayment is not null)
            {
                _logger.LogInformation(
                    "Auto-matched item {ExternalReference} to payment {PaymentNumber} (invoice {InvoiceId}).",
                    item.ExternalReference, matchedPayment.PaymentNumber, matchedPayment.InvoiceId);

                return (matchedPayment.InvoiceId, matchedPayment.Id);
            }

            return ((Guid?, Guid?))(null, null);
        });

        await _reconciliationRepository.UpdateAsync(reconciliation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Auto-reconciliation completed for {ReconciliationId}.", request.ReconciliationId);

        return Result.Success();
    }
}
