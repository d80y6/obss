using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;
