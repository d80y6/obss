using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.ModuleTemplate.Api.Endpoints;
using Obss.ModuleTemplate.Application.Mappings;
using Obss.ModuleTemplate.Infrastructure.Persistence;

namespace Obss.ModuleTemplate.Api.Extensions;

public static class ModuleTemplateRegistration
{
    public static IServiceCollection AddModuleTemplateModule(this IServiceCollection services)
    {
        services.AddScoped<SampleDbContext>();
        SampleMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapModuleTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/samples")
            .WithTags("Samples");

        SampleEndpoints.Map(group);

        return app;
    }
}
