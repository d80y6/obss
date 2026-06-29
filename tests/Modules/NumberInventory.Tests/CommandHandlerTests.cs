using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.NumberInventory.Application.Commands.AddNumber;
using Obss.NumberInventory.Application.Commands.AssignNumber;
using Obss.NumberInventory.Application.Commands.ReleaseNumber;
using Obss.NumberInventory.Application.Mappings;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.NumberInventory.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NumberInventory.Tests;

public class CommandHandlerTests : NumberInventoryIntegrationTests
{
    public CommandHandlerTests()
    {
        NumberInventoryMappingConfig.Configure();
    }

    [Fact]
    public async Task AddNumberCommand_ShouldCreateNumberInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new TelephoneNumberRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new AddNumberCommandHandler(repository, unitOfWork, CreateCurrentTenant("test-tenant"));

        var command = new AddNumberCommand("+1234567890", NumberType.Mobile, 10.0m, "USD", "Test number");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("+1234567890");
        result.Value.NumberType.Should().Be("Mobile");
        result.Value.Status.Should().Be("Available");

        var saved = await repository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Number.Should().Be("+1234567890");
        saved.Status.Should().Be(NumberStatus.Available);
    }

    [Fact]
    public async Task AssignNumberCommand_ShouldAssignNumberInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new TelephoneNumberRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var addHandler = new AddNumberCommandHandler(repository, unitOfWork, CreateCurrentTenant("test-tenant"));
        var addResult = await addHandler.Handle(
            new AddNumberCommand("+1234567890", NumberType.Mobile, 10.0m, "USD", null),
            CancellationToken.None);

        var assignHandler = new AssignNumberCommandHandler(repository, unitOfWork);
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        var assignResult = await assignHandler.Handle(
            new AssignNumberCommand(addResult.Value.Id, customerId, subscriptionId),
            CancellationToken.None);

        assignResult.IsSuccess.Should().BeTrue();

        var saved = await repository.GetByIdAsync(addResult.Value.Id);
        saved!.Status.Should().Be(NumberStatus.Assigned);
        saved.CustomerId.Should().Be(customerId);
        saved.SubscriptionId.Should().Be(subscriptionId);
    }

    [Fact]
    public async Task ReleaseNumberCommand_ShouldReleaseNumberInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new TelephoneNumberRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var addHandler = new AddNumberCommandHandler(repository, unitOfWork, CreateCurrentTenant("test-tenant"));
        var addResult = await addHandler.Handle(
            new AddNumberCommand("+1234567890", NumberType.Mobile, 10.0m, "USD", null),
            CancellationToken.None);

        var assignHandler = new AssignNumberCommandHandler(repository, unitOfWork);
        await assignHandler.Handle(
            new AssignNumberCommand(addResult.Value.Id, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        var releaseHandler = new ReleaseNumberCommandHandler(repository, unitOfWork);

        var releaseResult = await releaseHandler.Handle(
            new ReleaseNumberCommand(addResult.Value.Id),
            CancellationToken.None);

        releaseResult.IsSuccess.Should().BeTrue();

        var saved = await repository.GetByIdAsync(addResult.Value.Id);
        saved!.Status.Should().Be(NumberStatus.Available);
        saved.CustomerId.Should().BeNull();
        saved.SubscriptionId.Should().BeNull();
    }

    private static ICurrentTenant CreateCurrentTenant(string tenantId)
    {
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.TenantId.Returns(tenantId);
        return tenant;
    }
}
