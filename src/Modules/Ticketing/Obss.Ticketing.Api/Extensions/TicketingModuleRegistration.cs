using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Ticketing.Api.Endpoints;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.BackgroundJobs;
using Obss.Ticketing.Application.Mappings;
using Obss.Ticketing.Infrastructure.Persistence;
using Obss.Ticketing.Infrastructure.Persistence.Repositories;

namespace Obss.Ticketing.Api.Extensions;

public static class TicketingModuleRegistration
{
    public static IServiceCollection AddTicketingModule(this IServiceCollection services)
    {
        services.AddScoped<ISlaDefinitionRepository, SlaDefinitionRepository>();
        services.AddHostedService<SlaBreachCheckJob>();

        TicketMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapTicketingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/ticketing")
            .WithTags("Ticketing");

        TicketEndpoints.Map(group);

        return app;
    }
}
