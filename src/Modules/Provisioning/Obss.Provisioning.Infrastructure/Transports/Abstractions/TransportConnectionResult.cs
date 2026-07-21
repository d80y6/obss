namespace Obss.Provisioning.Infrastructure.Transports.Abstractions;

public sealed record TransportConnectionResult
{
    public bool Success { get; init; }
    public string? DeviceInfo { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ResponseTime { get; init; }

    public static TransportConnectionResult Ok(string? deviceInfo = null, TimeSpan? responseTime = null)
    {
        return new TransportConnectionResult
        {
            Success = true,
            DeviceInfo = deviceInfo,
            ResponseTime = responseTime ?? TimeSpan.Zero
        };
    }

    public static TransportConnectionResult Fail(string error, TimeSpan? responseTime = null)
    {
        return new TransportConnectionResult
        {
            Success = false,
            ErrorMessage = error,
            ResponseTime = responseTime ?? TimeSpan.Zero
        };
    }
}
