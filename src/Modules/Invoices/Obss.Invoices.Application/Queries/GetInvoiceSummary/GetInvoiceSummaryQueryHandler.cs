using Mapster;
using MediatR;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoiceSummary;

public sealed class GetInvoiceSummaryQueryHandler : IRequestHandler<GetInvoiceSummaryQuery, Result<InvoiceSummaryDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceSummaryQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<InvoiceSummaryDto>> Handle(GetInvoiceSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await _invoiceRepository.GetInvoiceSummaryAsync(cancellationToken);
        return Result.Success(summary.Adapt<InvoiceSummaryDto>());
    }
}
