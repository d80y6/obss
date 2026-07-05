namespace Obss.CRM.Application.DTOs;

public sealed record IndividualDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Salutation,
    string? Title,
    DateTime? BirthDate,
    string? Nationality,
    string? Gender,
    string KycStatus,
    DateTime? KycVerifiedAt,
    string? KycVerifiedBy,
    string RiskRating,
    List<IdentityDocumentDto> Documents);
