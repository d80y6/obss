using Obss.SharedKernel.Domain.Common;

namespace Obss.Collections.Domain.Events;

public sealed class CollectionCaseOpenedDomainEvent : DomainEvent, MediatR.INotification
{
    public CollectionCaseOpenedDomainEvent(
        Guid caseId,
        Guid customerId,
        string customerName,
        decimal totalOverdueAmount,
        string currency)
    {
        CaseId = caseId;
        CustomerId = customerId;
        CustomerName = customerName;
        TotalOverdueAmount = totalOverdueAmount;
        Currency = currency;
    }

    public Guid CaseId { get; }
    public Guid CustomerId { get; }
    public string CustomerName { get; }
    public decimal TotalOverdueAmount { get; }
    public string Currency { get; }
}
