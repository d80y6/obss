namespace Obss.CRM.Application.DTOs;

public sealed record OrganizationDto(
    Guid Id,
    string TradingName,
    string CompanyType,
    string? Industry,
    string? RegistrationNumber,
    string? TaxNumber,
    string? CountryOfRegistration,
    string KycStatus,
    DateTime? KycVerifiedAt,
    string? KycVerifiedBy);
