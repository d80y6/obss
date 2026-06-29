using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.Commands.RecordPayment;
using Obss.Payments.Application.Commands.CompletePayment;
using Obss.Payments.Application.Commands.RefundPayment;
using Obss.Payments.Application.Commands.AllocatePayment;
using Obss.Payments.Application.Commands.ReconcilePayment;
using Obss.Payments.Application.Commands.RegisterPaymentGateway;
using Obss.Payments.Application.Commands.ImportBankStatement;
using Obss.Payments.Application.Commands.ProcessGatewayPayment;
using Obss.Payments.Application.Commands.AutoReconcile;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Payments.Tests.Application;

public class MockCommandHandlerTests
{
    [Fact]
    public async Task RecordPayment_Valid_ReturnsSuccess()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        var ct = Substitute.For<ICurrentTenant>();
        ct.TenantId.Returns("t1");
        repo.GenerateNextPaymentNumberAsync(default).ReturnsForAnyArgs("PAY-1");
        var h = new RecordPaymentCommandHandler(repo, uow, ct, Substitute.For<ILogger<RecordPaymentCommandHandler>>());
        var r = await h.Handle(new RecordPaymentCommand(Guid.NewGuid(), 100m, "USD", "Cash", null, null, null), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RecordPayment_InvalidMethod_ReturnsFailure()
    {
        var h = new RecordPaymentCommandHandler(Substitute.For<IPaymentRepository>(), Substitute.For<IUnitOfWork>(), Substitute.For<ICurrentTenant>(), Substitute.For<ILogger<RecordPaymentCommandHandler>>());
        var r = await h.Handle(new RecordPaymentCommand(Guid.NewGuid(), 100m, "USD", "Bad", null, null, null), CancellationToken.None);
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CompletePayment_Existing_Succeeds()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var p = Payment.Create("t", "P-1", Guid.NewGuid(), 100m, "USD", PaymentMethodType.Cash);
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(p);
        var h = new CompletePaymentCommandHandler(repo, Substitute.For<IUnitOfWork>(), Substitute.For<ILogger<CompletePaymentCommandHandler>>());
        var r = await h.Handle(new CompletePaymentCommand(p.Id), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CompletePayment_Missing_ReturnsNotFound()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var h = new CompletePaymentCommandHandler(repo, Substitute.For<IUnitOfWork>(), Substitute.For<ILogger<CompletePaymentCommandHandler>>());
        var r = await h.Handle(new CompletePaymentCommand(Guid.NewGuid()), CancellationToken.None);
        r.IsFailure.Should().BeTrue();
        r.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task RefundPayment_Valid_Succeeds()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var p = Payment.Create("t", "P-1", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);
        p.Complete();
        repo.GetByIdWithDetailsAsync(p.Id, Arg.Any<CancellationToken>()).Returns(p);
        var h = new RefundPaymentCommandHandler(repo, Substitute.For<IUnitOfWork>(), Substitute.For<ILogger<RefundPaymentCommandHandler>>());
        var r = await h.Handle(new RefundPaymentCommand(p.Id, 200m, "reason"), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AllocatePayment_Valid_Succeeds()
    {
        var repo = Substitute.For<IPaymentRepository>();
        var p = Payment.Create("t", "P-1", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);
        p.Complete();
        repo.GetByIdWithDetailsAsync(p.Id, Arg.Any<CancellationToken>()).Returns(p);
        var h = new AllocatePaymentCommandHandler(repo, Substitute.For<IUnitOfWork>(), Substitute.For<ILogger<AllocatePaymentCommandHandler>>());
        var r = await h.Handle(new AllocatePaymentCommand(p.Id, Guid.NewGuid(), 200m), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GatewayPayment_Success_RecordsPayment()
    {
        var gs = Substitute.For<IPaymentGatewayService>();
        gs.ProcessPayment(Arg.Any<PaymentRequest>(), Arg.Any<CancellationToken>()).Returns(new PaymentResult(true, "txn1", PaymentStatus.Completed, null, null));
        var repo = Substitute.For<IPaymentRepository>();
        repo.GenerateNextPaymentNumberAsync(default).ReturnsForAnyArgs("PAY-1");
        var ct = Substitute.For<ICurrentTenant>(); ct.TenantId.Returns("t");
        var h = new ProcessGatewayPaymentCommandHandler(gs, repo, Substitute.For<IUnitOfWork>(), ct, Substitute.For<ILogger<ProcessGatewayPaymentCommandHandler>>());
        var r = await h.Handle(new ProcessGatewayPaymentCommand(100m, "USD", "CreditCard", null, null, Guid.NewGuid(), "d"), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReconcilePayment_Valid_Succeeds()
    {
        var rr = Substitute.For<IPaymentReconciliationRepository>();
        var rec = PaymentReconciliation.Create("t", "B", null, 0, "USD", "a");
        var item = new ReconciliationItem(Guid.NewGuid(), rec.Id, "E1", 500m, "USD", DateTime.UtcNow, null);
        rec.AddItem(item);
        rr.GetWithItemsAsync(rec.Id, Arg.Any<CancellationToken>()).Returns(rec);
        var h = new ReconcilePaymentCommandHandler(rr, Substitute.For<IUnitOfWork>(), Substitute.For<ILogger<ReconcilePaymentCommandHandler>>());
        var r = await h.Handle(new ReconcilePaymentCommand(rec.Id, item.Id, Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterGateway_Valid_Succeeds()
    {
        var gr = Substitute.For<IPaymentGatewayRepository>();
        var ct = Substitute.For<ICurrentTenant>(); ct.TenantId.Returns("t");
        var h = new RegisterPaymentGatewayCommandHandler(gr, Substitute.For<IUnitOfWork>(), ct, Substitute.For<ILogger<RegisterPaymentGatewayCommandHandler>>());
        var r = await h.Handle(new RegisterPaymentGatewayCommand("S", "Stripe", "{}", ["USD"], 0, 1000, 2.5m, "Percentage"), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ImportStatement_ReturnsReconciliation()
    {
        var rr = Substitute.For<IPaymentReconciliationRepository>();
        var ct = Substitute.For<ICurrentTenant>(); ct.TenantId.Returns("t");
        var h = new ImportBankStatementCommandHandler(rr, Substitute.For<IUnitOfWork>(), ct, Substitute.For<ILogger<ImportBankStatementCommandHandler>>());
        var r = await h.Handle(new ImportBankStatementCommand("Bank", "f.csv", "USD", [new BankTransactionLine("E1", 500m, DateTime.UtcNow, "d")]), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.TotalImportAmount.Should().Be(500m);
    }

    [Fact]
    public async Task AutoReconcile_MatchesByReference()
    {
        var rr = Substitute.For<IPaymentReconciliationRepository>();
        var pr = Substitute.For<IPaymentRepository>();
        var rec = PaymentReconciliation.Create("t", "B", null, 0, "USD", "a");
        var item = new ReconciliationItem(Guid.NewGuid(), rec.Id, "PAY-1", 500m, "USD", DateTime.UtcNow, null);
        rec.AddItem(item);
        rr.GetWithItemsAsync(rec.Id, Arg.Any<CancellationToken>()).Returns(rec);
        var pay = Payment.Create("t", "PAY-1", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash, "PAY-1");
        pr.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Payment> { pay });
        var h = new AutoReconcileCommandHandler(rr, pr, Substitute.For<IUnitOfWork>(), Substitute.For<ILogger<AutoReconcileCommandHandler>>());
        var r = await h.Handle(new AutoReconcileCommand(rec.Id), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }
}
