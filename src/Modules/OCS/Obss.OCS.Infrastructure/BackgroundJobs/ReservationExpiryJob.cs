using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Domain.Entities;
using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.OCS.Infrastructure.BackgroundJobs;

public sealed class ReservationExpiryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpiryJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    public ReservationExpiryJob(IServiceScopeFactory scopeFactory, ILogger<ReservationExpiryJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reservationRepo = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
                var balanceRepo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
                var transactionRepo = scope.ServiceProvider.GetRequiredService<IOcsTransactionRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expired = await reservationRepo.GetExpiredReservationsAsync(stoppingToken);
                foreach (var reservation in expired)
                {
                    try
                    {
                        var balance = await balanceRepo.GetByIdAsync(reservation.BalanceId, stoppingToken);
                        if (balance is null)
                        {
                            reservation.Expire();
                            await reservationRepo.UpdateAsync(reservation, stoppingToken);
                            continue;
                        }

                        balance.ReleaseReservation(reservation.Amount);
                        reservation.Expire();

                        var transaction = OcsTransaction.Create(
                            reservation.TenantId, reservation.SubscriptionId, reservation.BalanceId, null,
                            TransactionType.Expiry, reservation.Amount, reservation.Currency,
                            $"Auto-release expired reservation {reservation.Id}");
                        await transactionRepo.AddAsync(transaction, stoppingToken);
                        await reservationRepo.UpdateAsync(reservation, stoppingToken);
                        await balanceRepo.UpdateAsync(balance, stoppingToken);
                        await unitOfWork.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Released expired reservation {ReservationId} for subscription {SubscriptionId}",
                            reservation.Id, reservation.SubscriptionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to release expired reservation {ReservationId}", reservation.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reservation expiry check failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
