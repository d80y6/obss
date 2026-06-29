using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.CRM.Api.Endpoints;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.BackgroundJobs;
using Obss.CRM.Application.Mappings;
using Obss.CRM.Infrastructure.Persistence;
using Obss.CRM.Infrastructure.Persistence.Repositories;

namespace Obss.CRM.Api.Extensions;

public static class CrmModuleRegistration
{
    public static IServiceCollection AddCrmModule(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerSegmentRepository, CustomerSegmentRepository>();

        services.AddHostedService<CustomerSegmentationJob>();

        CrmMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapCrmEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/crm")
            .WithTags("CRM");

        CustomerEndpoints.Map(group);
        LookupEndpoints.Map(group);

        return app;
    }
}
