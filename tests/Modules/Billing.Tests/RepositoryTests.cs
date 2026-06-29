using FluentAssertions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.Billing.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Obss.Billing.Tests;

public class RepositoryTests : BillingIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveBill()
    {
        using var context = CreateDbContext();
        var repository = new BillRepository(context);

        var bill = Bill.Create(
            "tenant-1", Guid.NewGuid(), "Test Customer",
            BillingPeriod.Monthly,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc), "USD");

        await repository.AddAsync(bill);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(bill.Id);

        retrieved.Should().NotBeNull();
        retrieved!.CustomerName.Should().Be("Test Customer");
        retrieved.Status.Should().Be(BillStatus.Draft);
        retrieved.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task CanAddBillWithLines()
    {
        using var context = CreateDbContext();
        var repository = new BillRepository(context);

        var bill = Bill.Create(
            "tenant-1", Guid.NewGuid(), "Customer",
            BillingPeriod.Monthly,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc), "USD");

        var line = BillLine.CreateRecurring(
            bill.Id, "Service Fee", Guid.NewGuid(), null, null,
            2, 75m, 10m, 0.1m, "USD", DateTime.UtcNow);

        bill.AddLine(line);
        await repository.AddAsync(bill);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdWithLinesAsync(bill.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Lines.Should().ContainSingle();
        retrieved.Lines.First().Description.Should().Be("Service Fee");
        retrieved.Lines.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task CanAddAndRetrieveTaxRule()
    {
        using var context = CreateDbContext();
        var repository = new TaxRuleRepository(context);

        var taxRule = TaxRule.Create(
            "tenant-1", "VAT", "Standard VAT", 0.15m,
            TaxType.Percentage, "goods", "YE", "Sana'a",
            false, 1, DateTime.UtcNow.AddDays(-30), null);

        await repository.AddAsync(taxRule);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(taxRule.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("VAT");
        retrieved.TaxRate.Should().Be(0.15m);
        retrieved.TaxType.Should().Be(TaxType.Percentage);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanGetApplicableTaxRules()
    {
        using var context = CreateDbContext();
        var repository = new TaxRuleRepository(context);

        var rule1 = TaxRule.Create(
            "tenant-1", "VAT Goods", "Goods VAT", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);
        var rule2 = TaxRule.Create(
            "tenant-1", "VAT Services", "Services VAT", 0.05m,
            TaxType.Percentage, "services", "YE", "", false, 2,
            DateTime.UtcNow.AddDays(-30), null);

        await repository.AddAsync(rule1);
        await repository.AddAsync(rule2);
        await context.SaveChangesAsync();

        var applicableGoods = await repository.GetApplicableRulesAsync("goods", "YE");
        var applicableServices = await repository.GetApplicableRulesAsync("services", "YE");

        applicableGoods.Should().ContainSingle(r => r.Name == "VAT Goods");
        applicableServices.Should().ContainSingle(r => r.Name == "VAT Services");
    }

    [Fact]
    public async Task CanAddAndRetrieveExemption()
    {
        using var context = CreateDbContext();
        var repository = new TaxRuleRepository(context);

        var exemption = TaxExemption.Create(
            "tenant-1", Guid.NewGuid(), Guid.NewGuid(),
            "CERT-100", 0.5m,
            DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(30),
            "admin");

        await repository.AddExemptionAsync(exemption);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetCustomerExemptionsAsync(exemption.CustomerId);

        retrieved.Should().ContainSingle();
        retrieved[0].ExemptionCertificate.Should().Be("CERT-100");
    }

    [Fact]
    public async Task CanAddAndRetrieveBillingCycle()
    {
        using var context = CreateDbContext();
        var repository = new BillingCycleRepository(context);

        var cycle = BillingCycle.Create(
            "tenant-1", Guid.NewGuid(), BillingPeriod.Monthly,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        await repository.AddAsync(cycle);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByCustomerAsync(cycle.CustomerId);

        retrieved.Should().NotBeNull();
        retrieved!.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        retrieved.Status.Should().Be(BillingCycleStatus.Active);
    }

    [Fact]
    public async Task GetCyclesDue_ShouldReturnOnlyDueCycles()
    {
        using var context = CreateDbContext();
        var repository = new BillingCycleRepository(context);

        var dueCycle = BillingCycle.Create(
            "tenant-1", Guid.NewGuid(), BillingPeriod.Monthly,
            new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var futureCycle = BillingCycle.Create(
            "tenant-1", Guid.NewGuid(), BillingPeriod.Monthly,
            new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        await repository.AddAsync(dueCycle);
        await repository.AddAsync(futureCycle);
        await context.SaveChangesAsync();

        var due = await repository.GetCyclesDueAsync(new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc));

        due.Should().ContainSingle(c => c.CustomerId == dueCycle.CustomerId);
        due.Should().NotContain(c => c.CustomerId == futureCycle.CustomerId);
    }

    [Fact]
    public async Task CanGetBillsByCustomer()
    {
        using var context = CreateDbContext();
        var repository = new BillRepository(context);

        var customerId = Guid.NewGuid();
        var bill1 = Bill.Create(
            "tenant-1", customerId, "Customer", BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow, "USD");
        var bill2 = Bill.Create(
            "tenant-1", customerId, "Customer", BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");

        await repository.AddAsync(bill1);
        await repository.AddAsync(bill2);
        await context.SaveChangesAsync();

        var bills = await repository.GetByCustomerAsync(customerId, null, null, null, 1, 10);

        bills.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOpenBills_ShouldReturnOnlyFinalizedBills()
    {
        using var context = CreateDbContext();
        var repository = new BillRepository(context);

        var tenantId = "tenant-1";
        var customerId = Guid.NewGuid();

        var draftBill = Bill.Create(
            tenantId, customerId, "Customer", BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");
        var finalizedBill = Bill.Create(
            tenantId, Guid.NewGuid(), "Customer2", BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");

        await repository.AddAsync(draftBill);
        await repository.AddAsync(finalizedBill);
        await context.SaveChangesAsync();

        finalizedBill.AddLine(BillLine.CreateRecurring(
            finalizedBill.Id, "Item", Guid.NewGuid(), null, null,
            1, 100m, 0, 0m, "USD", DateTime.UtcNow));
        finalizedBill.CalculateTotals();
        finalizedBill.MarkAsFinalized();
        await context.SaveChangesAsync();

        var openBills = await repository.GetOpenBillsAsync();

        openBills.Should().ContainSingle(b => b.Id == finalizedBill.Id);
        openBills.Should().NotContain(b => b.Id == draftBill.Id);
    }
}
