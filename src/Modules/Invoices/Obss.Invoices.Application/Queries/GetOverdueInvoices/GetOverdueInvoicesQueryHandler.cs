using Mapster;
using MediatR;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetOverdueInvoices;

public sealed class GetOverdueInvoicesQueryHandler : IRequestHandler<GetOverdueInvoicesQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetOverdueInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(GetOverdueInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetOverdueInvoicesAsync(cancellationToken);
        return Result.Success(invoices.Adapt<List<InvoiceDto>>() as IReadOnlyList<InvoiceDto>);
    }
}
