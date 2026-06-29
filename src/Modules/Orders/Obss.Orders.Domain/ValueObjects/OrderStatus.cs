namespace Obss.Orders.Domain.ValueObjects;

public enum OrderStatus
{
    Draft = 1,
    Submitted = 2,
    PendingApproval = 3,
    Approved = 4,
    Rejected = 5,
    Fulfilling = 6,
    Completed = 7,
    Cancelled = 8
}
