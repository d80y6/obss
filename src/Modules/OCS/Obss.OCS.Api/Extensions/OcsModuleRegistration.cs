using Microsoft.Extensions.DependencyInjection;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.Mappings;
using Obss.OCS.Infrastructure.BackgroundJobs;
using Obss.OCS.Infrastructure.Persistence.Repositories;

namespace Obss.OCS.Api.Extensions;

public static class OcsModuleRegistration
{
    public static IServiceCollection AddOcsModule(this IServiceCollection services)
    {
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ICreditPoolRepository, CreditPoolRepository>();
        services.AddScoped<IOcsTransactionRepository, OcsTransactionRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        services.AddHostedService<ReservationExpiryJob>();

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
