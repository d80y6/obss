using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Audit.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Obss.Audit.Tests;

public abstract class AuditIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    protected AuditIntegrationTests()
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

    protected AuditDbContext CreateDbContext()
    {
        var connectionString = $"{ConnectionString};Include Error Detail=true";
        var options = new DbContextOptionsBuilder<AuditDbContext>()
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

        var context = new AuditDbContext(options, currentTenant, domainEventDispatcher);
        context.Database.EnsureCreated();
        return context;
    }

    protected static IUnitOfWork CreateUnitOfWork(AuditDbContext context)
    {
        var services = new ServiceCollection();
        services.AddScoped<EfDbContext>(_ => context);
        var serviceProvider = services.BuildServiceProvider();
        return new UnitOfWork(serviceProvider);
    }
}
