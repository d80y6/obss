using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string? CompanyName,
    string DisplayName,
    string? TaxNumber,
    string? RegistrationNumber,
    string Email,
    string? PhoneNumber,
    string? CountryCode,
    string? Website) : IRequest<Result<CustomerDto>>;
