using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Application.Commands.IssueCreditNote;

public sealed class IssueCreditNoteCommandHandler : IRequestHandler<IssueCreditNoteCommand, Result<CreditNoteDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IssueCreditNoteCommandHandler> _logger;

    public IssueCreditNoteCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<IssueCreditNoteCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CreditNoteDto>> Handle(IssueCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure<CreditNoteDto>(Error.NotFound("Invoice", request.InvoiceId));

        var totalAmount = request.Lines.Sum(l => l.Amount);
        invoice.IssueCreditNote(totalAmount, request.Reason);

        var creditNoteNumber = $"CN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20];

        var creditNote = CreditNote.Create(
            request.TenantId,
            creditNoteNumber,
            request.InvoiceId,
            request.CustomerId,
            request.Reason,
            request.Currency);

        var lines = request.Lines.Select(l => new CreditNoteLine(
            Guid.NewGuid(),
            creditNote.Id,
            l.InvoiceLineId,
            l.Description,
            l.Amount,
            l.Quantity)).ToList();

        creditNote.AddLines(lines);
        creditNote.Issue();

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _invoiceRepository.AddCreditNoteAsync(creditNote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Credit note {CreditNoteNumber} issued for invoice {InvoiceId}",
            creditNoteNumber, request.InvoiceId);

        return Result.Success(creditNote.Adapt<CreditNoteDto>());
    }
}
