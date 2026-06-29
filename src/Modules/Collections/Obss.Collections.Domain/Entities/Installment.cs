using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Collections.Domain.Entities;

public class Installment : Entity<Guid>
{
    private Installment() { }

    private Installment(
        Guid id,
        Guid paymentArrangementId,
        int installmentNumber,
        DateTime dueDate,
        decimal amount)
        : base(id)
    {
        PaymentArrangementId = paymentArrangementId;
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        Amount = amount;
        PaidAmount = 0;
        Status = InstallmentStatus.Pending;
    }

    public Guid PaymentArrangementId { get; private set; }
    public int InstallmentNumber { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public InstallmentStatus Status { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public static Installment Create(
        Guid paymentArrangementId,
        int installmentNumber,
        DateTime dueDate,
        decimal amount)
    {
        return new Installment(
            Guid.NewGuid(),
            paymentArrangementId,
            installmentNumber,
            dueDate,
            amount);
    }

    public void RecordPayment(decimal amount)
    {
        PaidAmount += amount;
        PaidAt = DateTime.UtcNow;
        Status = InstallmentStatus.Paid;
    }

    public void MarkOverdue()
    {
        if (Status == InstallmentStatus.Pending)
            Status = InstallmentStatus.Overdue;
    }

    public void MarkDefaulted()
    {
        Status = InstallmentStatus.Defaulted;
    }
}
