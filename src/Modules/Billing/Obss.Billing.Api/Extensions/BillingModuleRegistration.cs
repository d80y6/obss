using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Billing.Api.Endpoints;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.BackgroundJobs;
using Obss.Billing.Application.Mappings;
using Obss.Billing.Domain.Services;
using Obss.Billing.Infrastructure.Persistence;
using Obss.Billing.Infrastructure.Persistence.Repositories;
using Obss.Billing.Infrastructure.Services;

namespace Obss.Billing.Api.Extensions;

public static class BillingModuleRegistration
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services)
    {
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IBillingCycleRepository, BillingCycleRepository>();
        services.AddScoped<IBillingCalculator, BillingCalculator>();
        services.AddScoped<ITaxRuleRepository, TaxRuleRepository>();
        services.AddScoped<ITaxCalculator, TaxCalculator>();

        services.AddHostedService<BillingGenerationJob>();

        BillingMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/billing")
            .WithTags("Billing");

        BillingEndpoints.Map(group);

        return app;
    }
}
