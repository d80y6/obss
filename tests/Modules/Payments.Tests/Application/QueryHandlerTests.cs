using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.Queries.GetPaymentById;
using Obss.Payments.Application.Queries.GetPayments;
using Obss.Payments.Application.Queries.GetPaymentsByInvoice;
using Obss.Payments.Application.Queries.GetPaymentSummary;
using Obss.Payments.Application.Queries.GetRefunds;
using Obss.Payments.Application.Queries.GetReconciliations;
using Obss.Payments.Application.Queries.GetReconciliationStatus;
using Obss.Payments.Application.Queries.GetUnmatchedTransactions;
using Obss.Payments.Application.Queries.GetSupportedGateways;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Payments.Tests.Application;

public class MockQueryHandlerTests
{
    [Fact]
    public async Task GetPaymentById_Existing_ReturnsPayment()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var p = Payment.Create("t", "P-1", Guid.NewGuid(), 100m, "USD", PaymentMethodType.Cash);
        repo.GetByIdWithDetailsAsync(p.Id, Arg.Any<CancellationToken>()).Returns(p);
        var r = await new GetPaymentByIdQueryHandler(repo).Handle(new GetPaymentByIdQuery(p.Id), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.PaymentNumber.Should().Be("P-1");
    }

    [Fact]
    public async Task GetPaymentById_Missing_ReturnsNotFound()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var r = await new GetPaymentByIdQueryHandler(repo).Handle(new GetPaymentByIdQuery(Guid.NewGuid()), CancellationToken.None);
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetPayments_ReturnsList()
    {
        var repo = Substitute.For<IPaymentRepository>();
        repo.GetFilteredAsync(Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(new List<Payment> { Payment.Create("t", "P1", Guid.NewGuid(), 100m, "USD", PaymentMethodType.Cash) });
        var r = await new GetPaymentsQueryHandler(repo).Handle(new GetPaymentsQuery(null, null, null, null, 1, 20), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPaymentsByInvoice_ReturnsPayments()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var iid = Guid.NewGuid();
        repo.GetByInvoiceAsync(iid, Arg.Any<CancellationToken>()).Returns(new List<Payment> { Payment.Create("t", "P1", Guid.NewGuid(), 100m, "USD", PaymentMethodType.Cash, invoiceId: iid) });
        var r = await new GetPaymentsByInvoiceQueryHandler(repo).Handle(new GetPaymentsByInvoiceQuery(iid), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().ContainSingle();
    }

    [Fact]
    public async Task GetRefunds_ReturnsFiltered()
    {
        var repo = Substitute.For<IPaymentRepository>();
        repo.GetRefundsFilteredAsync(Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(new List<Refund> { new(Guid.NewGuid(), Guid.NewGuid(), 100m, "r", DateTime.UtcNow) });
        var r = await new GetRefundsQueryHandler(repo).Handle(new GetRefundsQuery(null, null, null, null, 1, 20), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPaymentSummary_ReturnsSummary()
    {
        var repo = Substitute.For<IPaymentRepository>();
        repo.GetPaymentSummaryAsync(Arg.Any<CancellationToken>()).Returns(new PaymentSummary(10, 2, 5, 1, 1, 1, 1000m, 500m, 100m, 900m));
        var r = await new GetPaymentSummaryQueryHandler(repo).Handle(new GetPaymentSummaryQuery(), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.TotalPayments.Should().Be(10);
    }

    [Fact]
    public async Task GetReconciliations_ReturnsList()
    {
        var rr = Substitute.For<IPaymentReconciliationRepository>();
        rr.GetFilteredAsync(Arg.Any<string?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(new List<PaymentReconciliation> { PaymentReconciliation.Create("t", "B", null, 1000m, "USD", "a") });
        var r = await new GetReconciliationsQueryHandler(rr).Handle(new GetReconciliationsQuery(null, null, null, 1, 20), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().ContainSingle();
    }

    [Fact]
    public async Task GetReconciliationStatus_Existing_Returns()
    {
        var rr = Substitute.For<IPaymentReconciliationRepository>();
        var rec = PaymentReconciliation.Create("t", "B", null, 1000m, "USD", "a");
        rr.GetWithItemsAsync(rec.Id, Arg.Any<CancellationToken>()).Returns(rec);
        var r = await new GetReconciliationStatusQueryHandler(rr).Handle(new GetReconciliationStatusQuery(rec.Id), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetUnmatchedTransactions_ReturnsItems()
    {
        var rr = Substitute.For<IPaymentReconciliationRepository>();
        var ct = Substitute.For<ICurrentTenant>(); ct.TenantId.Returns("t");
        rr.GetUnmatchedItemsAsync("t", Arg.Any<CancellationToken>()).Returns(new List<ReconciliationItem> { new(Guid.NewGuid(), Guid.NewGuid(), "E1", 500m, "USD", DateTime.UtcNow, null) });
        var r = await new GetUnmatchedTransactionsQueryHandler(rr, ct).Handle(new GetUnmatchedTransactionsQuery(), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetSupportedGateways_Returns()
    {
        var gs = Substitute.For<IPaymentGatewayService>();
        gs.GetSupportedGateways(Arg.Any<CancellationToken>()).Returns(new List<PaymentGatewayInfo> { new(PaymentProvider.Stripe, "S", ["USD"]) });
        var r = await new GetSupportedGatewaysQueryHandler(gs).Handle(new GetSupportedGatewaysQuery(), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }
}
