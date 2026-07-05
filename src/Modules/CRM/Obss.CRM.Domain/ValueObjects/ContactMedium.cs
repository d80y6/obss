namespace Obss.CRM.Domain.ValueObjects;

public class ContactMedium
{
    public ContactMedium(ContactMediumType mediumType, bool isPreferred, DateTime? validFrom, DateTime? validUntil)
    {
        MediumType = mediumType;
        IsPreferred = isPreferred;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
    }

    public ContactMediumType MediumType { get; private set; }
    public bool IsPreferred { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    private readonly List<ContactCharValue> _characteristics = [];
    public IReadOnlyCollection<ContactCharValue> Characteristics => _characteristics.AsReadOnly();

    public void AddCharacteristic(string key, string value, string valueType)
    {
        _characteristics.Add(new ContactCharValue(key, value, valueType));
    }

    public void SetPreferred(bool isPreferred) => IsPreferred = isPreferred;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);
    public override int GetHashCode() => HashCode.Combine(MediumType);
}
