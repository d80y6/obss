using Microsoft.EntityFrameworkCore;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Payments.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository : EfRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(PaymentDbContext context)
        : base(context)
    {
    }

    public async Task<Payment?> GetByIdWithDetailsAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Allocations)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetFilteredAsync(
        Guid? customerId,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int offset = 0,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.Allocations)
            .Include(p => p.Refunds)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            query = query.Where(p => p.Status == paymentStatus);

        if (fromDate.HasValue)
            query = query.Where(p => p.PaidAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.PaidAt <= toDate.Value);

        query = query
            .OrderByDescending(p => p.PaidAt)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Allocations)
            .Include(p => p.Refunds)
            .Where(p => p.Allocations.Any(a => a.InvoiceId == invoiceId) || p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Refund>> GetRefundsFilteredAsync(
        Guid? paymentId,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int offset = 0,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Refund>().AsQueryable();

        if (paymentId.HasValue)
            query = query.Where(r => r.PaymentId == paymentId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RefundStatus>(status, true, out var refundStatus))
            query = query.Where(r => r.Status == refundStatus);

        if (fromDate.HasValue)
            query = query.Where(r => r.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.CreatedAt <= toDate.Value);

        query = query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PaymentSummary> GetPaymentSummaryAsync(CancellationToken cancellationToken = default)
    {
        var payments = await DbSet.ToListAsync(cancellationToken);

        return new PaymentSummary(
            payments.Count,
            payments.Count(p => p.Status == PaymentStatus.Pending),
            payments.Count(p => p.Status == PaymentStatus.Completed),
            payments.Count(p => p.Status == PaymentStatus.Failed),
            payments.Count(p => p.Status == PaymentStatus.Refunded),
            payments.Count(p => p.Status == PaymentStatus.PartiallyRefunded),
            payments.Sum(p => p.Amount),
            payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
            payments.Sum(p => p.Refunds.Sum(r => r.Amount)),
            payments.Sum(p => p.Amount) - payments.Sum(p => p.Refunds.Sum(r => r.Amount)));
    }

    public async Task<string> GenerateNextPaymentNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastPayment = await DbSet
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextNumber = 1;
        if (lastPayment?.PaymentNumber is not null &&
            int.TryParse(lastPayment.PaymentNumber.Replace("PAY-", ""), out var lastSeq))
        {
            nextNumber = lastSeq + 1;
        }

        return $"PAY-{nextNumber:D6}";
    }
}
