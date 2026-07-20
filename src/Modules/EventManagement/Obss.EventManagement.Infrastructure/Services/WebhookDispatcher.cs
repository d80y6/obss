using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Infrastructure.Diagnostics;

namespace Obss.EventManagement.Infrastructure.Services;

public sealed class WebhookDispatcher : BackgroundService
{
    private static readonly HashSet<string> _forbiddenSchemes = ["file", "ftp", "gopher", "tftp", "dict"];
    private static readonly HashSet<string> _forbiddenHosts = ["169.254.169.254", "metadata.google.internal", "100.100.100.200"];

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookDispatcher> _logger;
    private readonly WebhookMetrics _metrics;
    private readonly HttpClient _httpClient;

    public WebhookDispatcher(
        IServiceProvider serviceProvider,
        ILogger<WebhookDispatcher> logger,
        WebhookMetrics metrics,
        HttpClient httpClient)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _metrics = metrics;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
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

                        if (!IsCallbackUrlValid(subscription.CallbackUrl))
                        {
                            _metrics.SsrfBlocked();
                            webhookEvent.MarkFailed($"Invalid callback URL: {subscription.CallbackUrl}");
                            _logger.LogWarning("SSRF protection blocked webhook {EventId} to {Url}",
                                webhookEvent.Id, subscription.CallbackUrl);
                            continue;
                        }

                        var payload = new
                        {
                            id = webhookEvent.Id,
                            eventType = webhookEvent.EventType,
                            payload = webhookEvent.Payload,
                            timestamp = webhookEvent.CreatedAt
                        };

                        var request = new HttpRequestMessage(HttpMethod.Post, subscription.CallbackUrl)
                        {
                            Content = JsonContent.Create(payload)
                        };

                        if (!string.IsNullOrEmpty(subscription.SigningSecret))
                        {
                            var signature = ComputeSignature(subscription.SigningSecret, payload);
                            request.Headers.Add("X-Webhook-Signature", signature);
                            request.Headers.Add("X-Webhook-Timestamp",
                                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                        }

                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        cts.CancelAfter(TimeSpan.FromSeconds(30));

                        var startedAt = DateTime.UtcNow;
                        var response = await _httpClient.SendAsync(request, cts.Token);
                        var elapsed = (DateTime.UtcNow - startedAt).TotalMilliseconds;

                        if (response.IsSuccessStatusCode)
                        {
                            webhookEvent.MarkDelivered();
                            _metrics.WebhookDelivered();
                            _metrics.RecordDispatchDuration(elapsed);
                            _logger.LogInformation("Webhook {EventId} delivered to {Url}",
                                webhookEvent.Id, subscription.CallbackUrl);
                        }
                        else
                        {
                            webhookEvent.MarkFailed($"HTTP {(int)response.StatusCode}");
                            _metrics.WebhookFailed();
                            _metrics.RecordDispatchDuration(elapsed);
                            _logger.LogWarning("Webhook {EventId} failed: HTTP {Status}",
                                webhookEvent.Id, (int)response.StatusCode);
                        }
                    }
                    catch (TaskCanceledException tex)
                    {
                        webhookEvent.MarkFailed("Timeout after 30s");
                        _metrics.WebhookFailed();
                        _metrics.WebhookRetried();
                        _logger.LogWarning(tex, "Webhook {EventId} timed out", webhookEvent.Id);
                    }
                    catch (HttpRequestException ex)
                    {
                        webhookEvent.MarkFailed($"Connection error: {ex.Message}");
                        _metrics.WebhookFailed();
                        _metrics.WebhookRetried();
                        _logger.LogError(ex, "Webhook {EventId} connection error", webhookEvent.Id);
                    }
                    catch (Exception ex)
                    {
                        webhookEvent.MarkFailed(ex.Message);
                        _metrics.WebhookFailed();
                        _logger.LogError(ex, "Webhook {EventId} delivery error", webhookEvent.Id);
                    }
                }

                var unitOfWork = scope.ServiceProvider.GetRequiredService<Obss.SharedKernel.Application.Abstractions.IUnitOfWork>();
                await unitOfWork.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook dispatch cycle error");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private static bool IsCallbackUrlValid(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase))
            return false;

        if (_forbiddenSchemes.Contains(uri.Scheme))
            return false;

        if (_forbiddenHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
            return false;

        if (uri.Host == "localhost" || uri.Host == "127.0.0.1" || uri.Host == "0.0.0.0")
            return false;

        if (uri.IsLoopback)
            return false;

        return true;
    }

    private static string ComputeSignature(string secret, object payload)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(json);

        var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(messageBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
