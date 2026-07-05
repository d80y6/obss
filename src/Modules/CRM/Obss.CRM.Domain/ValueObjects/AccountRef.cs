namespace Obss.CRM.Domain.ValueObjects;

public sealed record AccountRef(Guid BillingAccountId, string Name, string AccountType, string Role, string? Href);
