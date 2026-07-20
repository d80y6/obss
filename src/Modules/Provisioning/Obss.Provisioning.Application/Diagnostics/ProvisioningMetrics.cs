using System.Diagnostics.Metrics;

namespace Obss.Provisioning.Application.Diagnostics;

public sealed class ProvisioningMetrics
{
    public static readonly Meter Meter = new("Obss.Provisioning.Tasks", "1.0.0");

    private readonly Counter<int> _tasksStartedCounter;
    private readonly Counter<int> _tasksCompletedCounter;
    private readonly Counter<int> _tasksFailedCounter;
    private readonly Counter<int> _tasksRetriedCounter;
    private readonly Histogram<double> _taskDuration;

    public ProvisioningMetrics()
    {
        _tasksStartedCounter = Meter.CreateCounter<int>("provisioning.tasks.started",
            description: "Number of provisioning tasks started");
        _tasksCompletedCounter = Meter.CreateCounter<int>("provisioning.tasks.completed",
            description: "Number of provisioning tasks completed successfully");
        _tasksFailedCounter = Meter.CreateCounter<int>("provisioning.tasks.failed",
            description: "Number of provisioning tasks that failed");
        _tasksRetriedCounter = Meter.CreateCounter<int>("provisioning.tasks.retried",
            description: "Number of provisioning task retries");
        _taskDuration = Meter.CreateHistogram<double>("provisioning.task.duration_ms",
            unit: "ms", description: "Provisioning task execution duration in milliseconds");
    }

    public void TaskStarted() => _tasksStartedCounter.Add(1);
    public void TaskCompleted() => _tasksCompletedCounter.Add(1);
    public void TaskFailed() => _tasksFailedCounter.Add(1);
    public void TaskRetried() => _tasksRetriedCounter.Add(1);
    public void RecordTaskDuration(double ms) => _taskDuration.Record(ms);
}
