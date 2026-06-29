using Xunit;
using FluentAssertions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Tests.Domain;

public class OrderFulfillmentTests
{
    [Fact]
    public void Create_ShouldReturnPendingFulfillment()
    {
        var orderId = Guid.NewGuid();

        var fulfillment = OrderFulfillment.Create(orderId);

        fulfillment.OrderId.Should().Be(orderId);
        fulfillment.Status.Should().Be(FulfillmentStatus.Pending);
        fulfillment.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        fulfillment.CompletedAt.Should().BeNull();
        fulfillment.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void StartFulfillment_ShouldChangeStatusToInProgress()
    {
        var fulfillment = OrderFulfillment.Create(Guid.NewGuid());
        var workflowId = Guid.NewGuid();

        fulfillment.StartFulfillment(workflowId);

        fulfillment.Status.Should().Be(FulfillmentStatus.InProgress);
        fulfillment.WorkflowInstanceId.Should().Be(workflowId);
    }

    [Fact]
    public void StartFulfillment_WhenNotPending_ShouldThrow()
    {
        var fulfillment = OrderFulfillment.Create(Guid.NewGuid());
        fulfillment.StartFulfillment(Guid.NewGuid());

        var act = () => fulfillment.StartFulfillment(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*start*InProgress*");
    }

    [Fact]
    public void Complete_ShouldChangeStatusToCompleted()
    {
        var fulfillment = OrderFulfillment.Create(Guid.NewGuid());
        fulfillment.StartFulfillment(Guid.NewGuid());

        fulfillment.Complete();

        fulfillment.Status.Should().Be(FulfillmentStatus.Completed);
        fulfillment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenPending_ShouldAutoStartAndComplete()
    {
        var fulfillment = OrderFulfillment.Create(Guid.NewGuid());

        fulfillment.Complete();

        fulfillment.Status.Should().Be(FulfillmentStatus.Completed);
        fulfillment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldChangeStatusToFailed()
    {
        var fulfillment = OrderFulfillment.Create(Guid.NewGuid());
        fulfillment.StartFulfillment(Guid.NewGuid());

        fulfillment.Fail("Network error");

        fulfillment.Status.Should().Be(FulfillmentStatus.Failed);
        fulfillment.CompletedAt.Should().NotBeNull();
        fulfillment.ErrorMessage.Should().Be("Network error");
    }

    [Fact]
    public void Fail_WhenCompleted_ShouldThrow()
    {
        var fulfillment = OrderFulfillment.Create(Guid.NewGuid());
        fulfillment.StartFulfillment(Guid.NewGuid());
        fulfillment.Complete();

        var act = () => fulfillment.Fail("Too late");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*fail a completed*");
    }
}
