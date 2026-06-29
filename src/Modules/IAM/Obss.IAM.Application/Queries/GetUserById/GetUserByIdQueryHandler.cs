using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UserDto>(Error.NotFound(nameof(User), request.UserId));

        return Result.Success(user.Adapt<UserDto>());
    }
}
