using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Mappings;
using Obss.AAA.Infrastructure.Persistence;
using Obss.AAA.Infrastructure.Persistence.Repositories;
using Obss.AAA.Infrastructure.Services.Adapters;

namespace Obss.AAA.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAaaModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

        services.AddDbContext<AaaDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "aaa"))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<INasRepository, NasRepository>();
        services.AddScoped<IRadiusSessionRepository, RadiusSessionRepository>();

        services.AddScoped<IAaaAdapter, RadiusAdapterStub>();
        services.AddScoped<IAaaAdapter, DiameterAdapterStub>();
        services.AddScoped<IAaaAdapter, TacacsPlusAdapterStub>();

        AaaMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapAaaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/aaa")
            .WithTags("AAA");

        NasEndpoints.Map(group);
        SessionEndpoints.Map(group);

        return app;
    }
}
