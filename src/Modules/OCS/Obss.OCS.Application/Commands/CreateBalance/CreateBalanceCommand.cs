using MediatR;
using Obss.OCS.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.CreateBalance;

public sealed record CreateBalanceCommand(
    Guid SubscriptionId,
    string Currency) : IRequest<Result<BalanceDto>>;
