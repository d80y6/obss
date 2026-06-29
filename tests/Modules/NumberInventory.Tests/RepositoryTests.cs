using Xunit;
using FluentAssertions;
using Obss.NumberInventory.Domain.Entities;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.NumberInventory.Infrastructure.Persistence.Repositories;

namespace Obss.NumberInventory.Tests;

public class RepositoryTests : NumberInventoryIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveTelephoneNumber()
    {
        using var context = CreateDbContext();
        var repository = new TelephoneNumberRepository(context);

        var number = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");

        await repository.AddAsync(number);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(number.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Number.Should().Be("+1234567890");
        retrieved.NumberType.Should().Be(NumberType.Mobile);
        retrieved.Status.Should().Be(NumberStatus.Available);
        retrieved.Cost.Should().Be(10.0m);
        retrieved.Currency.Should().Be("USD");
        retrieved.TenantId.Should().Be("tenant-1");
    }

    [Fact]
    public async Task CanGetAvailableNumbersByType()
    {
        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);

            var mobile1 = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
            var mobile2 = TelephoneNumber.Create("tenant-1", "+1234567891", NumberType.Mobile, 10.0m, "USD");
            var tollFree = TelephoneNumber.Create("tenant-1", "+18005551234", NumberType.TollFree, 0m, "USD");

            await repo.AddAsync(mobile1);
            await repo.AddAsync(mobile2);
            await repo.AddAsync(tollFree);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);
            var mobiles = await repo.GetAvailableNumbersAsync(NumberType.Mobile, null);

            mobiles.Should().HaveCount(2);
            mobiles.Should().Contain(n => n.Number == "+1234567890");
            mobiles.Should().Contain(n => n.Number == "+1234567891");
            mobiles.Should().NotContain(n => n.Number == "+18005551234");
        }
    }

    [Fact]
    public async Task CanGetAvailableNumbersByPrefix()
    {
        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);

            var num1 = TelephoneNumber.Create("tenant-1", "+44123456789", NumberType.Geographic, 5.0m, "GBP");
            var num2 = TelephoneNumber.Create("tenant-1", "+44123456790", NumberType.Geographic, 5.0m, "GBP");
            var num3 = TelephoneNumber.Create("tenant-1", "+44987654321", NumberType.Geographic, 5.0m, "GBP");

            await repo.AddAsync(num1);
            await repo.AddAsync(num2);
            await repo.AddAsync(num3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);
            var results = await repo.GetAvailableNumbersAsync(null, "+441234");

            results.Should().HaveCount(2);
            results.Should().Contain(n => n.Number == "+44123456789");
            results.Should().Contain(n => n.Number == "+44123456790");
            results.Should().NotContain(n => n.Number == "+44987654321");
        }
    }

    [Fact]
    public async Task CanGetByCustomer()
    {
        var customerId = Guid.NewGuid();

        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);

            var num1 = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
            num1.Assign(customerId, Guid.NewGuid());
            var num2 = TelephoneNumber.Create("tenant-1", "+1234567891", NumberType.Mobile, 10.0m, "USD");
            num2.Assign(customerId, Guid.NewGuid());
            var num3 = TelephoneNumber.Create("tenant-1", "+1234567892", NumberType.Mobile, 10.0m, "USD");
            num3.Assign(Guid.NewGuid(), Guid.NewGuid());

            await repo.AddAsync(num1);
            await repo.AddAsync(num2);
            await repo.AddAsync(num3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);
            var results = await repo.GetByCustomerAsync(customerId);

            results.Should().HaveCount(2);
            results.Should().Contain(n => n.Number == "+1234567890");
            results.Should().Contain(n => n.Number == "+1234567891");
            results.Should().NotContain(n => n.Number == "+1234567892");
        }
    }

    [Fact]
    public async Task CanGetByNumber()
    {
        using var context = CreateDbContext();
        var repo = new TelephoneNumberRepository(context);

        var number = TelephoneNumber.Create("tenant-1", "+15551234567", NumberType.TollFree, 0m, "USD");
        await repo.AddAsync(number);
        await context.SaveChangesAsync();

        var found = await repo.GetByNumberAsync("+15551234567");
        found.Should().NotBeNull();
        found!.Number.Should().Be("+15551234567");

        var notFound = await repo.GetByNumberAsync("+15559999999");
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task CanSearchNumbers()
    {
        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);

            var num1 = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
            var num2 = TelephoneNumber.Create("tenant-1", "+1234567891", NumberType.Mobile, 10.0m, "USD");
            var num3 = TelephoneNumber.Create("tenant-1", "+18005551234", NumberType.TollFree, 0m, "USD");

            await repo.AddAsync(num1);
            await repo.AddAsync(num2);
            await repo.AddAsync(num3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);
            var results = await repo.SearchNumbersAsync(null, null, null, 1, 10);

            results.Should().HaveCount(3);
        }
    }

    [Fact]
    public async Task CanSearchNumbersWithFilters()
    {
        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);

            var num1 = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
            num1.Assign(Guid.NewGuid(), Guid.NewGuid());
            var num2 = TelephoneNumber.Create("tenant-1", "+1234567891", NumberType.Mobile, 10.0m, "USD");
            var num3 = TelephoneNumber.Create("tenant-1", "+18005551234", NumberType.TollFree, 0m, "USD");

            await repo.AddAsync(num1);
            await repo.AddAsync(num2);
            await repo.AddAsync(num3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new TelephoneNumberRepository(context);

            var assignedResults = await repo.SearchNumbersAsync(null, NumberStatus.Assigned, null, 1, 10);
            assignedResults.Should().HaveCount(1);

            var mobileResults = await repo.SearchNumbersAsync(null, null, NumberType.Mobile, 1, 10);
            mobileResults.Should().HaveCount(2);

            var prefixResults = await repo.SearchNumbersAsync("+1800", null, null, 1, 10);
            prefixResults.Should().HaveCount(1);
            prefixResults[0].Number.Should().Be("+18005551234");
        }
    }

    [Fact]
    public async Task CanUpdateTelephoneNumber()
    {
        using var context = CreateDbContext();
        var repo = new TelephoneNumberRepository(context);

        var number = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
        await repo.AddAsync(number);
        await context.SaveChangesAsync();

        number.Assign(Guid.NewGuid(), Guid.NewGuid());
        await context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(number.Id);
        retrieved!.Status.Should().Be(NumberStatus.Assigned);
        retrieved.CustomerId.Should().NotBeNull();
        retrieved.AssignedAt.Should().NotBeNull();
    }
}
