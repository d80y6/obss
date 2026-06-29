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
    string? ExternalId) : IRequest<Result<UserDto>>;
