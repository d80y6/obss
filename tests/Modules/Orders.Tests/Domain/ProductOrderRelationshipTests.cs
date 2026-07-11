using Xunit;
using FluentAssertions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.Orders.Domain.Exceptions;

namespace Obss.Orders.Tests.Domain;

public class ProductOrderRelationshipTests
{
    private static ProductOrder CreateOrderWithTwoItems(out ProductOrderItem item1, out ProductOrderItem item2)
    {
        var order = ProductOrder.Create("tenant1", Guid.NewGuid(), "Test", OrderType.New, "user1", null, null, null, "USD");
        var pid1 = Guid.NewGuid();
        var pid2 = Guid.NewGuid();
        order.AddItem(pid1, Guid.NewGuid(), "P1", "O1", 1, 100, 0, 0, 0, BillingPeriod.Monthly);
        order.AddItem(pid2, Guid.NewGuid(), "P2", "O2", 2, 200, 0, 0, 0, BillingPeriod.Monthly);
        item1 = order.Items.ElementAt(0);
        item2 = order.Items.ElementAt(1);
        return order;
    }

    [Fact]
    public void AddItemRelationship_AddsRelationship()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);
        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);
        order.ItemRelationships.Should().ContainSingle(r => r.ProductOrderItemId == item1.Id && r.TargetItemId == item2.Id);
    }

    [Fact]
    public void AddItemRelationship_CircularDependency_Throws()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);
        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);
        var act = () => order.AddItemRelationship(item2.Id, item1.Id, RelationshipType.Requires);
        act.Should().Throw<InvalidProductOrderStateException>();
    }

    [Fact]
    public void RemoveItemRelationship_RemovesRelationship()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);
        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);
        var relId = order.ItemRelationships[0].Id;
        order.RemoveItemRelationship(relId);
        order.ItemRelationships.Should().BeEmpty();
    }

    [Fact]
    public void GetItemRelationships_ReturnsOnlyRelatedItems()
    {
        var order = CreateOrderWithTwoItems(out var item1, out var item2);
        order.AddItemRelationship(item1.Id, item2.Id, RelationshipType.Requires);
        var rels = order.GetItemRelationships(item1.Id);
        rels.Should().ContainSingle(r => r.TargetItemId == item2.Id);
    }
}
