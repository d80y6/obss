using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Services;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public static class EfDatabaseConfiguration
{
    public static IServiceCollection AddEntityFramework<TContext>(
        this IServiceCollection services,
        string connectionString,
        string? migrationAssembly = null,
        int maxRetryCount = 3,
        TimeSpan? maxRetryDelay = null)
        where TContext : EfDbContext
    {
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlBuilder =>
            {
                if (!string.IsNullOrEmpty(migrationAssembly))
                {
                    npgsqlBuilder.MigrationsAssembly(migrationAssembly);
                }

                npgsqlBuilder.EnableRetryOnFailure(
                    maxRetryCount: maxRetryCount,
                    maxRetryDelay: maxRetryDelay ?? TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);

                npgsqlBuilder.CommandTimeout(60);
                npgsqlBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            options.UseSnakeCaseNamingConvention();

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }

    public static IServiceCollection AddOutboxProcessing(
        this IServiceCollection services,
        TimeSpan? pollingInterval = null)
    {
        services.AddHostedService<OutboxProcessor>();
        return services;
    }

    public static IServiceCollection AddRabbitMqConsumer(this IServiceCollection services)
    {
        services.AddHostedService<RabbitMqConsumerService>();
        return services;
    }
}
