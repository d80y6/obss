using MediatR;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Domain.Entities;
using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.DebitReservation;

internal sealed class DebitReservationCommandHandler : IRequestHandler<DebitReservationCommand, Result>
{
    private readonly IReservationRepository _reservationRepo;
    private readonly IBalanceRepository _balanceRepo;
    private readonly IOcsTransactionRepository _transactionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public DebitReservationCommandHandler(
        IReservationRepository reservationRepo,
        IBalanceRepository balanceRepo,
        IOcsTransactionRepository transactionRepo,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _reservationRepo = reservationRepo;
        _balanceRepo = balanceRepo;
        _transactionRepo = transactionRepo;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DebitReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepo.GetByIdAsync(request.ReservationId, cancellationToken);
        if (reservation is null)
            return Result.Failure(Error.NotFound("Reservation", request.ReservationId));
        if (reservation.Status != ReservationStatus.Reserved)
            return Result.Failure(Error.Validation($"Reservation {request.ReservationId} is not in Reserved state"));

        var balance = await _balanceRepo.GetByIdAsync(reservation.BalanceId, cancellationToken);
        if (balance is null)
            return Result.Failure(Error.NotFound("Balance", reservation.BalanceId));

        balance.Debit(reservation.Amount);
        balance.ReleaseReservation(reservation.Amount);
        reservation.Debit();

        var transaction = OcsTransaction.Create(
            reservation.TenantId, reservation.SubscriptionId, reservation.BalanceId, null,
            TransactionType.Usage, reservation.Amount, reservation.Currency,
            $"Debit from reservation {reservation.Id}");
        await _transactionRepo.AddAsync(transaction, cancellationToken);
        await _reservationRepo.UpdateAsync(reservation, cancellationToken);
        await _balanceRepo.UpdateAsync(balance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Reservation", reservation.Id.ToString(), "Debit",
            performedById: _currentUser.UserId, cancellationToken: cancellationToken);

        return Result.Success(new DebitReservationResponse(reservation.Id, reservation.Amount, reservation.Currency, balance.EffectiveBalance));
    }
}
