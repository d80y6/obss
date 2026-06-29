using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Obss.Subscriptions.Tests;

public abstract class SubscriptionIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    protected SubscriptionIntegrationTests()
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

    protected SubscriptionDbContext CreateDbContext()
    {
        var connectionString = $"{ConnectionString};Include Error Detail=true";
        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
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

        var context = new SubscriptionDbContext(options, currentTenant, domainEventDispatcher);
        context.Database.EnsureCreated();
        return context;
    }

    protected static IUnitOfWork CreateUnitOfWork(SubscriptionDbContext context)
    {
        var services = new ServiceCollection();
        services.AddScoped<EfDbContext>(_ => context);
        var serviceProvider = services.BuildServiceProvider();
        return new UnitOfWork(serviceProvider);
    }
}
