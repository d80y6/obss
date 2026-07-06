using Microsoft.Extensions.DependencyInjection;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Infrastructure.Persistence.Repositories;

namespace Obss.ServiceCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceCatalogInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceCandidateRepository, ServiceCandidateRepository>();
        services.AddScoped<IServiceSpecificationRepository, ServiceSpecificationRepository>();
        return services;
   }
}
