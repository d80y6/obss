namespace Obss.Invoices.Domain.ValueObjects;

public enum LineType
{
    Recurring,
    Usage,
    OneTime,
    Discount,
    Tax,
    Adjustment
}
