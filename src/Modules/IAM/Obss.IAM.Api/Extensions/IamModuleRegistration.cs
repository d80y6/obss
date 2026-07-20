using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Obss.IAM.Api.Endpoints;
using Obss.IAM.Application.Mappings;
using Obss.IAM.Infrastructure.Persistence;
using Obss.IAM.Infrastructure.Services;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Api.Extensions;

public static class IamModuleRegistration
{
    public static IServiceCollection AddIamModule(this IServiceCollection services)
    {
        IamMappingConfig.Configure();

        services.RemoveAll<ITenantStore>();
        services.AddScoped<ITenantStore, IamTenantStore>();

        return services;
    }

    public static IEndpointRouteBuilder MapIamEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/iam")
            .WithTags("IAM");

        UserEndpoints.Map(group);
        TenantEndpoints.Map(group);
        RoleEndpoints.Map(group);
        PartyRoleEndpoints.Map(group);

        return app;
    }
}
