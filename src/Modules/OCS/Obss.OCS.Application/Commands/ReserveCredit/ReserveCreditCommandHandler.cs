using MediatR;
using Obss.OCS.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.ReserveCredit;

internal sealed class ReserveCreditCommandHandler : IRequestHandler<ReserveCreditCommand, Result<ReserveCreditResult>>
{
    private readonly IBalanceRepository _balanceRepo;
    private readonly ICreditPoolRepository _poolRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ReserveCreditCommandHandler(IBalanceRepository balanceRepo, ICreditPoolRepository poolRepo, IUnitOfWork unitOfWork)
    {
        _balanceRepo = balanceRepo;
        _poolRepo = poolRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReserveCreditResult>> Handle(ReserveCreditCommand request, CancellationToken cancellationToken)
    {
        var balance = await _balanceRepo.GetBySubscriptionIdAsync(request.SubscriptionId, cancellationToken);
        if (balance is null)
            return Result.Failure<ReserveCreditResult>(Error.NotFound("Balance", request.SubscriptionId));

        var pools = await _poolRepo.GetActiveBySubscriptionAsync(request.SubscriptionId, cancellationToken);
        var poolIds = new List<Guid>();
        var toReserve = request.Amount;

        foreach (var pool in pools)
        {
            if (toReserve <= 0) break;
            var consume = Math.Min(pool.RemainingAmount, toReserve);
            pool.Consume(consume);
            await _poolRepo.UpdateAsync(pool, cancellationToken);
            poolIds.Add(pool.Id);
            toReserve -= consume;
        }

        try
        {
            balance.Reserve(request.Amount);
        }
        catch
        {
            return Result.Failure<ReserveCreditResult>(Error.Validation("Insufficient balance for reservation"));
        }

        await _balanceRepo.UpdateAsync(balance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReserveCreditResult(
            true, Guid.NewGuid(), request.Amount, balance.EffectiveBalance, null, poolIds.AsReadOnly()));
    }
}
