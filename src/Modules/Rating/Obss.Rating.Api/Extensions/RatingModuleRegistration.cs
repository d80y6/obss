using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Rating.Api.Endpoints;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.BackgroundJobs;
using Obss.Rating.Application.Mappings;
using Obss.Rating.Domain.DomainServices;
using Obss.Rating.Infrastructure.Persistence;
using Obss.Rating.Infrastructure.Persistence.Repositories;
using Obss.Rating.Infrastructure.Services;

namespace Obss.Rating.Api.Extensions;

public static class RatingModuleRegistration
{
    public static IServiceCollection AddRatingModule(this IServiceCollection services)
    {

        services.AddScoped<IUsageRecordRepository, UsageRecordRepository>();
        services.AddScoped<IRatingRuleRepository, RatingRuleRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IRatingEngine, RatingEngine>();
        services.AddScoped<IPromotionEngine, PromotionEngine>();

        services.AddHostedService<UsageRatingJob>();

        RatingMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapRatingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/rating")
            .WithTags("Rating");

        RatingEndpoints.Map(group);

        return app;
    }
}
