using Xunit;
using FluentAssertions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.Payments.Infrastructure.Persistence.Repositories;

namespace Obss.Payments.Tests;

public class RepositoryTests : PaymentIntegrationTests
{
    [Fact]
    public async Task PaymentRepository_AddAndRetrieve()
    {
        using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var payment = Payment.Create("test-tenant", "PAY-REPO-001", Guid.NewGuid(), 150m, "USD", PaymentMethodType.CreditCard, "TXN001");
        payment.Complete();

        await repository.AddAsync(payment);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(payment.Id);

        retrieved.Should().NotBeNull();
        retrieved!.PaymentNumber.Should().Be("PAY-REPO-001");
        retrieved.Amount.Should().Be(150m);
        retrieved.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task PaymentRepository_GetByIdWithDetails_ShouldIncludeAllocationsAndRefunds()
    {
        using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var payment = Payment.Create("test-tenant", "PAY-DTL-001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash, invoiceId: Guid.NewGuid());
        payment.Complete();
        payment.Refund(200m, "partial");

        await repository.AddAsync(payment);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdWithDetailsAsync(payment.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Allocations.Should().NotBeEmpty();
        retrieved.Refunds.Should().ContainSingle(r => r.Amount == 200m);
    }

    [Fact]
    public async Task PaymentRepository_GetFiltered_ShouldApplyFilters()
    {
        using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var customerId = Guid.NewGuid();
        var payment1 = Payment.Create("test-tenant", "PAY-FLT-001", customerId, 100m, "USD", PaymentMethodType.Cash);
        payment1.Complete();
        var payment2 = Payment.Create("test-tenant", "PAY-FLT-002", customerId, 200m, "USD", PaymentMethodType.Cash);
        var payment3 = Payment.Create("test-tenant", "PAY-FLT-003", Guid.NewGuid(), 300m, "USD", PaymentMethodType.Cash);

        await repository.AddAsync(payment1);
        await repository.AddAsync(payment2);
        await repository.AddAsync(payment3);
        await context.SaveChangesAsync();

        var customerPayments = await repository.GetFilteredAsync(customerId, null, null, null, 1, 20);
        customerPayments.Should().HaveCount(2);

        var completedPayments = await repository.GetFilteredAsync(customerId, "Completed", null, null, 1, 20);
        completedPayments.Should().ContainSingle();
    }

    [Fact]
    public async Task PaymentRepository_GetByInvoice_ShouldReturnMatching()
    {
        using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var invoiceId = Guid.NewGuid();
        var payment = Payment.Create("test-tenant", "PAY-INV-001", Guid.NewGuid(), 300m, "USD", PaymentMethodType.BankTransfer, invoiceId: invoiceId);

        await repository.AddAsync(payment);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByInvoiceAsync(invoiceId);

        retrieved.Should().ContainSingle(p => p.PaymentNumber == "PAY-INV-001");
    }

    [Fact]
    public async Task PaymentRepository_GetRefundsFiltered_ShouldReturnRefunds()
    {
        using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var payment = Payment.Create("test-tenant", "PAY-RFD-001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);
        payment.Complete();
        payment.Refund(100m, "reason1");
        payment.Refund(50m, "reason2");

        await repository.AddAsync(payment);
        await context.SaveChangesAsync();

        var refunds = await repository.GetRefundsFilteredAsync(payment.Id, null, null, null, 1, 20);

        refunds.Should().HaveCount(2);
    }

    [Fact]
    public async Task PaymentGatewayRepository_AddAndRetrieve()
    {
        using var context = CreateDbContext();
        var repository = new PaymentGatewayRepository(context);

        var gateway = PaymentGateway.Create(
            "test-tenant", "Test Gateway", PaymentProvider.Stripe,
            "{\"key\":\"val\"}", new[] { "USD" },
            5m, 5000m, 1.5m, FeeType.Fixed);

        await repository.AddAsync(gateway);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(gateway.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Gateway");
        retrieved.Provider.Should().Be(PaymentProvider.Stripe);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task PaymentGatewayRepository_GetActiveGateways_ShouldReturnActive()
    {
        using var context = CreateDbContext();
        var repository = new PaymentGatewayRepository(context);

        var active = PaymentGateway.Create("test-tenant", "Active", PaymentProvider.Stripe, "{}", new[] { "USD" }, null, null, 0, FeeType.Fixed);
        var inactive = PaymentGateway.Create("test-tenant", "Inactive", PaymentProvider.PayPal, "{}", new[] { "USD" }, null, null, 0, FeeType.Fixed);
        inactive.Deactivate();

        await repository.AddAsync(active);
        await repository.AddAsync(inactive);
        await context.SaveChangesAsync();

        var activeGateways = await repository.GetActiveGatewaysAsync("test-tenant");

        activeGateways.Should().ContainSingle(g => g.Name == "Active");
        activeGateways.Should().NotContain(g => g.Name == "Inactive");
    }

    [Fact]
    public async Task PaymentReconciliationRepository_AddAndRetrieve()
    {
        using var context = CreateDbContext();
        var repository = new PaymentReconciliationRepository(context);

        var reconciliation = PaymentReconciliation.Create("test-tenant", "Bank", "statement.csv", 1000m, "USD", "admin");
        var item = new ReconciliationItem(Guid.NewGuid(), reconciliation.Id, "EXT001", 500m, "USD", DateTime.UtcNow, "Test");
        reconciliation.AddItem(item);

        await repository.AddAsync(reconciliation);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetWithItemsAsync(reconciliation.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Items.Should().ContainSingle(i => i.ExternalReference == "EXT001");
        retrieved.Status.Should().Be(ReconciliationStatus.Imported);
    }

    [Fact]
    public async Task PaymentRepository_GenerateNextPaymentNumber_ShouldReturnSequential()
    {
        using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var first = await repository.GenerateNextPaymentNumberAsync();
        first.Should().Be("PAY-000001");

        var payment = Payment.Create("test-tenant", first, Guid.NewGuid(), 100m, "USD", PaymentMethodType.Cash);
        await repository.AddAsync(payment);
        await context.SaveChangesAsync();

        var second = await repository.GenerateNextPaymentNumberAsync();
        second.Should().Be("PAY-000002");
    }

    [Fact]
    public async Task PaymentRepository_GetPaymentSummary_ShouldCalculateCorrectly()
    {
        using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var p1 = Payment.Create("test-tenant", "PAY-SUM-001", Guid.NewGuid(), 100m, "USD", PaymentMethodType.Cash);
        p1.Complete();
        var p2 = Payment.Create("test-tenant", "PAY-SUM-002", Guid.NewGuid(), 200m, "USD", PaymentMethodType.Cash);
        p2.Complete();
        var p3 = Payment.Create("test-tenant", "PAY-SUM-003", Guid.NewGuid(), 300m, "USD", PaymentMethodType.Cash);
        p3.Fail("failed");

        await repository.AddAsync(p1);
        await repository.AddAsync(p2);
        await repository.AddAsync(p3);
        await context.SaveChangesAsync();

        var summary = await repository.GetPaymentSummaryAsync();

        summary.TotalPayments.Should().Be(3);
        summary.CompletedCount.Should().Be(2);
        summary.FailedCount.Should().Be(1);
        summary.TotalAmount.Should().Be(600m);
    }
}
