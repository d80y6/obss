namespace Obss.CRM.Domain.ValueObjects;

public class NotificationHub
{
    public NotificationHub(HubType hubType, string identifier, bool isOptIn, DateTime? validFrom, DateTime? validUntil)
    {
        HubType = hubType;
        Identifier = identifier;
        IsOptIn = isOptIn;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
    }

    public HubType HubType { get; private set; }
    public string Identifier { get; private set; }
    public bool IsOptIn { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }

    internal void SetOptIn(bool isOptIn) => IsOptIn = isOptIn;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);
    public override int GetHashCode() => HashCode.Combine(HubType, Identifier);
}
