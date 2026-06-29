using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Invoices.Application.Commands.MarkInvoiceAsSent;

public sealed class MarkInvoiceAsSentCommandHandler : IRequestHandler<MarkInvoiceAsSentCommand, Result>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkInvoiceAsSentCommandHandler> _logger;

    public MarkInvoiceAsSentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<MarkInvoiceAsSentCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(MarkInvoiceAsSentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure(Error.NotFound("Invoice", request.InvoiceId));

        invoice.MarkAsSent();

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} marked as sent.", invoice.InvoiceNumber);

        return Result.Success();
    }
}
