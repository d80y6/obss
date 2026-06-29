using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.ApiGateway.Api.Endpoints;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Application.Mappings;
using Obss.ApiGateway.Domain.Services;
using Obss.ApiGateway.Infrastructure.Persistence;
using Obss.ApiGateway.Infrastructure.Persistence.Repositories;
using Obss.ApiGateway.Infrastructure.Services;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ApiGateway.Api.Extensions;

public static class GatewayModuleRegistration
{
    public static IServiceCollection AddApiGatewayModule(this IServiceCollection services)
    {
        services.AddDbContext<GatewayDbContext>(options =>
        {
        });

        services.AddScoped<IApiRouteRepository, ApiRouteRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        // IUnitOfWork is registered globally in Program.cs via TryAddScoped

        services.AddSingleton<IRateLimiter, RateLimiter>();
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

        GatewayMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapApiGatewayEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/gateway")
            .WithTags("ApiGateway");

        GatewayEndpoints.Map(group);

        return app;
    }
}
