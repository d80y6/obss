using Obss.SharedKernel.Domain.Common;

namespace Obss.Collections.Domain.Events;

public sealed class CollectionCaseResolvedDomainEvent : DomainEvent, MediatR.INotification
{
    public CollectionCaseResolvedDomainEvent(
        Guid caseId,
        Guid customerId,
        decimal totalOverdueAmount,
        string currency)
    {
        CaseId = caseId;
        CustomerId = customerId;
        TotalOverdueAmount = totalOverdueAmount;
        Currency = currency;
    }

    public Guid CaseId { get; }
    public Guid CustomerId { get; }
    public decimal TotalOverdueAmount { get; }
    public string Currency { get; }
}
