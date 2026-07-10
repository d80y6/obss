using Mapster;
using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountBalance;

public sealed class GetBillingAccountBalanceQueryHandler(
    IAccountBalanceRepository balanceRepository)
    : IRequestHandler<GetBillingAccountBalanceQuery, Result<AccountBalanceDto>>
{
    public async Task<Result<AccountBalanceDto>> Handle(GetBillingAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await balanceRepository.GetByBillingAccountIdWithTransactionsAsync(request.BillingAccountId, cancellationToken);
        if (balance is null)
            return Result.Failure<AccountBalanceDto>(Error.NotFound("AccountBalance", request.BillingAccountId));

        var dto = balance.Adapt<AccountBalanceDto>();
        return Result.Success(dto);
    }
}
