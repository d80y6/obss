using System.Diagnostics.Metrics;

namespace Obss.SharedKernel.Infrastructure.Diagnostics;

public sealed class RabbitMqMetrics
{
    public static readonly Meter Meter = new("Obss.SharedKernel.RabbitMq", "1.0.0");

    private readonly Counter<int> _consumedCounter;
    private readonly Counter<int> _failedCounter;
    private readonly Counter<int> _retriedCounter;
    private readonly Counter<int> _deadLetteredCounter;
    private readonly Histogram<double> _messageProcessingDuration;

    public RabbitMqMetrics()
    {
        _consumedCounter = Meter.CreateCounter<int>("rabbitmq.messages.consumed",
            description: "Number of messages consumed from RabbitMQ");
        _failedCounter = Meter.CreateCounter<int>("rabbitmq.messages.failed",
            description: "Number of messages that failed processing");
        _retriedCounter = Meter.CreateCounter<int>("rabbitmq.messages.retried",
            description: "Number of messages requeued for retry");
        _deadLetteredCounter = Meter.CreateCounter<int>("rabbitmq.messages.dead_lettered",
            description: "Number of messages sent to dead-letter queue");
        _messageProcessingDuration = Meter.CreateHistogram<double>("rabbitmq.processing.duration_ms",
            unit: "ms", description: "Message processing duration in milliseconds");
    }

    public void MessageConsumed() => _consumedCounter.Add(1);
    public void MessageFailed() => _failedCounter.Add(1);
    public void MessageRetried() => _retriedCounter.Add(1);
    public void MessageDeadLettered() => _deadLetteredCounter.Add(1);
    public void RecordProcessingDuration(double ms) => _messageProcessingDuration.Record(ms);
}
