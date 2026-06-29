using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Invoices.Application.Commands.CancelInvoice;

public sealed class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Result>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelInvoiceCommandHandler> _logger;

    public CancelInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelInvoiceCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure(Error.NotFound("Invoice", request.InvoiceId));

        invoice.Cancel(request.Reason);

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} cancelled. Reason: {Reason}", invoice.InvoiceNumber, request.Reason);

        return Result.Success();
    }
}
