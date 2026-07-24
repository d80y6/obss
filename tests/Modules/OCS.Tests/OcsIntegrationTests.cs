using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.OCS.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Obss.OCS.Tests;

public abstract class OcsIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private string _connectionString = string.Empty;

    protected OcsIntegrationTests()
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
        _connectionString = $"{_container.GetConnectionString()};Include Error Detail=true";
        using var ctx = CreateDbContext();
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    protected OcsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OcsDbContext>()
            .UseNpgsql(_connectionString, npgsqlBuilder =>
            {
                npgsqlBuilder.CommandTimeout(60);
            })
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns("test-tenant");
        var domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();

        var context = new OcsDbContext(options, currentTenant, domainEventDispatcher);
        return context;
    }

    protected static IUnitOfWork CreateUnitOfWork(OcsDbContext context)
    {
        var services = new ServiceCollection();
        services.AddScoped<EfDbContext>(_ => context);
        var serviceProvider = services.BuildServiceProvider();
        return new UnitOfWork(serviceProvider);
    }
}
