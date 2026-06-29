namespace Obss.CRM.Application.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string TenantId,
    string CustomerType,
    string Status,
    string? CompanyName,
    string DisplayName,
    string? TaxNumber,
    string? RegistrationNumber,
    string Email,
    string? PhoneNumber,
    string? Website,
    bool IsActive,
    decimal CreditLimit,
    string Currency,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ContactDto> Contacts,
    List<CustomerNoteDto> Notes);
