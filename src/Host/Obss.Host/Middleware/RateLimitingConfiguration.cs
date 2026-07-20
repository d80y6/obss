namespace Obss.Host.Middleware;

public sealed class RateLimitingConfiguration
{
    public Dictionary<string, RateLimitRule> Rules { get; set; } = new()
    {
        ["/api/catalog"] = new() { Burst = 100, Period = TimeSpan.FromMinutes(1) },
        ["/api/qualification"] = new() { Burst = 30, Period = TimeSpan.FromMinutes(1) },
        ["/api/orders"] = new() { Burst = 20, Period = TimeSpan.FromMinutes(1) },
        ["/api/cdr"] = new() { Burst = 1000, Period = TimeSpan.FromMinutes(1) },
        ["/api/payments"] = new() { Burst = 10, Period = TimeSpan.FromMinutes(1) },
        ["/api/auth"] = new() { Burst = 5, Period = TimeSpan.FromMinutes(1) },
        ["/api/provisioning"] = new() { Burst = 30, Period = TimeSpan.FromMinutes(1) },
        ["/api/billing"] = new() { Burst = 30, Period = TimeSpan.FromMinutes(1) },
        ["/api/subscriptions"] = new() { Burst = 30, Period = TimeSpan.FromMinutes(1) },
        ["/api/inventory"] = new() { Burst = 60, Period = TimeSpan.FromMinutes(1) },
        ["/api/notifications"] = new() { Burst = 20, Period = TimeSpan.FromMinutes(1) },
    };

    public string DefaultPolicy { get; set; } = "fixed";
    public bool Enabled { get; set; } = true;
}

public sealed class RateLimitRule
{
    public int Burst { get; set; }
    public TimeSpan Period { get; set; }
    public bool EnableRateLimiting { get; set; } = true;
}
