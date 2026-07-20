namespace Obss.Provisioning.Infrastructure.Adapters.Common;

public sealed record AdapterHealthStatus
{
    public bool IsHealthy { get; init; }
    public string AdapterName { get; init; } = string.Empty;
    public string? StatusMessage { get; init; }
    public TimeSpan? Latency { get; init; }
    public DateTime LastChecked { get; init; } = DateTime.UtcNow;
}
