using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? CountryCode,
    string? Title,
    string? MiddleName,
    DateTime? BirthDate,
    string? NationalId,
    string? PreferredLanguage,
    string? Gender) : IRequest<Result<UserDto>>;
