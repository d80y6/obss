using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.CreateUser;

public sealed record CreateUserCommand(
    string TenantId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? CountryCode,
    string? ExternalId,
    string? Title,
    string? MiddleName,
    DateTime? BirthDate,
    string? NationalId,
    string? PreferredLanguage,
    string? Gender) : IRequest<Result<UserDto>>;
