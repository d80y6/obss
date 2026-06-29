using FluentAssertions;
using Obss.SharedKernel.Application.Contracts;
using Xunit;
using Obss.Subscriptions.Application.Commands.ActivateSubscription;
using Obss.Subscriptions.Application.Commands.CancelSubscription;
using Obss.Subscriptions.Application.Commands.CreateSubscription;
using Obss.Subscriptions.Application.Commands.SuspendSubscription;
using Obss.Subscriptions.Domain.ValueObjects;
using Obss.Subscriptions.Infrastructure.Persistence.Repositories;

namespace Obss.Subscriptions.Tests;

public class CommandHandlerIntegrationTests : SubscriptionIntegrationTests
{
    [Fact]
    public async Task CreateSubscriptionCommand_ShouldPersistInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var handler = new CreateSubscriptionCommandHandler(repository, unitOfWork);

        var command = new CreateSubscriptionCommand(
            "tenant-int", Guid.NewGuid(), "Integration Tester", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Integration Plan",
            BillingPeriod.Monthly, "USD", 49.99m, 3, DateTime.UtcNow, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CustomerName.Should().Be("Integration Tester");
        result.Value.OfferName.Should().Be("Integration Plan");

        var saved = await repository.GetByIdWithDetailsAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.CustomerName.Should().Be("Integration Tester");
        saved.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task ActivateSubscriptionCommand_ShouldUpdateStatus()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateSubscriptionCommandHandler(repository, unitOfWork);
        var createResult = await createHandler.Handle(
            new CreateSubscriptionCommand("t1", Guid.NewGuid(), "Activate Me", Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan",
                BillingPeriod.Monthly, "USD", 10, 1, DateTime.UtcNow, null),
            CancellationToken.None);

        var activateHandler = new ActivateSubscriptionCommandHandler(repository, unitOfWork);
        var activateResult = await activateHandler.Handle(
            new ActivateSubscriptionCommand(createResult.Value.Id), CancellationToken.None);

        activateResult.IsSuccess.Should().BeTrue();

        var saved = await repository.GetByIdAsync(createResult.Value.Id);
        saved!.Status.Should().Be(SubscriptionStatus.Active);
        saved.ActivationDate.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelSubscriptionCommand_ShouldUpdateStatus()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateSubscriptionCommandHandler(repository, unitOfWork);
        var createResult = await createHandler.Handle(
            new CreateSubscriptionCommand("t1", Guid.NewGuid(), "Cancel Me", Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan",
                BillingPeriod.Monthly, "USD", 10, 1, DateTime.UtcNow, null),
            CancellationToken.None);

        var activateHandler = new ActivateSubscriptionCommandHandler(repository, unitOfWork);
        await activateHandler.Handle(
            new ActivateSubscriptionCommand(createResult.Value.Id), CancellationToken.None);

        var cancelHandler = new CancelSubscriptionCommandHandler(repository, unitOfWork);
        var cancelResult = await cancelHandler.Handle(
            new CancelSubscriptionCommand(createResult.Value.Id, "Testing cancellation", DateTime.UtcNow),
            CancellationToken.None);

        cancelResult.IsSuccess.Should().BeTrue();

        var saved = await repository.GetByIdAsync(createResult.Value.Id);
        saved!.Status.Should().Be(SubscriptionStatus.Cancelled);
        saved.CancelledAt.Should().NotBeNull();
        saved.EndDate.Should().NotBeNull();
    }

    [Fact]
    public async Task SuspendAndResumeSubscription_ShouldUpdateStatus()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateSubscriptionCommandHandler(repository, unitOfWork);
        var createResult = await createHandler.Handle(
            new CreateSubscriptionCommand("t1", Guid.NewGuid(), "Suspend Me", Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan",
                BillingPeriod.Monthly, "USD", 10, 1, DateTime.UtcNow, null),
            CancellationToken.None);

        var activateHandler = new ActivateSubscriptionCommandHandler(repository, unitOfWork);
        await activateHandler.Handle(
            new ActivateSubscriptionCommand(createResult.Value.Id), CancellationToken.None);

        var suspendHandler = new SuspendSubscriptionCommandHandler(repository, unitOfWork);
        var suspendResult = await suspendHandler.Handle(
            new SuspendSubscriptionCommand(createResult.Value.Id, "Non-payment"),
            CancellationToken.None);

        suspendResult.IsSuccess.Should().BeTrue();

        var saved = await repository.GetByIdAsync(createResult.Value.Id);
        saved!.Status.Should().Be(SubscriptionStatus.Suspended);
        saved.SuspendedAt.Should().NotBeNull();
    }
}
