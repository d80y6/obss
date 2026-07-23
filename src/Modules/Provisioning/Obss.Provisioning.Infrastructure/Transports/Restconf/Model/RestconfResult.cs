using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public sealed record RestconfResult : TransportResult
{
    public new static RestconfResult Ok(string? data = null, TimeSpan? duration = null, TransportProtocol protocol = TransportProtocol.Restconf)
        => new() { Success = true, Data = data, Duration = duration ?? TimeSpan.Zero, Protocol = protocol };

    public new static RestconfResult Fail(string error, TimeSpan? duration = null, TransportProtocol protocol = TransportProtocol.Restconf)
        => new() { Success = false, ErrorMessage = error, Duration = duration ?? TimeSpan.Zero, Protocol = protocol };
}