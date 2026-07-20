using System.Diagnostics.Metrics;

namespace Obss.SharedKernel.Infrastructure.Diagnostics;

public sealed class OutboxMetrics
{
    public static readonly Meter Meter = new("Obss.SharedKernel.Outbox", "1.0.0");

    private readonly Counter<int> _processedCounter;
    private readonly Counter<int> _failedCounter;
    private readonly Counter<int> _deadLetteredCounter;
    private readonly Histogram<double> _processingDuration;

    public OutboxMetrics()
    {
        _processedCounter = Meter.CreateCounter<int>("outbox.messages.processed",
            description: "Number of outbox messages processed");
        _failedCounter = Meter.CreateCounter<int>("outbox.messages.failed",
            description: "Number of outbox messages that failed");
        _deadLetteredCounter = Meter.CreateCounter<int>("outbox.messages.dead_lettered",
            description: "Number of outbox messages dead-lettered after max retries");
        _processingDuration = Meter.CreateHistogram<double>("outbox.processing.duration_ms",
            unit: "ms", description: "Outbox message processing duration in milliseconds");
    }

    public void MessageProcessed() => _processedCounter.Add(1);
    public void MessageFailed() => _failedCounter.Add(1);
    public void MessageDeadLettered() => _deadLetteredCounter.Add(1);
    public void RecordProcessingDuration(double ms) => _processingDuration.Record(ms);
}
