using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetUsers;

public sealed record GetUsersQuery(
    string? TenantId,
    bool? IsActive,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<UserDto>>>;
