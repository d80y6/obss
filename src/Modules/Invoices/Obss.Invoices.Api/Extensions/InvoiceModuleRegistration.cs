using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Invoices.Api.Endpoints;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Mappings;
using Obss.Invoices.Application.Services;
using Obss.Invoices.Infrastructure.Persistence;
using Obss.Invoices.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Invoices.Api.Extensions;

public static class InvoiceModuleRegistration
{
    public static IServiceCollection AddInvoiceModule(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceDisputeRepository, InvoiceDisputeRepository>();
        services.AddScoped<IBillQuery, BillQuery>();
        services.AddScoped<IInvoicePresenter, InvoicePresenter>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        InvoiceMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/invoices")
            .WithTags("Invoices");

        InvoiceEndpoints.Map(group);

        return app;
    }
}
