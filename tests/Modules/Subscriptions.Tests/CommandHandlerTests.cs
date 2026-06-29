using FluentAssertions;
using NSubstitute;
using Obss.SharedKernel.Domain.Common;
using Xunit;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.Commands.ActivateSubscription;
using Obss.Subscriptions.Application.Commands.CancelSubscription;
using Obss.Subscriptions.Application.Commands.ChangeOffer;
using Obss.Subscriptions.Application.Commands.CreateSubscription;
using Obss.Subscriptions.Application.Commands.OverrideEntitlementLimit;
using Obss.Subscriptions.Application.Commands.RenewSubscription;
using Obss.Subscriptions.Application.Commands.SetSubscriptionEntitlements;
using Obss.Subscriptions.Application.Commands.SuspendSubscription;
using Obss.Subscriptions.Application.Commands.UpdateEntitlementUsage;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Tests;

public class CommandHandlerTests
{
    [Fact]
    public async Task CreateSubscriptionCommand_ShouldCreateAndReturnDto()
    {
        var repository = Substitute.For<ISubscriptionRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSubscriptionCommandHandler(repository, unitOfWork);
        var subscriptionId = Guid.NewGuid();
        repository.AddAsync(Arg.Any<Subscription>()).Returns(call =>
        {
            var sub = call.Arg<Subscription>();
            typeof(Entity<Guid>).GetProperty("Id")!.SetValue(sub, subscriptionId);
            return sub;
        });

        var command = new CreateSubscriptionCommand(
            "tenant-1", Guid.NewGuid(), "John", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "Basic", BillingPeriod.Monthly,
            "USD", 99.99m, 1, DateTime.UtcNow, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(subscriptionId);
        result.Value.CustomerName.Should().Be("John");
        result.Value.OfferName.Should().Be("Basic");
        await repository.Received(1).AddAsync(Arg.Any<Subscription>());
        await unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ActivateSubscription_ShouldSucceed()
    {
        var subscription = CreatePendingSubscription();
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdAsync(subscription.Id).Returns(subscription);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ActivateSubscriptionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new ActivateSubscriptionCommand(subscription.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        await repository.Received(1).UpdateAsync(subscription);
    }

    [Fact]
    public async Task ActivateSubscription_NonExistent_ShouldReturnNotFound()
    {
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Subscription?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ActivateSubscriptionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new ActivateSubscriptionCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task ActivateSubscription_InvalidState_ShouldReturnValidationError()
    {
        var subscription = CreateActiveSubscription();
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdAsync(subscription.Id).Returns(subscription);
        var handler = new ActivateSubscriptionCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(
            new ActivateSubscriptionCommand(subscription.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CancelSubscription_ShouldSucceed()
    {
        var subscription = CreateActiveSubscription();
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdAsync(subscription.Id).Returns(subscription);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CancelSubscriptionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CancelSubscriptionCommand(subscription.Id, "Customer request", DateTime.UtcNow),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public async Task SuspendSubscription_ShouldSucceed()
    {
        var subscription = CreateActiveSubscription();
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdAsync(subscription.Id).Returns(subscription);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SuspendSubscriptionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new SuspendSubscriptionCommand(subscription.Id, "Non-payment"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
    }

    [Fact]
    public async Task RenewSubscription_ShouldSucceed()
    {
        var subscription = CreateActiveSubscription();
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdAsync(subscription.Id).Returns(subscription);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RenewSubscriptionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new RenewSubscriptionCommand(subscription.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        subscription.RenewalDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ChangeOffer_ShouldSucceed()
    {
        var subscription = CreateActiveSubscription();
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdWithDetailsAsync(subscription.Id).Returns(subscription);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ChangeOfferCommandHandler(repository, unitOfWork);
        var newOfferId = Guid.NewGuid();

        var result = await handler.Handle(
            new ChangeOfferCommand(subscription.Id, newOfferId, 199.99m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OfferId.Should().Be(newOfferId);
        result.Value.Price.Should().Be(199.99m);
    }

    [Fact]
    public async Task SetSubscriptionEntitlements_ShouldSucceed()
    {
        var subscription = CreateActiveSubscription();
        var subRepo = Substitute.For<ISubscriptionRepository>();
        subRepo.GetByIdAsync(subscription.Id).Returns(subscription);
        var entRepo = Substitute.For<ISubscriptionEntitlementRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SetSubscriptionEntitlementsCommandHandler(entRepo, subRepo, unitOfWork);

        var command = new SetSubscriptionEntitlementsCommand(subscription.Id,
        [
            new EntitlementDefinition("Bandwidth", "Bandwidth", 1000, 0, "GB", false, true, DateTime.UtcNow, null)
        ]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await entRepo.Received(1).DeleteBySubscriptionIdAsync(subscription.Id);
        await entRepo.Received(1).AddAsync(Arg.Any<SubscriptionEntitlement>());
    }

    [Fact]
    public async Task OverrideEntitlementLimit_ShouldSucceed()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(), EntitlementType.Bandwidth, "Bandwidth", 1000, 100, "GB", false, true, DateTime.UtcNow);
        var entRepo = Substitute.For<ISubscriptionEntitlementRepository>();
        entRepo.GetBySubscriptionAndTypeAsync(Arg.Any<Guid>(), EntitlementType.Bandwidth)
            .Returns(entitlement);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new OverrideEntitlementLimitCommandHandler(entRepo, unitOfWork);

        var result = await handler.Handle(
            new OverrideEntitlementLimitCommand(Guid.NewGuid(), "Bandwidth", 5000),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        entitlement.Limit.Should().Be(5000);
    }

    [Fact]
    public async Task UpdateEntitlementUsage_ShouldSucceed()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(), EntitlementType.Bandwidth, "Bandwidth", 1000, 100, "GB", false, true, DateTime.UtcNow);
        var entRepo = Substitute.For<ISubscriptionEntitlementRepository>();
        entRepo.GetBySubscriptionAndTypeAsync(Arg.Any<Guid>(), EntitlementType.Bandwidth)
            .Returns(entitlement);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateEntitlementUsageCommandHandler(entRepo, unitOfWork);

        var result = await handler.Handle(
            new UpdateEntitlementUsageCommand(Guid.NewGuid(), "Bandwidth", 50, false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        entitlement.Used.Should().Be(150);
    }

    private static Subscription CreatePendingSubscription()
    {
        return Subscription.Create(
            "tenant-1", Guid.NewGuid(), "John", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "Basic", BillingPeriod.Monthly,
            "USD", 99.99m, 1, DateTime.UtcNow.AddDays(-30));
    }

    private static Subscription CreateActiveSubscription()
    {
        var sub = CreatePendingSubscription();
        sub.Activate();
        sub.ClearDomainEvents();
        return sub;
    }
}
