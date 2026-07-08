namespace Obss.Provisioning.Domain.Entities;

public record CancelServiceOrder
{
    public CancelServiceOrder(Guid id, string reason)
    {
        Id = id;
        Reason = reason;
        State = "Pending";
    }

    public Guid Id { get; init; }
    public string Reason { get; init; }
    public DateTime? CompletedDate { get; init; }
    public string State { get; init; }
}
