using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Invoices.Application.Commands.FinalizeInvoice;

public sealed class FinalizeInvoiceCommandHandler : IRequestHandler<FinalizeInvoiceCommand, Result>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FinalizeInvoiceCommandHandler> _logger;

    public FinalizeInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<FinalizeInvoiceCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(FinalizeInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure(Error.NotFound("Invoice", request.InvoiceId));

        invoice.MarkAsFinalized();

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} finalized.", invoice.InvoiceNumber);

        return Result.Success();
    }
}
