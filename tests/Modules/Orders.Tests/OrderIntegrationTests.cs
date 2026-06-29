using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Orders.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Obss.Orders.Tests;

public abstract class OrderIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    protected OrderIntegrationTests()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("obss_test")
            .WithUsername("test")
            .WithPassword("test123")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    protected string ConnectionString => _container.GetConnectionString();

    protected OrderDbContext CreateDbContext()
    {
        var connectionString = $"{ConnectionString};Include Error Detail=true";
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseNpgsql(connectionString, npgsqlBuilder =>
            {
                npgsqlBuilder.CommandTimeout(60);
            })
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        var currentTenant = Substitute.For<ICurrentTenant>();
        var domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();

        var context = new OrderDbContext(options, currentTenant, domainEventDispatcher);
        context.Database.EnsureCreated();
        return context;
    }

    protected static IUnitOfWork CreateUnitOfWork(OrderDbContext context)
    {
        var services = new ServiceCollection();
        services.AddScoped<EfDbContext>(_ => context);
        var serviceProvider = services.BuildServiceProvider();
        return new UnitOfWork(serviceProvider);
    }
}
