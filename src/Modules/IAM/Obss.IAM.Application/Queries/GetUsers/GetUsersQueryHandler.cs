using Mapster;
using MediatR;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetFilteredAsync(
            request.TenantId,
            request.IsActive,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = users.Adapt<List<UserDto>>();
        return Result.Success<IReadOnlyList<UserDto>>(result);
    }
}
