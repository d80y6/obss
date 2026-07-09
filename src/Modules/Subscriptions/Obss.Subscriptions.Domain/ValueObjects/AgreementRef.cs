namespace Obss.Subscriptions.Domain.ValueObjects;

public sealed record AgreementRef(
    string AgreementId,
    string? Name,
    string? Href);
