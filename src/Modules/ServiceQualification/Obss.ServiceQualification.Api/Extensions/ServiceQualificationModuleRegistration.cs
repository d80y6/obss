using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.ServiceQualification.Api.Endpoints;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceQualification.Api.Extensions;

public static class ServiceQualificationModuleRegistration
{
    public static IServiceCollection AddServiceQualificationModule(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        return services;
    }

    public static IEndpointRouteBuilder MapServiceQualificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/service-qualification")
            .WithTags("Service Qualification");

        ServiceQualificationEndpoints.Map(group);

        return app;
    }
}
