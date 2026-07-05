namespace Obss.CRM.Application.DTOs;

public sealed record IdentityDocumentDto(
    Guid Id,
    Guid IndividualId,
    string DocumentType,
    string DocumentNumber,
    string? IssuingAuthority,
    string? IssuingCountry,
    DateTime? IssuedDate,
    DateTime? ExpiryDate,
    bool IsVerified);
