using FluentAssertions;
using NSubstitute;
using Xunit;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.Queries.CheckEntitlementAvailability;
using Obss.Subscriptions.Application.Queries.GetActiveSubscriptionsByCustomer;
using Obss.Subscriptions.Application.Queries.GetSubscriptionById;
using Obss.Subscriptions.Application.Queries.GetSubscriptions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Tests;

public class QueryHandlerTests
{
    [Fact]
    public async Task GetSubscriptionById_ShouldReturnSubscription()
    {
        var subscription = CreateTestSubscription();
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdWithDetailsAsync(subscription.Id).Returns(subscription);
        var handler = new GetSubscriptionByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetSubscriptionByIdQuery(subscription.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(subscription.Id);
        result.Value.CustomerName.Should().Be("John");
    }

    [Fact]
    public async Task GetSubscriptionById_NonExistent_ShouldReturnNotFound()
    {
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetByIdWithDetailsAsync(Arg.Any<Guid>()).Returns((Subscription?)null);
        var handler = new GetSubscriptionByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetSubscriptionByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetActiveSubscriptionsByCustomer_ShouldReturnList()
    {
        var customerId = Guid.NewGuid();
        var subscriptions = new List<Subscription>
        {
            CreateTestSubscription(customerId),
            CreateTestSubscription(customerId)
        };
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetActiveByCustomerAsync(customerId).Returns(subscriptions);
        var handler = new GetActiveSubscriptionsByCustomerQueryHandler(repository);

        var result = await handler.Handle(
            new GetActiveSubscriptionsByCustomerQuery(customerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnFiltered()
    {
        var subscriptions = new List<Subscription> { CreateTestSubscription() };
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetFilteredAsync(null, null, null, null, 1, 20)
            .Returns(subscriptions);
        var handler = new GetSubscriptionsQueryHandler(repository);

        var result = await handler.Handle(
            new GetSubscriptionsQuery(null, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckEntitlementAvailability_ShouldReturnResult()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(), EntitlementType.Bandwidth, "Bandwidth", 1000, 100, "GB", false, true, DateTime.UtcNow);
        var repository = Substitute.For<ISubscriptionEntitlementRepository>();
        repository.GetBySubscriptionAndTypeAsync(Arg.Any<Guid>(), EntitlementType.Bandwidth)
            .Returns(entitlement);
        var handler = new CheckEntitlementAvailabilityQueryHandler(repository);

        var result = await handler.Handle(
            new CheckEntitlementAvailabilityQuery(Guid.NewGuid(), "Bandwidth", 200),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    private static Subscription CreateTestSubscription(Guid? customerId = null)
    {
        var sub = Subscription.Create(
            "tenant-1", customerId ?? Guid.NewGuid(), "John", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "Basic", BillingPeriod.Monthly,
            "USD", 99.99m, 1, DateTime.UtcNow.AddDays(-30));
        sub.Activate();
        sub.ClearDomainEvents();
        return sub;
    }
}
