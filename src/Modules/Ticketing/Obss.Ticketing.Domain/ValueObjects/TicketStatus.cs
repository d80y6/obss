namespace Obss.Ticketing.Domain.ValueObjects;

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    WaitingCustomer = 2,
    WaitingThirdParty = 3,
    Resolved = 4,
    Closed = 5
}
