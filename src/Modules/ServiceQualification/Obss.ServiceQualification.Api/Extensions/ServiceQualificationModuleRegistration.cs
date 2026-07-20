using Obss.ServiceQualification.Api.Endpoints;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Domain.Abstractions;
using Obss.ServiceQualification.Application.Mappings;
using Obss.ServiceQualification.Application.Services;
using Obss.ServiceQualification.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceQualification.Api.Extensions;

public static class ServiceQualificationModuleRegistration
{
    public static IServiceCollection AddServiceQualificationModule(this IServiceCollection services)
    {
        services.AddScoped<IServiceQualificationRepository, ServiceQualificationRepository>();
        services.AddScoped<ICoverageAreaRepository, CoverageAreaRepository>();
        services.AddScoped<IServiceQualificationEngine, CoverageBasedQualificationEngine>();
        services.AddScoped(typeof(Obss.SharedKernel.Application.Abstractions.IRepository<>), typeof(EfRepository<>));

        ServiceQualificationMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapServiceQualificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/service-qualification")
            .WithTags("ServiceQualification");

        ServiceQualificationEndpoints.Map(group);

        return app;
    }
}
