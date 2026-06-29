using Xunit;
using FluentAssertions;
using Obss.NumberInventory.Domain.Entities;
using Obss.NumberInventory.Domain.Events;
using Obss.NumberInventory.Domain.Exceptions;
using Obss.NumberInventory.Domain.ValueObjects;

namespace Obss.NumberInventory.Tests.Domain;

public class TelephoneNumberTests
{
    private static TelephoneNumber CreateAvailableNumber()
    {
        return TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
    }

    [Fact]
    public void Create_ShouldReturnNumberWithAvailableStatus()
    {
        var number = CreateAvailableNumber();

        number.Status.Should().Be(NumberStatus.Available);
        number.CustomerId.Should().BeNull();
        number.SubscriptionId.Should().BeNull();
        number.AssignedAt.Should().BeNull();
        number.ReservedAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        var number = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.TollFree, 25.0m, "EUR", "Test notes");

        number.TenantId.Should().Be("tenant-1");
        number.Number.Should().Be("+1234567890");
        number.NumberType.Should().Be(NumberType.TollFree);
        number.Cost.Should().Be(25.0m);
        number.Currency.Should().Be("EUR");
        number.Notes.Should().Be("Test notes");
        number.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldAllowNullNotes()
    {
        var number = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");

        number.Notes.Should().BeNull();
    }

