using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.Mappings;
using Obss.OCS.Infrastructure.Persistence;
using Obss.OCS.Infrastructure.Persistence.Repositories;

namespace Obss.OCS.Api.Extensions;

public static class OcsModuleRegistration
{
    public static IServiceCollection AddOcsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

        services.AddDbContext<OcsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "ocs"))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ICreditPoolRepository, CreditPoolRepository>();
        services.AddScoped<IOcsTransactionRepository, OcsTransactionRepository>();

        OcsMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapOcsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/ocs").WithTags("OCS");
        OcsEndpoints.Map(group);
        return app;
    }
}
