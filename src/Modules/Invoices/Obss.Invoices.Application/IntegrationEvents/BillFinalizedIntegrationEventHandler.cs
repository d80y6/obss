using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Application.IntegrationEvents;

public sealed class BillFinalizedIntegrationEventHandler : INotificationHandler<BillFinalizedIntegrationEvent>
{
    private readonly IBillQuery _billQuery;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillFinalizedIntegrationEventHandler> _logger;

    public BillFinalizedIntegrationEventHandler(
        IBillQuery billQuery,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<BillFinalizedIntegrationEventHandler> logger)
    {
        _billQuery = billQuery;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BillFinalizedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling BillFinalizedIntegrationEvent for BillId: {BillId}", notification.BillId);

        var billResult = await _billQuery.GetBillByIdAsync(notification.BillId, cancellationToken);
        if (billResult.IsFailure)
        {
            _logger.LogError("Bill {BillId} not found", notification.BillId);
            return;
        }

        var bill = billResult.Value;
        var invoiceNumber = await _invoiceRepository.GenerateNextInvoiceNumberAsync(cancellationToken);
        var invoiceDate = DateTime.UtcNow;
        var dueDate = invoiceDate.AddDays(30);

        var invoice = Invoice.Create(
            notification.TenantId,
            invoiceNumber,
            bill.CustomerId,
            bill.CustomerName,
            string.Empty,
            string.Empty,
            invoiceDate,
            dueDate,
            bill.Currency);

        foreach (var line in bill.Lines)
        {
            if (!Enum.TryParse<LineType>(line.LineType, true, out var lineType))
                continue;

            invoice.AddLine(new InvoiceLine(
                Guid.NewGuid(),
                invoice.Id,
                line.BillId,
                line.Id,
                lineType,
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.TotalAmount,
                line.TaxAmount,
                line.TaxRate,
                line.Currency));
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} created from bill {BillId}",
            invoiceNumber, notification.BillId);
    }
}
