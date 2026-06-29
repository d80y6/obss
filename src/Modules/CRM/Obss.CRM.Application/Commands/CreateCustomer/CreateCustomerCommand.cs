using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string TenantId,
    string CustomerType,
    string? CompanyName,
    string DisplayName,
    string? TaxNumber,
    string? RegistrationNumber,
    string Email,
    string? PhoneNumber,
    string? CountryCode,
    string? Website,
    string Currency) : IRequest<Result<CustomerDto>>;
