namespace Obss.Billing.Domain.ValueObjects;

public sealed record AccountHolder
{
    public AccountHolder(string name, string? email, string? phone, Guid? contactId)
    {
        Name = name;
        Email = email;
        Phone = phone;
        ContactId = contactId;
    }

    public string Name { get; }
    public string? Email { get; }
    public string? Phone { get; }
    public Guid? ContactId { get; }
}
