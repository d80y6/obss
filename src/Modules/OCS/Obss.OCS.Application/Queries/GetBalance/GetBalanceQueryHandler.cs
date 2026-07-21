using Mapster;
using MediatR;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Queries.GetBalance;

internal sealed class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, Result<BalanceDto>>
{
    private readonly IBalanceRepository _repository;

    public GetBalanceQueryHandler(IBalanceRepository repository) => _repository = repository;

    public async Task<Result<BalanceDto>> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _repository.GetBySubscriptionIdAsync(request.SubscriptionId, cancellationToken);
        if (balance is null)
            return Result.Failure<BalanceDto>(Error.NotFound("Balance", request.SubscriptionId));
        return Result.Success(balance.Adapt<BalanceDto>());
    }
}
