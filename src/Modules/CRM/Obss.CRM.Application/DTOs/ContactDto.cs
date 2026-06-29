namespace Obss.CRM.Application.DTOs;

public sealed record ContactDto(
    Guid Id,
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? MobileNumber,
    string? Position,
    bool IsPrimary,
    bool IsBilling,
    bool IsTechnical,
    DateTime CreatedAt);
