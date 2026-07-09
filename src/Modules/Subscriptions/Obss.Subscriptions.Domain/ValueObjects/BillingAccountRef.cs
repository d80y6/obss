namespace Obss.Subscriptions.Domain.ValueObjects;

public sealed record BillingAccountRef(
    string AccountId,
    string? Href);
