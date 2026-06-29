using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Commands.AddAdjustment;
using Obss.Billing.Application.Commands.ApplyTaxExemption;
using Obss.Billing.Application.Commands.CreateTaxRule;
using Obss.Billing.Application.Commands.FinalizeBill;
using Obss.Billing.Application.Commands.GenerateBillingCycle;
using Obss.Billing.Application.Queries.GetBillById;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.Billing.Infrastructure.Persistence;
using Obss.Billing.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Tests;

public class CommandHandlerTests : BillingIntegrationTests
{
    [Fact]
    public async Task CreateTaxRuleCommand_ShouldCreateTaxRuleInDatabase()
    {
        using var context = CreateDbContext();
        var taxRuleRepository = new TaxRuleRepository(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("tenant-1");
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<CreateTaxRuleCommandHandler>>();

        var handler = new CreateTaxRuleCommandHandler(taxRuleRepository, currentTenant, unitOfWork, logger);

        var command = new CreateTaxRuleCommand(
            "VAT Standard", "Standard VAT", 0.15m,
            "Percentage", "goods", "YE", "Sana'a",
            false, 1, DateTime.UtcNow.AddDays(-30), null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("VAT Standard");
        result.Value.TaxRate.Should().Be(0.15m);

        var saved = await taxRuleRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("VAT Standard");
    }

    [Fact]
    public async Task ApplyTaxExemptionCommand_ShouldCreateExemptionInDatabase()
    {
        using var context = CreateDbContext();
        var taxRuleRepository = new TaxRuleRepository(context);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("tenant-1");
        var unitOfWork = CreateUnitOfWork(context);

        var createLogger = Substitute.For<ILogger<CreateTaxRuleCommandHandler>>();
        var createHandler = new CreateTaxRuleCommandHandler(taxRuleRepository, currentTenant, unitOfWork, createLogger);
        var createResult = await createHandler.Handle(
            new CreateTaxRuleCommand(
                "VAT", "desc", 0.15m, "Percentage", "goods", "YE", "",
                false, 1, DateTime.UtcNow.AddDays(-30), null),
            CancellationToken.None);

        var applyLogger = Substitute.For<ILogger<ApplyTaxExemptionCommandHandler>>();
        var applyHandler = new ApplyTaxExemptionCommandHandler(taxRuleRepository, currentTenant, unitOfWork, applyLogger);

        var command = new ApplyTaxExemptionCommand(
            Guid.NewGuid(), createResult.Value.Id, "CERT-001",
            0.5m, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(30),
            "admin@test.com");

        var result = await applyHandler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExemptionCertificate.Should().Be("CERT-001");
    }

    [Fact]
    public async Task GenerateBillingCycleCommand_ShouldCreateCycleInDatabase()
    {
        using var context = CreateDbContext();
        var billingCycleRepository = new BillingCycleRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<GenerateBillingCycleCommandHandler>>();

        var handler = new GenerateBillingCycleCommandHandler(billingCycleRepository, unitOfWork, logger);

        var customerId = Guid.NewGuid();
        var command = new GenerateBillingCycleCommand(
            customerId, "Monthly",
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.BillingPeriod.Should().Be("Monthly");

        var saved = await billingCycleRepository.GetByCustomerAsync(customerId);
        saved.Should().NotBeNull();
        saved!.BillingPeriod.Should().Be(BillingPeriod.Monthly);
    }

    [Fact]
    public async Task FinalizeBillCommand_ShouldFinalizeBillInDatabase()
    {
        using var context = CreateDbContext();
        var billRepository = new BillRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var tenantId = "tenant-1";
        var customerId = Guid.NewGuid();
        var bill = CreateTestBill(tenantId, customerId, billRepository, context);

        var finalizeLogger = Substitute.For<ILogger<FinalizeBillCommandHandler>>();
        var finalizeHandler = new FinalizeBillCommandHandler(billRepository, unitOfWork, finalizeLogger);

        var result = await finalizeHandler.Handle(new FinalizeBillCommand(bill.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await billRepository.GetByIdWithLinesAsync(bill.Id);
        saved!.Status.Should().Be(BillStatus.Finalized);
        saved.FinalizedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBillByIdQuery_WithExistingBill_ShouldReturnBill()
    {
        using var context = CreateDbContext();
        var billRepository = new BillRepository(context);

        var bill = CreateTestBill("tenant-1", Guid.NewGuid(), billRepository, context);

        var queryHandler = new GetBillByIdQueryHandler(billRepository);
        var result = await queryHandler.Handle(new GetBillByIdQuery(bill.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(bill.Id);
    }

    [Fact]
    public async Task GetBillByIdQuery_WithNonExistingBill_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var billRepository = new BillRepository(context);

        var handler = new GetBillByIdQueryHandler(billRepository);
        var result = await handler.Handle(new GetBillByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task AddAdjustmentCommand_ShouldAddAdjustmentToBillInDatabase()
    {
        using var context = CreateDbContext();
        var billRepository = new BillRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var bill = CreateTestBill("tenant-1", Guid.NewGuid(), billRepository, context);

        var adjustLogger = Substitute.For<ILogger<AddAdjustmentCommandHandler>>();
        var adjustHandler = new AddAdjustmentCommandHandler(billRepository, unitOfWork, adjustLogger);

        var result = await adjustHandler.Handle(
            new AddAdjustmentCommand(bill.Id, "Credit", -20m, "USD"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await billRepository.GetByIdWithLinesAsync(bill.Id);
        saved!.Lines.Should().Contain(l => l.LineType == LineType.Adjustment);
    }

    [Fact]
    public async Task FinalizeBillCommand_WithNonExistingBill_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var billRepository = new BillRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<FinalizeBillCommandHandler>>();

        var handler = new FinalizeBillCommandHandler(billRepository, unitOfWork, logger);

        var result = await handler.Handle(new FinalizeBillCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    private static Bill CreateTestBill(
        string tenantId,
        Guid customerId,
        BillRepository billRepository,
        BillingDbContext context)
    {
        var bill = Bill.Create(
            tenantId, customerId, "Test Customer",
            BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");

        var line = BillLine.CreateRecurring(
            bill.Id, "Monthly Subscription", Guid.NewGuid(), null, null,
            1, 150m, 0, 0.05m, "USD", DateTime.UtcNow);

        bill.AddLine(line);
        billRepository.AddAsync(bill).GetAwaiter().GetResult();
        context.SaveChangesAsync().GetAwaiter().GetResult();

        bill.CalculateTotals();
        context.SaveChangesAsync().GetAwaiter().GetResult();

        return bill;
    }
}
