using MediatR;
using Obss.OCS.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Queries.GetBalance;

public sealed record GetBalanceQuery(Guid SubscriptionId) : IRequest<Result<BalanceDto>>;
