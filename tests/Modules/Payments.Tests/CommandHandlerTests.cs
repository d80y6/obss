using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Payments.Application.Commands.AllocatePayment;
using Obss.Payments.Application.Commands.CompletePayment;
using Obss.Payments.Application.Commands.RecordPayment;
using Obss.Payments.Application.Commands.RefundPayment;
using Obss.Payments.Application.Commands.RegisterPaymentGateway;
using Obss.Payments.Application.Commands.ReconcilePayment;
using Obss.Payments.Application.Commands.ImportBankStatement;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.Payments.Infrastructure.Persistence.Repositories;
using Obss.Payments.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Payments.Tests;

public class IntegrationCommandHandlerTests : PaymentIntegrationTests
{
    [Fact]
    public async Task RecordPaymentCommand_ShouldCreatePaymentInDatabase()
    {
        using var context = CreateDbContext();
        var paymentRepository = new PaymentRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");

        var handler = new RecordPaymentCommandHandler(
            paymentRepository, unitOfWork, currentTenant,
            Substitute.For<ILogger<RecordPaymentCommandHandler>>());

        var command = new RecordPaymentCommand(
            Guid.NewGuid(), 250m, "USD", "Cash", "REF001", Guid.NewGuid(), "Test payment");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentNumber.Should().NotBeNullOrEmpty();
        result.Value.Amount.Should().Be(250m);

        var saved = await paymentRepository.GetByIdWithDetailsAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Amount.Should().Be(250m);
    }

    [Fact]
    public async Task CompletePaymentCommand_ExistingPayment_ShouldComplete()
    {
        using var context = CreateDbContext();
        var paymentRepository = new PaymentRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var payment = Payment.Create("test-tenant", "PAY-INT-001", Guid.NewGuid(), 100m, "USD", PaymentMethodType.BankTransfer);
        await paymentRepository.AddAsync(payment);
        await unitOfWork.SaveChangesAsync();

        var handler = new CompletePaymentCommandHandler(
            paymentRepository, unitOfWork,
            Substitute.For<ILogger<CompletePaymentCommandHandler>>());

        var result = await handler.Handle(new CompletePaymentCommand(payment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await paymentRepository.GetByIdAsync(payment.Id);
        saved!.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task CompletePaymentCommand_NonExistingPayment_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var paymentRepository = new PaymentRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CompletePaymentCommandHandler(
            paymentRepository, unitOfWork,
            Substitute.For<ILogger<CompletePaymentCommandHandler>>());

        var result = await handler.Handle(new CompletePaymentCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task RefundPaymentCommand_ShouldRefundPayment()
    {
        using var context = CreateDbContext();
        var paymentRepository = new PaymentRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var payment = await CreateSavedPaymentAsync(paymentRepository, unitOfWork, 500m);
        payment.Complete();
        await unitOfWork.SaveChangesAsync();

        var handler = new RefundPaymentCommandHandler(
            paymentRepository, unitOfWork,
            Substitute.For<ILogger<RefundPaymentCommandHandler>>());

        var result = await handler.Handle(
            new RefundPaymentCommand(payment.Id, 200m, "Partial refund"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await paymentRepository.GetByIdWithDetailsAsync(payment.Id);
        saved!.Refunds.Should().ContainSingle(r => r.Amount == 200m);
        saved.Status.Should().Be(PaymentStatus.PartiallyRefunded);
    }

    [Fact]
    public async Task RegisterPaymentGatewayCommand_ShouldCreateGateway()
    {
        using var context = CreateDbContext();
        var gatewayRepository = new PaymentGatewayRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");

        var handler = new RegisterPaymentGatewayCommandHandler(
            gatewayRepository, unitOfWork, currentTenant,
            Substitute.For<ILogger<RegisterPaymentGatewayCommandHandler>>());

        var result = await handler.Handle(
            new RegisterPaymentGatewayCommand("Stripe Main", "Stripe", "{\"key\":\"val\"}",
                ["USD", "EUR"], null, null, 2.5m, "Percentage"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Stripe Main");

        var saved = await gatewayRepository.GetByProviderAsync(PaymentProvider.Stripe, "test-tenant");
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Stripe Main");
    }

    [Fact]
    public async Task ImportAndReconcilePayment_ShouldWorkEndToEnd()
    {
        using var context = CreateDbContext();
        var reconciliationRepository = new PaymentReconciliationRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");

        var importHandler = new ImportBankStatementCommandHandler(
            reconciliationRepository, unitOfWork, currentTenant,
            Substitute.For<ILogger<ImportBankStatementCommandHandler>>());

        var importResult = await importHandler.Handle(
            new ImportBankStatementCommand("Bank", "stmt.csv", "USD",
            [
                new BankTransactionLine("EXT001", 500m, DateTime.UtcNow, "Payment")
            ]), CancellationToken.None);

        importResult.IsSuccess.Should().BeTrue();
        var reconciliationId = importResult.Value.Id;

        var reconciliation = await reconciliationRepository.GetWithItemsAsync(reconciliationId);
        reconciliation!.Items.Should().ContainSingle();

        var itemId = reconciliation.Items.First().Id;

        var reconcileHandler = new ReconcilePaymentCommandHandler(
            reconciliationRepository, unitOfWork,
            Substitute.For<ILogger<ReconcilePaymentCommandHandler>>());

        var reconcileResult = await reconcileHandler.Handle(
            new ReconcilePaymentCommand(reconciliationId, itemId, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        reconcileResult.IsSuccess.Should().BeTrue();

        var updated = await reconciliationRepository.GetWithItemsAsync(reconciliationId);
        updated!.Items.First().Status.Should().Be(ReconciliationItemStatus.Matched);
        updated.Status.Should().Be(ReconciliationStatus.Reconciled);
    }

    private static async Task<Payment> CreateSavedPaymentAsync(IPaymentRepository repo, IUnitOfWork uow, decimal amount)
    {
        var payment = Payment.Create("test-tenant", $"PAY-{Guid.NewGuid():N}"[..12], Guid.NewGuid(), amount, "USD", PaymentMethodType.Cash);
        await repo.AddAsync(payment);
        await uow.SaveChangesAsync();
        return payment;
    }
}
