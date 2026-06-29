using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string? ConnectionString,
    string? Settings,
    string AdminUsername,
    string AdminEmail,
    string AdminFirstName,
    string AdminLastName) : IRequest<Result<TenantDto>>;
