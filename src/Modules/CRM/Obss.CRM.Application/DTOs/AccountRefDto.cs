namespace Obss.CRM.Application.DTOs;

public sealed record AccountRefDto(Guid BillingAccountId, string Name, string AccountType, string Role, string? Href);
