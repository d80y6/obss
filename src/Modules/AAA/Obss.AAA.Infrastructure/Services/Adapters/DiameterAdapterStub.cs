using Microsoft.Extensions.Logging;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Infrastructure.Services.Adapters;

public sealed class DiameterAdapterStub : IAaaAdapter
{
    private readonly ILogger<DiameterAdapterStub> _logger;

    public DiameterAdapterStub(ILogger<DiameterAdapterStub> logger)
    {
        _logger = logger;
    }

    public string AdapterName => "DiameterStub";
    public AaaProtocolType ProtocolType => AaaProtocolType.Diameter;

    public Task<AaaAuthResult> AuthenticateAsync(AaaAuthRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Diameter: Authenticating user {Username}", request.Username);
        return Task.FromResult(new AaaAuthResult(true, Guid.NewGuid().ToString("N"), "10.0.0.200", null));
    }

    public Task<AaaAcctResult> AccountingAsync(AaaAcctRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Diameter: Accounting for session {SessionId}", request.SessionId);
        return Task.FromResult(new AaaAcctResult(true, request.SessionId, null));
    }

    public Task<AaaCoAResult> ChangeOfAuthorizationAsync(AaaCoARequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Diameter: CoA for session {SessionId}", request.SessionId);
        return Task.FromResult(new AaaCoAResult(true, null));
    }

    public Task<bool> TestConnectionAsync(NetworkAccessServer nas, CancellationToken ct = default)
    {
        _logger.LogInformation("Diameter: Testing connection to {Name}", nas.Name);
        return Task.FromResult(true);
    }
}
