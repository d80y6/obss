using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.OpenDispute;

public sealed class OpenDisputeCommandHandler : IRequestHandler<OpenDisputeCommand, Result<InvoiceDisputeDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceDisputeRepository _disputeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OpenDisputeCommandHandler> _logger;

    public OpenDisputeCommandHandler(
        IInvoiceRepository invoiceRepository,
        IInvoiceDisputeRepository disputeRepository,
        IUnitOfWork unitOfWork,
        ILogger<OpenDisputeCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _disputeRepository = disputeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<InvoiceDisputeDto>> Handle(OpenDisputeCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure<InvoiceDisputeDto>(Error.NotFound(nameof(Invoice), request.InvoiceId));

        var dispute = InvoiceDispute.Submit(
            request.InvoiceId,
            invoice.CustomerId,
            request.Reason,
            request.Description,
            request.DisputedAmount);

        await _disputeRepository.AddAsync(dispute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Dispute {DisputeId} opened for invoice {InvoiceId} by customer {CustomerId}",
            dispute.Id, request.InvoiceId, invoice.CustomerId);

        return Result.Success(dispute.Adapt<InvoiceDisputeDto>());
    }
}
