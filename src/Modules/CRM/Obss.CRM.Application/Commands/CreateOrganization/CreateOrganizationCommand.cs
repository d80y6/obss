using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(
    string TradingName,
    string CompanyType,
    string? Industry,
    string? RegistrationNumber,
    string? TaxNumber,
    string? CountryOfRegistration) : IRequest<Result<OrganizationDto>>;
