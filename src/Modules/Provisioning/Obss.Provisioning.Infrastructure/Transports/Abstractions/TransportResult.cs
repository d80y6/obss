namespace Obss.Provisioning.Infrastructure.Transports.Abstractions;

public record TransportResult
{
    public bool Success { get; init; }
    public string? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }
    public TransportProtocol Protocol { get; init; }

    public static TransportResult Ok(string? data = null, TimeSpan? duration = null, TransportProtocol protocol = TransportProtocol.Rest)
    {
        return new TransportResult
        {
            Success = true,
            Data = data,
            Duration = duration ?? TimeSpan.Zero,
            Protocol = protocol
        };
    }

    public static TransportResult Fail(string error, TimeSpan? duration = null, TransportProtocol protocol = TransportProtocol.Rest)
    {
        return new TransportResult
        {
            Success = false,
            ErrorMessage = error,
            Duration = duration ?? TimeSpan.Zero,
            Protocol = protocol
        };
    }
}
