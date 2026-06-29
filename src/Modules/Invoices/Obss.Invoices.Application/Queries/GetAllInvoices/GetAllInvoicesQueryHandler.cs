using Mapster;
using MediatR;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetAllInvoices;

internal sealed class GetAllInvoicesQueryHandler
    : IRequestHandler<GetAllInvoicesQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(
        GetAllInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        return Result.Success(invoices.Adapt<List<InvoiceDto>>() as IReadOnlyList<InvoiceDto>);
    }
}
