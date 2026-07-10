using Obss.SharedKernel.Domain.Common;
using Obss.ServiceQualification.Domain.Events;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Entities;

public class ServiceQualification : AggregateRoot<Guid>
{
    private readonly List<QualificationItem> _items = [];

    public Guid CustomerId { get; private set; }
    public GeographicAddress Address { get; private set; } = null!;
    public ServiceQualificationState State { get; private set; }
    public DateTime RequestedDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<QualificationItem> Items => _items.AsReadOnly();

    private ServiceQualification() { }

    public ServiceQualification(
        Guid id,
        Guid customerId,
        GeographicAddress address,
        string? description) : base(id)
    {
        CustomerId = customerId;
        Address = address;
        Description = description;
        State = ServiceQualificationState.InProgress;
        RequestedDate = DateTime.UtcNow;
        ExpirationDate = RequestedDate.AddDays(30);

        AddDomainEvent(new ServiceQualificationSubmittedDomainEvent(Id, customerId));
    }

    public void AddItem(Guid itemId, Guid serviceId, string serviceName)
    {
        if (State != ServiceQualificationState.InProgress)
            throw new InvalidOperationException("Cannot add items to a completed qualification.");

        _items.Add(new QualificationItem(itemId, serviceId, serviceName));
    }

    public void Complete()
    {
        if (State != ServiceQualificationState.InProgress)
            throw new InvalidOperationException("Qualification is not in progress.");

        State = ServiceQualificationState.Done;
        var isFullyQualified = _items.All(i => i.ResultType == QualificationResultType.Qualified);
        AddDomainEvent(new ServiceQualificationCompletedDomainEvent(Id, CustomerId, isFullyQualified));
    }

    public void Terminate(string reason)
    {
        if (State != ServiceQualificationState.InProgress)
            throw new InvalidOperationException("Qualification is not in progress.");

        State = ServiceQualificationState.TerminatedWithError;
        AddDomainEvent(new ServiceQualificationTerminatedDomainEvent(Id, reason));
    }
}
