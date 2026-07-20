namespace Obss.Provisioning.Infrastructure.Adapters.Common;

public abstract class AdapterConfigurationBase
{
    public string? ControllerUrl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool ValidateCertificate { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public bool EnableCircuitBreaker { get; set; }
    public int CircuitBreakerThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromMinutes(5);
}
