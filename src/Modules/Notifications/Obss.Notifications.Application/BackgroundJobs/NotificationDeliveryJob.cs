using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Notifications.Application.BackgroundJobs;

public sealed class NotificationDeliveryJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationDeliveryJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public NotificationDeliveryJob(
        IServiceProvider serviceProvider,
        ILogger<NotificationDeliveryJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification delivery job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotifications(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing pending notifications.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingNotifications(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var smsSender = scope.ServiceProvider.GetRequiredService<ISmsSender>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pending = await notificationRepository.GetPendingAsync(50, cancellationToken);
        _logger.LogInformation("Found {Count} pending notifications to deliver.", pending.Count);

        foreach (var notification in pending)
        {
            try
            {
                bool success = notification.NotificationType switch
                {
                    NotificationType.Email => await emailSender.SendEmailAsync(
                        notification.Recipient, notification.Subject, notification.Body, cancellationToken),
                    NotificationType.SMS => await smsSender.SendSmsAsync(
                        notification.Recipient, notification.Body, cancellationToken),
                    _ => true
                };

                if (success)
                {
                    notification.MarkSent();
                    _logger.LogInformation(
                        "Notification {NotificationId} marked as sent.", notification.Id);
                }
                else
                {
                    notification.MarkFailed("Delivery service returned failure.");
                    _logger.LogWarning(
                        "Notification {NotificationId} delivery failed.", notification.Id);
                }
            }
            catch (Exception ex)
            {
                notification.MarkFailed(ex.Message);
                _logger.LogError(ex,
                    "Error delivering notification {NotificationId}.", notification.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
