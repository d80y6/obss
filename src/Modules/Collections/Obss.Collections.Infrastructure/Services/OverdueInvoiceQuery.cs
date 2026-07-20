using MediatR;
using Obss.Collections.Application.Abstractions;
using Obss.Invoices.Application.Queries.GetOverdueInvoices;

namespace Obss.Collections.Infrastructure.Services;

public sealed class OverdueInvoiceQuery : IOverdueInvoiceQuery
{
    private readonly IMediator _mediator;

    public OverdueInvoiceQuery(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetOverdueInvoicesQuery(), cancellationToken);

        if (result.IsFailure || result.Value is null)
            return Array.Empty<OverdueInvoiceDto>();

        return result.Value.Select(invoice => new OverdueInvoiceDto(
            invoice.Id,
            invoice.CustomerId,
            invoice.CustomerName,
            invoice.AmountDue,
            invoice.Currency,
            invoice.DueDate,
            Math.Max(0, (int)(DateTime.UtcNow - invoice.DueDate).TotalDays)
        )).ToList();
    }
}
