using Microsoft.Extensions.Logging;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Infrastructure.Services.Adapters;

public sealed class TacacsPlusAdapterStub : IAaaAdapter
{
    private readonly ILogger<TacacsPlusAdapterStub> _logger;

    public TacacsPlusAdapterStub(ILogger<TacacsPlusAdapterStub> logger)
    {
        _logger = logger;
    }

    public string AdapterName => "TacacsPlusStub";
    public AaaProtocolType ProtocolType => AaaProtocolType.TacacsPlus;

    public Task<AaaAuthResult> AuthenticateAsync(AaaAuthRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("TACACS+: Authenticating admin {Username}", request.Username);
        return Task.FromResult(new AaaAuthResult(true, Guid.NewGuid().ToString("N"), null, null));
    }

    public Task<AaaAcctResult> AccountingAsync(AaaAcctRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("TACACS+: Accounting for session {SessionId}", request.SessionId);
        return Task.FromResult(new AaaAcctResult(true, request.SessionId, null));
    }

    public Task<AaaCoAResult> ChangeOfAuthorizationAsync(AaaCoARequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("TACACS+: CoA for session {SessionId}", request.SessionId);
        return Task.FromResult(new AaaCoAResult(true, null));
    }

    public Task<bool> TestConnectionAsync(NetworkAccessServer nas, CancellationToken ct = default)
    {
        _logger.LogInformation("TACACS+: Testing connection to {Name}", nas.Name);
        return Task.FromResult(true);
    }
}
