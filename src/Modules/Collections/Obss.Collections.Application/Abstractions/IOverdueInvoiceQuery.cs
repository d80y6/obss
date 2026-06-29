using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Abstractions;

public sealed record OverdueInvoiceDto(
    Guid InvoiceId,
    Guid CustomerId,
    string CustomerName,
    decimal AmountDue,
    string Currency,
    DateTime DueDate,
    int DaysOverdue);

public interface IOverdueInvoiceQuery
{
    Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
}
