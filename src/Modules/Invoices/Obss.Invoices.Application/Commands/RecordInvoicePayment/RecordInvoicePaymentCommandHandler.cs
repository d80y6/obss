using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Invoices.Application.Commands.RecordInvoicePayment;

public sealed class RecordInvoicePaymentCommandHandler : IRequestHandler<RecordInvoicePaymentCommand, Result>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordInvoicePaymentCommandHandler> _logger;

    public RecordInvoicePaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<RecordInvoicePaymentCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RecordInvoicePaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure(Error.NotFound("Invoice", request.InvoiceId));

        invoice.RecordPayment(request.Amount, request.PaymentReference);

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment of {Amount} recorded on invoice {InvoiceNumber}. Reference: {Reference}",
            request.Amount, invoice.InvoiceNumber, request.PaymentReference);

        return Result.Success();
    }
}
