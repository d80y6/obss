using Microsoft.Extensions.Logging;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Infrastructure.Services.Adapters;

public sealed class RadiusAdapterStub : IAaaAdapter
{
    private readonly ILogger<RadiusAdapterStub> _logger;

    public RadiusAdapterStub(ILogger<RadiusAdapterStub> logger)
    {
        _logger = logger;
    }

    public string AdapterName => "RadiusStub";
    public AaaProtocolType ProtocolType => AaaProtocolType.Radius;

    public Task<AaaAuthResult> AuthenticateAsync(AaaAuthRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("RADIUS: Authenticating user {Username} via NAS {NasIp}", request.Username, request.NasIp);
        return Task.FromResult(new AaaAuthResult(true, Guid.NewGuid().ToString("N"), "10.0.0.100", null));
    }

    public Task<AaaAcctResult> AccountingAsync(AaaAcctRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("RADIUS: Accounting {Status} for session {SessionId}", request.AcctStatusType, request.SessionId);
        return Task.FromResult(new AaaAcctResult(true, request.SessionId, null));
    }

    public Task<AaaCoAResult> ChangeOfAuthorizationAsync(AaaCoARequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("RADIUS: CoA for session {SessionId} with {Count} attributes", request.SessionId, request.Attributes.Count);
        return Task.FromResult(new AaaCoAResult(true, null));
    }

    public Task<bool> TestConnectionAsync(NetworkAccessServer nas, CancellationToken ct = default)
    {
        _logger.LogInformation("RADIUS: Testing connection to NAS {Name} at {Ip}", nas.Name, nas.NasIpAddress);
        return Task.FromResult(true);
    }
}
