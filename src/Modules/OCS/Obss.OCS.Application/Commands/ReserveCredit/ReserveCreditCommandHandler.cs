using MediatR;
using Microsoft.EntityFrameworkCore;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Domain.Entities;
using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.ReserveCredit;

internal sealed class ReserveCreditCommandHandler : IRequestHandler<ReserveCreditCommand, Result<ReserveCreditResult>>
{
    private readonly IBalanceRepository _balanceRepo;
    private readonly ICreditPoolRepository _poolRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly IOcsTransactionRepository _transactionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private static readonly int MaxRetryCount = 25;

    public ReserveCreditCommandHandler(
        IBalanceRepository balanceRepo,
        ICreditPoolRepository poolRepo,
        IReservationRepository reservationRepo,
        IOcsTransactionRepository transactionRepo,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _balanceRepo = balanceRepo;
        _poolRepo = poolRepo;
        _reservationRepo = reservationRepo;
        _transactionRepo = transactionRepo;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<ReserveCreditResult>> Handle(ReserveCreditCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var rng = new Random();

        var pools = await _poolRepo.GetActiveBySubscriptionAsync(request.SubscriptionId, cancellationToken);

        var reservation = Reservation.Create(
            string.Empty, Guid.Empty, request.SubscriptionId, request.Amount, request.Currency);
        var transaction = OcsTransaction.Create(
            string.Empty, request.SubscriptionId, Guid.Empty, null,
            TransactionType.Reservation, request.Amount, request.Currency,
            $"Reservation {reservation.Id}");

        for (var attempt = 0; attempt <= MaxRetryCount; attempt++)
        {
            if (attempt > 0)
            {
                var baseDelay = Math.Min(10 * (1 << Math.Min(attempt - 1, 8)), 2000);
                var jitter = rng.Next(0, baseDelay);
                await Task.Delay(baseDelay + jitter, cancellationToken);
            }

            var balance = await _balanceRepo.GetBySubscriptionIdAsync(request.SubscriptionId, cancellationToken);
            if (balance is null)
                return Result.Failure<ReserveCreditResult>(Error.NotFound("Balance", request.SubscriptionId));

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

            await _reservationRepo.AddAsync(reservation, cancellationToken);
            await _transactionRepo.AddAsync(transaction, cancellationToken);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _auditService.LogAsync("Reservation", reservation.Id.ToString(), "ReserveCredit",
                    performedById: _currentUser.UserId, cancellationToken: cancellationToken);

                return Result.Success(new ReserveCreditResult(
                    true, reservation.Id, request.Amount, balance.EffectiveBalance, null, poolIds.AsReadOnly()));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                errors.Add($"Concurrency conflict on attempt {attempt + 1}: {ex.Message}");
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync(cancellationToken);
                    if (entry.State is EntityState.Added)
                        entry.State = EntityState.Detached;
                }
            }
        }

        return Result.Failure<ReserveCreditResult>(Error.Conflict(
            $"Reservation failed after {MaxRetryCount + 1} attempts due to concurrency conflicts: {string.Join("; ", errors)}"));
    }
}
