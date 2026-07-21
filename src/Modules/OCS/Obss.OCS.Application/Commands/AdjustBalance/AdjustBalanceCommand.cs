using MediatR;
using Obss.OCS.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.AdjustBalance;

public sealed record AdjustBalanceCommand(
    Guid BalanceId,
    decimal Amount,
    string Direction,
    string Description) : IRequest<Result<BalanceDto>>;
