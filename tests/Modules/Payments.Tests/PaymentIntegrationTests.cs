using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Payments.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Obss.Payments.Tests;

public abstract class PaymentIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    protected PaymentIntegrationTests() => _container = new PostgreSqlBuilder().WithImage("postgres:16-alpine").WithDatabase("obss_test").WithUsername("test").WithPassword("test123").Build();
    public async Task InitializeAsync() => await _container.StartAsync();
    public async Task DisposeAsync() => await _container.DisposeAsync();
    protected string ConnectionString => _container.GetConnectionString();
    protected PaymentDbContext CreateDbContext()
    {
        var opts = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql($"{ConnectionString};Include Error Detail=true").Options;
        var ct = Substitute.For<ICurrentTenant>(); ct.TenantId.Returns("t");
        var ctx = new PaymentDbContext(opts, ct, Substitute.For<IDomainEventDispatcher>());
        ctx.Database.EnsureCreated(); return ctx;
    }
    protected static IUnitOfWork CreateUnitOfWork(PaymentDbContext c)
    {
        var s = new ServiceCollection(); s.AddScoped<EfDbContext>(_ => c);
        return new UnitOfWork(s.BuildServiceProvider());
    }
}
