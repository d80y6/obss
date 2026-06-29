using Mapster;
using MediatR;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoicesByCustomer;

public sealed class GetInvoicesByCustomerQueryHandler : IRequestHandler<GetInvoicesByCustomerQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesByCustomerQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(GetInvoicesByCustomerQuery request, CancellationToken cancellationToken)
    {
        InvoiceStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<InvoiceStatus>(request.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var invoices = await _invoiceRepository.GetByCustomerAsync(
            request.CustomerId,
            status,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        return Result.Success(invoices.Adapt<List<InvoiceDto>>() as IReadOnlyList<InvoiceDto>);
    }
}
