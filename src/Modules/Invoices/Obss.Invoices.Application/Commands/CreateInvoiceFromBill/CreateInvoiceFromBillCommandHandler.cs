using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Application.Commands.CreateInvoiceFromBill;

public sealed class CreateInvoiceFromBillCommandHandler : IRequestHandler<CreateInvoiceFromBillCommand, Result<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IBillQuery _billQuery;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateInvoiceFromBillCommandHandler> _logger;

    public CreateInvoiceFromBillCommandHandler(
        IInvoiceRepository invoiceRepository,
        IBillQuery billQuery,
        IUnitOfWork unitOfWork,
        ILogger<CreateInvoiceFromBillCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _billQuery = billQuery;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceFromBillCommand request, CancellationToken cancellationToken)
    {
        var billResult = await _billQuery.GetBillByIdAsync(request.BillId, cancellationToken);
        if (billResult.IsFailure)
        {
            _logger.LogError("Bill {BillId} not found", request.BillId);
            return Result.Failure<InvoiceDto>(Error.NotFound("Bill", request.BillId.ToString()));
        }

        var bill = billResult.Value;
        var tenantId = TenantId.Create(request.TenantId);
        var invoiceNumber = await _invoiceRepository.GenerateNextInvoiceNumberAsync(cancellationToken);
        var invoiceDate = DateTime.UtcNow;
        var dueDate = invoiceDate.AddDays(30);

        var invoice = Invoice.Create(
            tenantId,
            invoiceNumber,
            bill.CustomerId,
            bill.CustomerName,
            request.CustomerEmail,
            request.CustomerAddress,
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

        _logger.LogInformation(
            "Invoice {InvoiceNumber} created from bill {BillId} for customer {CustomerId}",
            invoiceNumber, request.BillId, request.CustomerId);

        return Result.Success(invoice.Adapt<InvoiceDto>());
    }
}
