namespace Obss.Billing.Application.DTOs;

public sealed record AccountHolderDto(
    string Name,
    string? Email,
    string? Phone,
    Guid? ContactId);
