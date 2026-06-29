using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Application.Queries.GetInvoiceById;
using Obss.Invoices.Application.Queries.GetInvoicesByCustomer;
using Obss.Invoices.Application.Queries.GetOverdueInvoices;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Tests.Application;

public class QueryHandlerTests
{
    private static Invoice CreateInvoice(string number = "INV-2026-00001")
    {
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        return Invoice.Create(tenantId, number, Guid.NewGuid(),
            "Test Customer", "test@example.com", "123 Test St",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnInvoice_WhenFound()
    {
        var invoice = CreateInvoice();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdWithDetailsAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        var handler = new GetInvoiceByIdQueryHandler(repository);
        var query = new GetInvoiceByIdQuery(invoice.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(invoice.Id);
        result.Value.CustomerName.Should().Be("Test Customer");
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnNotFound_WhenMissing()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var handler = new GetInvoiceByIdQueryHandler(repository);
        var query = new GetInvoiceByIdQuery(Guid.NewGuid());

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetInvoicesByCustomer_ShouldReturnInvoices()
    {
        var customerId = Guid.NewGuid();
        var invoice1 = CreateInvoice("INV-2026-00001");
        var invoice2 = CreateInvoice("INV-2026-00002");
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByCustomerAsync(customerId, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Invoice> { invoice1, invoice2 });

        var handler = new GetInvoicesByCustomerQueryHandler(repository);
        var query = new GetInvoicesByCustomerQuery(customerId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInvoicesByCustomer_ShouldFilterByStatus_WhenSpecified()
    {
        var customerId = Guid.NewGuid();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByCustomerAsync(customerId, InvoiceStatus.Draft, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Invoice>());

        var handler = new GetInvoicesByCustomerQueryHandler(repository);
        var query = new GetInvoicesByCustomerQuery(customerId, Status: "Draft");

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetOverdueInvoices_ShouldReturnOverdueInvoices()
    {
        var overdueInvoice = CreateInvoice("INV-2026-00001");
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetOverdueInvoicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Invoice> { overdueInvoice });

        var handler = new GetOverdueInvoicesQueryHandler(repository);
        var query = new GetOverdueInvoicesQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOverdueInvoices_ShouldReturnEmptyList_WhenNoneOverdue()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetOverdueInvoicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Invoice>());

        var handler = new GetOverdueInvoicesQueryHandler(repository);
        var query = new GetOverdueInvoicesQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
