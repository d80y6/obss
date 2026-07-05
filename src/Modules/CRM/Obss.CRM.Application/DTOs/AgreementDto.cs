namespace Obss.CRM.Application.DTOs;

public sealed record AgreementDto(
    Guid Id,
    Guid CustomerId,
    string Name,
    string AgreementType,
    string Status,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? Description,
    DateTime? SignedAt,
    string? SignedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);
