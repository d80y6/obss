using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.EventManagement.Application.Abstractions;

namespace Obss.EventManagement.Infrastructure.Services;

public sealed class WebhookDispatcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookDispatcher> _logger;
    private readonly HttpClient _httpClient;

    public WebhookDispatcher(
        IServiceProvider serviceProvider,
        ILogger<WebhookDispatcher> logger,
        HttpClient httpClient)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var eventRepo = scope.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
                var subRepo = scope.ServiceProvider.GetRequiredService<IEventSubscriptionRepository>();

                var pendingEvents = await eventRepo.GetPendingEventsAsync(stoppingToken);

                foreach (var webhookEvent in pendingEvents)
                {
                    try
                    {
                        var subscription = await subRepo.GetByIdAsync(webhookEvent.SubscriptionId, stoppingToken);
                        if (subscription is null || subscription.Status != "active")
                        {
                            webhookEvent.MarkFailed("Subscription not found or inactive");
                            continue;
                        }

                        var response = await _httpClient.PostAsJsonAsync(
                            subscription.CallbackUrl,
                            new
                            {
                                id = webhookEvent.Id,
                                eventType = webhookEvent.EventType,
                                payload = webhookEvent.Payload,
                                timestamp = webhookEvent.CreatedAt
                            },
                            stoppingToken);

                        if (response.IsSuccessStatusCode)
                        {
                            webhookEvent.MarkDelivered();
                            _logger.LogInformation("Webhook {EventId} delivered to {Url}",
                                webhookEvent.Id, subscription.CallbackUrl);
                        }
                        else
                        {
                            webhookEvent.MarkFailed($"HTTP {response.StatusCode}");
                            _logger.LogWarning("Webhook {EventId} failed: HTTP {Status}",
                                webhookEvent.Id, response.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        webhookEvent.MarkFailed(ex.Message);
                        _logger.LogError(ex, "Webhook {EventId} delivery error", webhookEvent.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook dispatch cycle error");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
