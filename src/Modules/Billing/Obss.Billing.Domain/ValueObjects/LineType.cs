namespace Obss.Billing.Domain.ValueObjects;

public enum LineType
{
    Recurring = 1,
    Usage = 2,
    OneTime = 3,
    Discount = 4,
    Tax = 5,
    Adjustment = 6
}
