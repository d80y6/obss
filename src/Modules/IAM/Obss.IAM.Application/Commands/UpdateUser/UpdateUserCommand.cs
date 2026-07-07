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
    string? Title = null,
    string? MiddleName = null,
    DateTime? BirthDate = null,
    string? NationalId = null,
    string? PreferredLanguage = null,
    string? Gender = null) : IRequest<Result<UserDto>>;
