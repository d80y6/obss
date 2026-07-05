using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetDunningPolicyById;

public sealed record GetDunningPolicyByIdQuery(Guid Id) : IRequest<Result<DunningPolicyDto>>;
