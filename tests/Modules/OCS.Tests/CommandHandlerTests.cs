using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.Commands.CreateBalance;
using Obss.OCS.Domain.ValueObjects;
using Obss.OCS.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;
using Xunit;

namespace Obss.OCS.Tests;

public sealed class CommandHandlerTests : OcsIntegrationTests
{
    [Fact]
    public async Task CreateBalanceCommand_ShouldCreateBalanceInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new BalanceRepository(context);
        var transactionRepo = new OcsTransactionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");

        var handler = new CreateBalanceCommandHandler(
            repository, transactionRepo, unitOfWork, currentTenant);

        var command = new CreateBalanceCommand(Guid.NewGuid(), "YER");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var saved = await repository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Currency.Should().Be("YER");
    }

    [Fact]
    public async Task ReserveCredit_WhenSufficientBalance_ShouldCreateReservation()
    {
        using var context = CreateDbContext();

        var balanceRepo = new BalanceRepository(context);
        var poolRepo = new CreditPoolRepository(context);
        var reservationRepo = new ReservationRepository(context);
        var transactionRepo = new OcsTransactionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");
        var auditService = Substitute.For<IAuditService>();
        var currentUser = Substitute.For<ICurrentUser>();

        // First create a balance
        var createHandler = new CreateBalanceCommandHandler(
            balanceRepo, transactionRepo, unitOfWork, currentTenant);
        var subscriptionId = Guid.NewGuid();
        var balanceResult = await createHandler.Handle(
            new CreateBalanceCommand(subscriptionId, "YER"), CancellationToken.None);
        balanceResult.IsSuccess.Should().BeTrue();

        // Add initial credit by adjusting balance
        var balance = await balanceRepo.GetByIdAsync(balanceResult.Value.Id);
        balance!.Credit(10000m);
        await balanceRepo.UpdateAsync(balance, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Now reserve credit
        var reserveHandler = new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommandHandler(
            balanceRepo, poolRepo, reservationRepo, transactionRepo, unitOfWork, auditService, currentUser);

        var reserveCommand = new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommand(
            subscriptionId, 500m, "YER");

        var reserveResult = await reserveHandler.Handle(reserveCommand, CancellationToken.None);

        reserveResult.IsSuccess.Should().BeTrue();
        reserveResult.Value.ReservationId.Should().NotBeEmpty();
        reserveResult.Value.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ReserveCredit_WhenInsufficientBalance_ShouldFail()
    {
        using var context = CreateDbContext();

        var balanceRepo = new BalanceRepository(context);
        var poolRepo = new CreditPoolRepository(context);
        var reservationRepo = new ReservationRepository(context);
        var transactionRepo = new OcsTransactionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");
        var auditService = Substitute.For<IAuditService>();
        var currentUser = Substitute.For<ICurrentUser>();

        var createHandler = new CreateBalanceCommandHandler(
            balanceRepo, transactionRepo, unitOfWork, currentTenant);
        var subscriptionId = Guid.NewGuid();
        await createHandler.Handle(
            new CreateBalanceCommand(subscriptionId, "YER"), CancellationToken.None);

        // Try to reserve more than available
        var reserveHandler = new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommandHandler(
            balanceRepo, poolRepo, reservationRepo, transactionRepo, unitOfWork, auditService, currentUser);

        var reserveCommand = new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommand(
            subscriptionId, 1000m, "YER");

        var reserveResult = await reserveHandler.Handle(reserveCommand, CancellationToken.None);

        reserveResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ReserveCreditCommandHandler_Sequential_2_Reserves_ShouldWork()
    {
        using var context = CreateDbContext();

        var balanceRepo = new BalanceRepository(context);
        var poolRepo = new CreditPoolRepository(context);
        var reservationRepo = new ReservationRepository(context);
        var transactionRepo = new OcsTransactionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");
        var auditService = Substitute.For<IAuditService>();
        var currentUser = Substitute.For<ICurrentUser>();

        var createHandler = new CreateBalanceCommandHandler(
            balanceRepo, transactionRepo, unitOfWork, currentTenant);
        var subscriptionId = Guid.NewGuid();
        var createResult = await createHandler.Handle(
            new CreateBalanceCommand(subscriptionId, "YER"), CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        var balance = await balanceRepo.GetByIdAsync(createResult.Value.Id);
        balance!.Credit(1000m);
        await balanceRepo.UpdateAsync(balance, CancellationToken.None);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var reserveHandler = new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommandHandler(
            balanceRepo, poolRepo, reservationRepo, transactionRepo, unitOfWork, auditService, currentUser);

        // First reserve
        var r1 = await reserveHandler.Handle(
            new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommand(subscriptionId, 10m, "YER"),
            CancellationToken.None);
        r1.IsSuccess.Should().BeTrue();

        // Second reserve
        var r2 = await reserveHandler.Handle(
            new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommand(subscriptionId, 10m, "YER"),
            CancellationToken.None);
        r2.IsSuccess.Should().BeTrue();

        // Verify
        var saved = await balanceRepo.GetQueryable().FirstOrDefaultAsync(b => b.SubscriptionId == subscriptionId);
        saved.Should().NotBeNull();
        saved!.ReservedAmount.Should().Be(20m);
    }

    [Fact]
    public async Task ReserveCreditCommandHandler_Concurrent_5_Parallel_Requests_NoLostUpdate()
    {
        var subscriptionId = Guid.NewGuid();
        var parallelCount = 5;
        var amountPerRequest = 10m;
        var totalAvailable = 1000m;

        // Setup: create balance with totalAvailable
        using (var setupContext = CreateDbContext())
        {
            var setupBalanceRepo = new BalanceRepository(setupContext);
            var setupTransactionRepo = new OcsTransactionRepository(setupContext);
            var setupUnitOfWork = CreateUnitOfWork(setupContext);
            var setupTenant = Substitute.For<ICurrentTenant>();
            setupTenant.TenantId.Returns("test-tenant");

            var createHandler = new CreateBalanceCommandHandler(
                setupBalanceRepo, setupTransactionRepo, setupUnitOfWork, setupTenant);
            var createResult = await createHandler.Handle(
                new CreateBalanceCommand(subscriptionId, "YER"), CancellationToken.None);
            createResult.IsSuccess.Should().BeTrue();

            var balance = await setupBalanceRepo.GetByIdAsync(createResult.Value.Id);
            balance!.Credit(totalAvailable);
            await setupBalanceRepo.UpdateAsync(balance, CancellationToken.None);
            await setupUnitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        // Execute parallel reserve calls
        var tasks = new List<Task<Obss.SharedKernel.Application.Contracts.Result<Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditResult>>>();
        for (var i = 0; i < parallelCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var context = CreateDbContext();
                var balanceRepo = new BalanceRepository(context);
                var poolRepo = new CreditPoolRepository(context);
                var reservationRepo = new ReservationRepository(context);
                var transactionRepo = new OcsTransactionRepository(context);
                var unitOfWork = CreateUnitOfWork(context);
                var auditService = Substitute.For<IAuditService>();
                var currentUser = Substitute.For<ICurrentUser>();

                var handler = new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommandHandler(
                    balanceRepo, poolRepo, reservationRepo, transactionRepo, unitOfWork, auditService, currentUser);

                return await handler.Handle(
                    new Obss.OCS.Application.Commands.ReserveCredit.ReserveCreditCommand(subscriptionId, amountPerRequest, "YER"),
                    CancellationToken.None);
            }));
        }

        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r.IsSuccess);

        successCount.Should().Be(parallelCount);

        // Verify final state
        using (var verifyContext = CreateDbContext())
        {
            var verifyBalanceRepo = new BalanceRepository(verifyContext);
            var verifyTransactionRepo = new OcsTransactionRepository(verifyContext);

            var balances = await verifyBalanceRepo.GetQueryable().ToListAsync();
            var balance = balances.FirstOrDefault(b => b.SubscriptionId == subscriptionId);
            balance.Should().NotBeNull();
            balance!.AvailableAmount.Should().Be(totalAvailable);
            balance.ReservedAmount.Should().Be(parallelCount * amountPerRequest);

            var transactions = await verifyTransactionRepo.GetQueryable()
                .Where(t => t.SubscriptionId == subscriptionId)
                .ToListAsync();
            transactions.Count(t => t.TransactionType == TransactionType.Reservation).Should().Be(parallelCount);
        }
    }
}
