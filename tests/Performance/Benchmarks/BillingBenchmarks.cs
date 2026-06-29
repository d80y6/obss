using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;

namespace Obss.Performance.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class BillingBenchmarks
{
    private Bill _bill = null!;
    private const string TenantId = "tenant-perf-001";
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid BillId = Guid.NewGuid();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _bill = CreateDraftBill();
        SeedBillWithLines(_bill, 10);
    }

    [Benchmark]
    public Bill CreateBill()
    {
        return Bill.Create(
            TenantId,
            CustomerId,
            "Performance Customer",
            BillingPeriod.Monthly,
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30),
            new DateTime(2026, 7, 15),
            "USD");
    }

    [Benchmark]
    public Bill GenerateBillWithLines()
    {
        var bill = CreateDraftBill();
        for (var i = 0; i < 5; i++)
        {
            var line = BillLine.CreateRecurring(
                bill.Id,
                $"Subscription Line {i}",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                99.99m,
                0,
                0.05m,
                "USD",
                DateTime.UtcNow);
            bill.AddLine(line);
        }
        return bill;
    }

    [Benchmark]
    public Bill CalculateTotals()
    {
        var bill = CreateDraftBill();
        SeedBillWithLines(bill, 8);
        bill.CalculateTotals();
        return bill;
    }

    [Benchmark]
    public Bill RecalculateWithTax()
    {
        var bill = CreateDraftBill();
        SeedBillWithLines(bill, 5);

        for (var i = 0; i < 3; i++)
        {
            var taxLine = BillLine.CreateTaxLine(
                bill.Id,
                $"Tax Line {i}",
                15.00m + i,
                0.05m,
                "USD",
                DateTime.UtcNow);
            bill.AddTaxLine(taxLine);
        }

        bill.RecalculateTotalsWithTax();
        return bill;
    }

    [Benchmark]
    public Bill FullBillingLifecycle()
    {
        var bill = CreateDraftBill();
        SeedBillWithLines(bill, 5);

        var taxLine = BillLine.CreateTaxLine(
            bill.Id,
            "VAT 5%",
            25.00m,
            0.05m,
            "USD",
            DateTime.UtcNow);
        bill.AddTaxLine(taxLine);

        bill.CalculateTotals();
        bill.MarkAsFinalized();
        return bill;
    }

    [Benchmark]
    public Bill CreateBillWithManyLines()
    {
        var bill = CreateDraftBill();
        SeedBillWithLines(bill, 50);
        bill.CalculateTotals();
        return bill;
    }

    [Benchmark]
    public Bill CalculateAndFinalize()
    {
        var bill = CreateDraftBill();
        SeedBillWithLines(bill, 5);
        bill.CalculateTotals();
        bill.MarkAsFinalized();
        return bill;
    }

    private static Bill CreateDraftBill()
    {
        return Bill.Create(
            TenantId,
            CustomerId,
            "Performance Customer",
            BillingPeriod.Monthly,
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30),
            new DateTime(2026, 7, 15),
            "USD");
    }

    private static void SeedBillWithLines(Bill bill, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var line = BillLine.CreateRecurring(
                bill.Id,
                $"Subscription-{i}",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                99.99m + i,
                0,
                0.05m,
                "USD",
                new DateTime(2026, 6, 1));
            bill.AddLine(line);
        }
    }
}