    [Fact]
    public void Reserve_ShouldChangeStatusToReserved()
    {
        var number = CreateAvailableNumber();

        number.Reserve();

        number.Status.Should().Be(NumberStatus.Reserved);
        number.ReservedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reserve_WhenNotAvailable_ShouldThrow()
    {
        var number = CreateAvailableNumber();
        number.Reserve();

        var act = () => number.Reserve();

        act.Should().Throw<InvalidNumberStateException>()
            .WithMessage("*Reserved*");
    }

    [Fact]
    public void Assign_FromAvailable_ShouldChangeStatusToAssigned()
    {
        var number = CreateAvailableNumber();
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        number.Assign(customerId, subscriptionId);

        number.Status.Should().Be(NumberStatus.Assigned);
        number.CustomerId.Should().Be(customerId);
        number.SubscriptionId.Should().Be(subscriptionId);
        number.AssignedAt.Should().NotBeNull();
    }

    [Fact]
    public void Assign_FromReserved_ShouldChangeStatusToAssigned()
    {
        var number = CreateAvailableNumber();
        number.Reserve();
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        number.Assign(customerId, subscriptionId);

        number.Status.Should().Be(NumberStatus.Assigned);
        number.CustomerId.Should().Be(customerId);
        number.SubscriptionId.Should().Be(subscriptionId);
    }

    [Fact]
    public void Assign_WhenNotAvailableOrReserved_ShouldThrow()
    {
        var number = CreateAvailableNumber();
        number.Reserve();
        number.Assign(Guid.NewGuid(), Guid.NewGuid());

        var act = () => number.Assign(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<InvalidNumberStateException>()
            .WithMessage("*assign*Assigned*");
    }

    [Fact]
    public void Release_FromReserved_ShouldChangeStatusToAvailable()
    {
        var number = CreateAvailableNumber();
        number.Reserve();

        number.Release();

        number.Status.Should().Be(NumberStatus.Available);
        number.ReservedAt.Should().BeNull();
    }

    [Fact]
    public void Release_FromAssigned_ShouldChangeStatusToAvailable()
    {
        var number = CreateAvailableNumber();
        number.Assign(Guid.NewGuid(), Guid.NewGuid());

        number.Release();

        number.Status.Should().Be(NumberStatus.Available);
        number.CustomerId.Should().BeNull();
        number.SubscriptionId.Should().BeNull();
        number.AssignedAt.Should().BeNull();
    }

    [Fact]
    public void Release_WhenNotReservedOrAssigned_ShouldThrow()
    {
        var number = CreateAvailableNumber();

        var act = () => number.Release();

        act.Should().Throw<InvalidNumberStateException>()
            .WithMessage("*release*Available*");
    }

    [Fact]
    public void Suspend_FromAssigned_ShouldChangeStatusToSuspended()
    {
        var number = CreateAvailableNumber();
        number.Assign(Guid.NewGuid(), Guid.NewGuid());

        number.Suspend();

        number.Status.Should().Be(NumberStatus.Suspended);
        number.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Suspend_FromPorted_ShouldChangeStatusToSuspended()
    {
        var number = CreateAvailableNumber();
        number.PortIn();

        number.Suspend();

        number.Status.Should().Be(NumberStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenNotAssignedOrPorted_ShouldThrow()
    {
        var number = CreateAvailableNumber();

        var act = () => number.Suspend();

        act.Should().Throw<InvalidNumberStateException>()
            .WithMessage("*suspend*Available*");
    }

    [Fact]
    public void Disconnect_FromAssigned_ShouldChangeStatusToDisconnected()
    {
        var number = CreateAvailableNumber();
        number.Assign(Guid.NewGuid(), Guid.NewGuid());

        number.Disconnect();

        number.Status.Should().Be(NumberStatus.Disconnected);
        number.CustomerId.Should().BeNull();
        number.SubscriptionId.Should().BeNull();
        number.AssignedAt.Should().BeNull();
    }

    [Fact]
    public void Disconnect_FromSuspended_ShouldChangeStatusToDisconnected()
    {
        var number = CreateAvailableNumber();
        number.Assign(Guid.NewGuid(), Guid.NewGuid());
        number.Suspend();

        number.Disconnect();

        number.Status.Should().Be(NumberStatus.Disconnected);
    }

    [Fact]
    public void Disconnect_FromPorted_ShouldChangeStatusToDisconnected()
    {
        var number = CreateAvailableNumber();
        number.PortIn();

        number.Disconnect();

        number.Status.Should().Be(NumberStatus.Disconnected);
    }

    [Fact]
    public void Disconnect_WhenNotAssignedSuspendedOrPorted_ShouldThrow()
    {
        var number = CreateAvailableNumber();

        var act = () => number.Disconnect();

        act.Should().Throw<InvalidNumberStateException>()
            .WithMessage("*disconnect*Available*");
    }

    [Fact]
    public void PortIn_ShouldChangeStatusToPorted()
    {
        var number = CreateAvailableNumber();
        var customerId = Guid.NewGuid();

        number.PortIn(customerId);

        number.Status.Should().Be(NumberStatus.Ported);
        number.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void PortIn_WithoutCustomer_ShouldSetCustomerIdNull()
    {
        var number = CreateAvailableNumber();

        number.PortIn();

        number.Status.Should().Be(NumberStatus.Ported);
        number.CustomerId.Should().BeNull();
    }

    [Fact]
    public void PortIn_WhenNotAvailable_ShouldThrow()
    {
        var number = CreateAvailableNumber();
        number.Reserve();

        var act = () => number.PortIn();

        act.Should().Throw<InvalidNumberStateException>()
            .WithMessage("*port in*Reserved*");
    }

    [Fact]
    public void PortOut_FromAssigned_ShouldChangeStatusToAvailable()
    {
        var number = CreateAvailableNumber();
        number.Assign(Guid.NewGuid(), Guid.NewGuid());

        number.PortOut();

        number.Status.Should().Be(NumberStatus.Available);
        number.CustomerId.Should().BeNull();
        number.SubscriptionId.Should().BeNull();
        number.AssignedAt.Should().BeNull();
    }

    [Fact]
    public void PortOut_FromPorted_ShouldChangeStatusToAvailable()
    {
        var number = CreateAvailableNumber();
        number.PortIn();

        number.PortOut();

        number.Status.Should().Be(NumberStatus.Available);
    }

    [Fact]
    public void PortOut_WhenNotAssignedOrPorted_ShouldThrow()
    {
        var number = CreateAvailableNumber();

        var act = () => number.PortOut();

        act.Should().Throw<InvalidNumberStateException>()
            .WithMessage("*port out*Available*");
    }

    [Fact]
    public void Assign_ShouldRaiseNumberAssignedDomainEvent()
    {
        var number = CreateAvailableNumber();
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        number.ClearDomainEvents();

        number.Assign(customerId, subscriptionId);

        number.DomainEvents.Should().ContainSingle(e => e is NumberAssignedDomainEvent);
        var domainEvent = number.DomainEvents.OfType<NumberAssignedDomainEvent>().Single();
        domainEvent.NumberId.Should().Be(number.Id);
        domainEvent.Number.Should().Be(number.Number);
        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.SubscriptionId.Should().Be(subscriptionId);
    }

    [Fact]
    public void PortIn_ShouldRaiseNumberPortedDomainEvent()
    {
        var number = CreateAvailableNumber();
        var customerId = Guid.NewGuid();
        number.ClearDomainEvents();

        number.PortIn(customerId);

        number.DomainEvents.Should().ContainSingle(e => e is NumberPortedDomainEvent);
        var domainEvent = number.DomainEvents.OfType<NumberPortedDomainEvent>().Single();
        domainEvent.NumberId.Should().Be(number.Id);
        domainEvent.Number.Should().Be(number.Number);
        domainEvent.Direction.Should().Be("In");
        domainEvent.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void PortOut_ShouldRaiseNumberPortedDomainEvent()
    {
        var number = CreateAvailableNumber();
        number.Assign(Guid.NewGuid(), Guid.NewGuid());
        number.ClearDomainEvents();

        number.PortOut();

        number.DomainEvents.Should().ContainSingle(e => e is NumberPortedDomainEvent);
        var domainEvent = number.DomainEvents.OfType<NumberPortedDomainEvent>().Single();
        domainEvent.NumberId.Should().Be(number.Id);
        domainEvent.Number.Should().Be(number.Number);
        domainEvent.Direction.Should().Be("Out");
        domainEvent.CustomerId.Should().NotBeNull();
    }

    [Fact]
    public void FullLifecycle_AvailableToDisconnected_ShouldFollowValidTransitions()
    {
        var number = CreateAvailableNumber();
        number.Status.Should().Be(NumberStatus.Available);

        number.Reserve();
        number.Status.Should().Be(NumberStatus.Reserved);

        number.Assign(Guid.NewGuid(), Guid.NewGuid());
        number.Status.Should().Be(NumberStatus.Assigned);

        number.Suspend();
        number.Status.Should().Be(NumberStatus.Suspended);

        number.Disconnect();
        number.Status.Should().Be(NumberStatus.Disconnected);
    }

    [Fact]
    public void ReserveThenReleaseThenReserve_ShouldAllowMultipleCycles()
    {
        var number = CreateAvailableNumber();

        number.Reserve();
        number.Release();
        number.Reserve();
        number.Release();

        number.Status.Should().Be(NumberStatus.Available);
    }
}
