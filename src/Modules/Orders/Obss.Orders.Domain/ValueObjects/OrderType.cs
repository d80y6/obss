namespace Obss.Orders.Domain.ValueObjects;

public enum OrderType
{
    New = 1,
    Renewal = 2,
    Change = 3,
    Termination = 4,
    Transfer = 5
}
