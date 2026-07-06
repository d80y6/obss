using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetUsers;

public sealed record GetUsersQuery(
    string? TenantId,
    bool? IsActive,
    string? SearchTerm,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<UserDto>>>;
