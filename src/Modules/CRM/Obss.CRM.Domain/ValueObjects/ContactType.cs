namespace Obss.CRM.Domain.ValueObjects;

[Flags]
public enum ContactType
{
    None = 0,
    Primary = 1,
    Billing = 2,
    Technical = 4
}
