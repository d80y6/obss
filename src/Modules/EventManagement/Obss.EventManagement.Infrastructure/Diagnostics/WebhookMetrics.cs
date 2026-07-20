using System.Diagnostics.Metrics;

namespace Obss.EventManagement.Infrastructure.Diagnostics;

public sealed class WebhookMetrics
{
    public static readonly Meter Meter = new("Obss.EventManagement.Webhooks", "1.0.0");

    private readonly Counter<int> _deliveredCounter;
    private readonly Counter<int> _failedCounter;
    private readonly Counter<int> _retriedCounter;
    private readonly Histogram<double> _dispatchDuration;
    private readonly Counter<int> _ssrfBlockedCounter;

    public WebhookMetrics()
    {
        _deliveredCounter = Meter.CreateCounter<int>("webhooks.delivered",
            description: "Number of webhooks successfully delivered");
        _failedCounter = Meter.CreateCounter<int>("webhooks.failed",
            description: "Number of webhooks that failed delivery");
        _retriedCounter = Meter.CreateCounter<int>("webhooks.retried",
            description: "Number of webhook delivery retries");
        _dispatchDuration = Meter.CreateHistogram<double>("webhooks.dispatch.duration_ms",
            unit: "ms", description: "Webhook dispatch duration in milliseconds");
        _ssrfBlockedCounter = Meter.CreateCounter<int>("webhooks.ssrf_blocked",
            description: "Number of webhook deliveries blocked by SSRF protection");
    }

    public void WebhookDelivered() => _deliveredCounter.Add(1);
    public void WebhookFailed() => _failedCounter.Add(1);
    public void WebhookRetried() => _retriedCounter.Add(1);
    public void RecordDispatchDuration(double ms) => _dispatchDuration.Record(ms);
    public void SsrfBlocked() => _ssrfBlockedCounter.Add(1);
}
