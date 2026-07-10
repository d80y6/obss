namespace Obss.Billing.Application.DTOs;

public sealed record BillPresentationMediaDto(
    Guid Id,
    string MediaType,
    string? EmailAddress,
    string? PaperFormat,
    string Language,
    bool IsPreferred,
    DateTime? ValidFrom,
    DateTime? ValidUntil);
