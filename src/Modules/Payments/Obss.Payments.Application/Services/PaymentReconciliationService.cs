using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Services;

public sealed class PaymentReconciliationService : IPaymentReconciliationService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentReconciliationService> _logger;

    public PaymentReconciliationService(
        IPaymentRepository paymentRepository,
        ILogger<PaymentReconciliationService> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<ReconciliationResult>> MatchPaymentToInvoiceAsync(
        Guid paymentId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure<ReconciliationResult>(Error.NotFound("Payment", paymentId));
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            return Result.Failure<ReconciliationResult>(
                Error.Validation($"Cannot match payment in '{payment.Status}' state."));
        }

        payment.AllocateToInvoice(invoiceId, payment.Amount);
        _logger.LogInformation(
            "Payment {PaymentId} matched to invoice {InvoiceId}: {Amount} {Currency}",
            paymentId, invoiceId, payment.Amount, payment.Currency);

        var result = new ReconciliationResult(
            true,
            paymentId.ToString(),
            invoiceId.ToString(),
            payment.Amount,
            0,
            "Matched");

        return Result.Success(result);
    }

    public async Task<Result<ReconciliationResult>> HandlePartialPaymentAsync(
        Guid paymentId,
        Guid invoiceId,
        decimal amountApplied,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure<ReconciliationResult>(Error.NotFound("Payment", paymentId));
        }

        if (amountApplied <= 0 || amountApplied > payment.Amount)
        {
            return Result.Failure<ReconciliationResult>(
                Error.Validation($"Invalid partial amount {amountApplied}. Payment amount is {payment.Amount}."));
        }

        payment.AllocateToInvoice(invoiceId, amountApplied);
        var remaining = payment.Amount - amountApplied;

        _logger.LogInformation(
            "Partial payment {PaymentId}: {Applied} applied to invoice {InvoiceId}, {Remaining} remaining",
            paymentId, amountApplied, invoiceId, remaining);

        var result = new ReconciliationResult(
            true,
            paymentId.ToString(),
            invoiceId.ToString(),
            amountApplied,
            remaining,
            "PartiallyMatched");

        return Result.Success(result);
    }

    public async Task<Result<ReconciliationResult>> HandleOverpaymentAsync(
        Guid paymentId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure<ReconciliationResult>(Error.NotFound("Payment", paymentId));
        }

        payment.AllocateToInvoice(invoiceId, payment.Amount);
        var creditBalance = 0m;

        _logger.LogInformation(
            "Overpayment {PaymentId}: {Amount} fully applied to invoice {InvoiceId}. Credit balance: {Credit}",
            paymentId, payment.Amount, invoiceId, creditBalance);

        var result = new ReconciliationResult(
            true,
            paymentId.ToString(),
            invoiceId.ToString(),
            payment.Amount,
            creditBalance,
            "OverpaymentApplied");

        return Result.Success(result);
    }

    public async Task<ReconciliationReport> GenerateReconciliationReportAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetFilteredAsync(null, null, from, to, 0, int.MaxValue, cancellationToken);

        var details = new List<ReconciliationResult>();
        var matchedAmount = 0m;
        var unmatchedAmount = 0m;
        var matchedCount = 0;
        var partialCount = 0;
        var overpaymentCount = 0;

        foreach (var payment in payments)
        {
            var totalAllocated = payment.Allocations.Sum(a => a.Amount);
            var isMatched = totalAllocated > 0;
            var isPartial = isMatched && totalAllocated < payment.Amount;

            if (isMatched)
            {
                matchedAmount += totalAllocated;
                matchedCount++;

                if (isPartial)
                {
                    partialCount++;
                }

                if (totalAllocated >= payment.Amount)
                {
                    overpaymentCount++;
                }

                details.Add(new ReconciliationResult(
                    true,
                    payment.Id.ToString(),
                    payment.InvoiceId?.ToString() ?? "unknown",
                    totalAllocated,
                    payment.Amount - totalAllocated,
                    isPartial ? "PartiallyMatched" : "Matched"));
            }
            else
            {
                unmatchedAmount += payment.Amount;
                details.Add(new ReconciliationResult(
                    false,
                    payment.Id.ToString(),
                    string.Empty,
                    0,
                    payment.Amount,
                    "Unmatched"));
            }
        }

        var report = new ReconciliationReport(
            from,
            to,
            payments.Count,
            matchedCount,
            partialCount,
            overpaymentCount,
            payments.Count - matchedCount,
            payments.Sum(p => p.Amount),
            matchedAmount,
            unmatchedAmount,
            details.AsReadOnly());

        _logger.LogInformation(
            "Reconciliation report {From} - {To}: {Matched}/{Total} matched, {Amount} reconciled",
            from.ToShortDateString(), to.ToShortDateString(), matchedCount, payments.Count, matchedAmount);

        return report;
    }
}
