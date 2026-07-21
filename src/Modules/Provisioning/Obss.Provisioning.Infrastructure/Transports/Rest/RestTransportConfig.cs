using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Rest;

public enum RestAuthType
{
    None,
    Basic,
    Bearer,
    ApiKey,
    Digest
}

public sealed record RestTransportConfig : TransportConfigBase
{
    public RestAuthType AuthType { get; init; } = RestAuthType.None;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? BearerToken { get; init; }
    public string? ApiKey { get; init; }
    public string? ApiKeyHeader { get; init; } = "X-API-Key";
    public bool UseTls { get; init; } = true;
    public bool ValidateCertificate { get; init; } = true;
    public string? BaseUrl { get; init; }
    public Dictionary<string, string> DefaultHeaders { get; init; } = new();

    public override TransportProtocol Protocol => TransportProtocol.Rest;
}
