using Obss.Collections.Domain.Events;
using Obss.Collections.Domain.Exceptions;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Collections.Domain.Entities;

public class PaymentArrangement : AggregateRoot<Guid>
{
    private readonly List<Installment> _installments = [];

    private PaymentArrangement() { }

    private PaymentArrangement(
        Guid id,
        Guid collectionCaseId,
        Guid customerId,
        decimal totalAmount,
        int installmentCount,
        decimal installmentAmount,
        PaymentFrequency frequency,
        DateTime firstPaymentDate)
        : base(id)
    {
        CollectionCaseId = collectionCaseId;
        CustomerId = customerId;
        Status = ArrangementStatus.Proposed;
        TotalAmount = totalAmount;
        PaidAmount = 0;
        InstallmentCount = installmentCount;
        InstallmentAmount = installmentAmount;
        Frequency = frequency;
        FirstPaymentDate = firstPaymentDate;
        LastPaymentDate = null;
        CreatedAt = DateTime.UtcNow;
        DefaultedAt = null;
    }

    public Guid CollectionCaseId { get; private set; }
    public Guid CustomerId { get; private set; }
    public ArrangementStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public int InstallmentCount { get; private set; }
    public decimal InstallmentAmount { get; private set; }
    public PaymentFrequency Frequency { get; private set; }
    public DateTime FirstPaymentDate { get; private set; }
    public DateTime? LastPaymentDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DefaultedAt { get; private set; }

    public IReadOnlyCollection<Installment> Installments => _installments.AsReadOnly();

    public static PaymentArrangement Create(
        Guid collectionCaseId,
        Guid customerId,
        decimal totalAmount,
        int installmentCount,
        decimal installmentAmount,
        PaymentFrequency frequency,
        DateTime firstPaymentDate)
    {
        var arrangement = new PaymentArrangement(
            Guid.NewGuid(),
            collectionCaseId,
            customerId,
            totalAmount,
            installmentCount,
            installmentAmount,
            frequency,
            firstPaymentDate);

        arrangement.GenerateInstallments();
        return arrangement;
    }

    private void GenerateInstallments()
    {
        var dueDate = FirstPaymentDate;
        for (int i = 1; i <= InstallmentCount; i++)
        {
            var installment = Installment.Create(
                Id,
                i,
                dueDate,
                InstallmentAmount);
            _installments.Add(installment);

            dueDate = Frequency switch
            {
                PaymentFrequency.Weekly => dueDate.AddDays(7),
                PaymentFrequency.Monthly => dueDate.AddMonths(1),
                _ => dueDate.AddMonths(1)
            };
        }
    }

    public void Activate()
    {
        if (Status != ArrangementStatus.Proposed)
            throw new InvalidCollectionStateException("Only proposed arrangements can be activated.");

        Status = ArrangementStatus.Active;
    }

    public void RecordPayment(decimal amount)
    {
        if (Status != ArrangementStatus.Active)
            throw new InvalidCollectionStateException("Cannot record payment on a non-active arrangement.");

        if (amount <= 0)
            throw new InvalidCollectionStateException("Payment amount must be positive.");

        var remaining = TotalAmount - PaidAmount;
        if (amount > remaining)
            throw new InvalidCollectionStateException($"Payment amount {amount} exceeds remaining balance {remaining}.");

        var unpaidInstallment = _installments
            .FirstOrDefault(i => i.Status == InstallmentStatus.Pending || i.Status == InstallmentStatus.Overdue);

        if (unpaidInstallment is not null)
        {
            unpaidInstallment.RecordPayment(amount);
        }

        PaidAmount += amount;
        LastPaymentDate = DateTime.UtcNow;

        if (PaidAmount >= TotalAmount)
        {
            Complete();
        }
    }

    public void Default()
    {
        if (Status != ArrangementStatus.Active)
            throw new InvalidCollectionStateException("Only active arrangements can be defaulted.");

        Status = ArrangementStatus.Defaulted;
        DefaultedAt = DateTime.UtcNow;

        foreach (var installment in _installments)
        {
            if (installment.Status == InstallmentStatus.Pending || installment.Status == InstallmentStatus.Overdue)
                installment.MarkDefaulted();
        }

        AddDomainEvent(new PaymentArrangementDefaultedDomainEvent(
            Id, CollectionCaseId, CustomerId, TotalAmount, PaidAmount));
    }

    public void Complete()
    {
        if (Status != ArrangementStatus.Active)
            throw new InvalidCollectionStateException("Only active arrangements can be completed.");

        Status = ArrangementStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == ArrangementStatus.Completed || Status == ArrangementStatus.Cancelled)
            throw new InvalidCollectionStateException("Arrangement is already completed or cancelled.");

        Status = ArrangementStatus.Cancelled;
    }

    public void MarkOverdueInstallments()
    {
        var now = DateTime.UtcNow;
        foreach (var installment in _installments)
        {
            if (installment.Status == InstallmentStatus.Pending && installment.DueDate < now)
                installment.MarkOverdue();
        }
    }
}
