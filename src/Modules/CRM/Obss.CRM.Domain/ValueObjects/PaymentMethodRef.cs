namespace Obss.CRM.Domain.ValueObjects;

public sealed record PaymentMethodRef(Guid PaymentMethodId, string Name, string? Href);
