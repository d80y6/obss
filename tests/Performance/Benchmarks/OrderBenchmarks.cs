using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Performance.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class OrderBenchmarks
{
    private Order _order = null!;
    private const string TenantId = "tenant-perf-001";
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid OfferId = Guid.NewGuid();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _order = CreateDraftOrder();
        SeedOrderWithItems(_order, 5);
    }

    [Benchmark]
    public Order CreateOrder()
    {
        return Order.Create(
            TenantId,
            CustomerId,
            "Performance Customer",
            OrderType.New,
            Guid.NewGuid().ToString());
    }

    [Benchmark]
    public Order AddItemToOrder()
    {
        var order = CreateDraftOrder();
        order.AddItem(
            ProductId,
            OfferId,
            "Internet 100Mbps",
            "Fiber Premium",
            1,
            99.99m,
            0,
            0,
            9.99m,
            Obss.Orders.Domain.ValueObjects.BillingPeriod.Monthly);
        return order;
    }

    [Benchmark]
    public Order SubmitOrder()
    {
        var order = CreateDraftOrder();
        SeedOrderWithItems(order, 3);
        order.Submit();
        return order;
    }

    [Benchmark]
    public Order ApproveOrder()
    {
        var order = CreateDraftOrder();
        SeedOrderWithItems(order, 3);
        order.Submit();
        order.Approve(Guid.NewGuid().ToString());
        return order;
    }

    [Benchmark]
    public Order FullLifecycle()
    {
        var order = CreateDraftOrder();
        SeedOrderWithItems(order, 3);
        order.Submit();
        order.Approve(Guid.NewGuid().ToString());
        order.StartFulfillment();
        order.MarkCompleted();
        return order;
    }

    [Benchmark]
    public Order AddRemoveItems()
    {
        var order = CreateDraftOrder();
        for (var i = 0; i < 10; i++)
        {
            order.AddItem(
                Guid.NewGuid(),
                Guid.NewGuid(),
                $"Product-{i}",
                $"Offer-{i}",
                1,
                50m + i,
                0,
                0,
                5m,
                Obss.Orders.Domain.ValueObjects.BillingPeriod.Monthly);
        }
        var items = order.Items.ToList();
        for (var i = 0; i < 5; i++)
        {
            order.RemoveItem(items[i].Id);
        }
        order.CalculateTotals();
        return order;
    }

    [Benchmark]
    public Order CancelOrder()
    {
        var order = CreateDraftOrder();
        SeedOrderWithItems(order, 2);
        order.Submit();
        order.Cancel("Customer requested cancellation");
        return order;
    }

    private static Order CreateDraftOrder()
    {
        return Order.Create(
            TenantId,
            CustomerId,
            "Performance Customer",
            OrderType.New,
            Guid.NewGuid().ToString());
    }

    private static void SeedOrderWithItems(Order order, int count)
    {
        for (var i = 0; i < count; i++)
        {
            order.AddItem(
                Guid.NewGuid(),
                Guid.NewGuid(),
                $"Product-{i}",
                $"Offer-{i}",
                1,
                99.99m + i,
                0,
                0,
                9.99m,
                Obss.Orders.Domain.ValueObjects.BillingPeriod.Monthly);
        }
    }
}
