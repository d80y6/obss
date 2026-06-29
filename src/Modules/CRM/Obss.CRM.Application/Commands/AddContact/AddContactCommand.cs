using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddContact;

public sealed record AddContactCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? CountryCode,
    string? MobileNumber,
    string? MobileCountryCode,
    string? Position,
    bool IsPrimary,
    bool IsBilling,
    bool IsTechnical) : IRequest<Result<ContactDto>>;
