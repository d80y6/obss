using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.CRM.Application.Commands.CreateCustomer;
using Obss.CRM.Application.Commands.CreateSegment;
using Obss.CRM.Domain.ValueObjects;
using Obss.CRM.Infrastructure.Persistence;
using Obss.CRM.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.CRM.Tests;

public class CommandHandlerTests : CrmIntegrationTests
{
    [Fact]
    public async Task CreateCustomerCommand_ShouldCreateCustomerInDatabase()
    {
        using var context = CreateDbContext();
        var customerRepository = new CustomerRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateCustomerCommandHandler(customerRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateCustomerCommand(
            tenantId,
            "Business",
            "Test Corp",
            "Test Corporation",
            "TAX-999",
            "REG-888",
            "testcorp@example.com",
            "+1234567890",
            "+1",
            "https://testcorp.example.com",
            "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.DisplayName.Should().Be("Test Corporation");
        result.Value.CompanyName.Should().Be("Test Corp");
        result.Value.CustomerType.Should().Be("Business");
        result.Value.Email.Should().Be("testcorp@example.com");
        result.Value.Currency.Should().Be("USD");

        var saved = await customerRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.DisplayName.Should().Be("Test Corporation");
    }

    [Fact]
    public async Task CreateCustomerCommand_ShouldCreateResidentialCustomer()
    {
        using var context = CreateDbContext();
        var customerRepository = new CustomerRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateCustomerCommandHandler(customerRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateCustomerCommand(
            tenantId,
            "Residential",
            null,
            "John Doe",
            null,
            null,
            "john.doe@example.com",
            null,
            null,
            null,
            "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerType.Should().Be("Residential");
        result.Value.DisplayName.Should().Be("John Doe");
        result.Value.CompanyName.Should().BeNull();
    }

    [Fact]
    public async Task CreateCustomerCommand_ShouldDefaultToLeadStatus()
    {
        using var context = CreateDbContext();
        var customerRepository = new CustomerRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateCustomerCommandHandler(customerRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateCustomerCommand(
            tenantId,
            "Wholesale",
            "WholeCo",
            "WholeCo Ltd",
            null,
            null,
            "wholeco@example.com",
            null,
            null,
            null,
            "EUR");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Status.Should().Be("Lead");
    }

    [Fact]
    public async Task CreateSegmentCommand_ShouldCreateSegmentInDatabase()
    {
        using var context = CreateDbContext();
        var segmentRepository = new CustomerSegmentRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateSegmentCommandHandler(segmentRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var criteria = "{\"RuleGroups\": []}";
        var command = new CreateSegmentCommand(
            tenantId,
            "Premium",
            "Premium customers segment",
            criteria,
            100);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Premium");
        result.Value.Description.Should().Be("Premium customers segment");
        result.Value.Priority.Should().Be(100);

        var saved = await segmentRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Premium");
    }
}
