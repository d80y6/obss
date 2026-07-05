using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(
    Guid Id,
    string TradingName,
    string CompanyType,
    string? Industry,
    string? RegistrationNumber,
    string? TaxNumber,
    string? CountryOfRegistration) : IRequest<Result<OrganizationDto>>;
