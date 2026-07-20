using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Obss.Notifications.Api.Endpoints;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Application.BackgroundJobs;
using Obss.Notifications.Application.Mappings;
using Obss.Notifications.Infrastructure.Persistence;
using Obss.Notifications.Infrastructure.Persistence.Repositories;
using Obss.Notifications.Infrastructure.Services;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Notifications.Api.Extensions;

public static class NotificationsModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailSender>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SmtpOptions>>();
            if (options.Value.Enabled)
                return ActivatorUtilities.CreateInstance<SmtpEmailSender>(sp);
            return ActivatorUtilities.CreateInstance<EmailService>(sp);
        });
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddHostedService<NotificationDeliveryJob>();

        NotificationMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/notifications")
            .WithTags("Notifications");

        NotificationEndpoints.Map(group);
        TemplateEndpoints.Map(group);

        return app;
    }
}
