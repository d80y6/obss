using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.IAM.Api.Endpoints;
using Obss.IAM.Application.Mappings;
using Obss.IAM.Infrastructure.Persistence;

namespace Obss.IAM.Api.Extensions;

public static class IamModuleRegistration
{
    public static IServiceCollection AddIamModule(this IServiceCollection services)
    {
        IamMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapIamEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/iam")
            .WithTags("IAM");

        UserEndpoints.Map(group);
        TenantEndpoints.Map(group);
        RoleEndpoints.Map(group);

        return app;
    }
}