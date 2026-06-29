namespace Obss.Rating.Domain.DomainServices;

public sealed record BillLine(
    decimal Amount,
    int Quantity,
    Guid? ProductId,
    Guid? SubscriptionId);
