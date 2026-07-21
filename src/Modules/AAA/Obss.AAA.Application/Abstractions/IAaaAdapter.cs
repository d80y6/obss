using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Application.Abstractions;

public interface IAaaAdapter
{
    string AdapterName { get; }
    AaaProtocolType ProtocolType { get; }

    Task<AaaAuthResult> AuthenticateAsync(AaaAuthRequest request, CancellationToken ct = default);

    Task<AaaAcctResult> AccountingAsync(AaaAcctRequest request, CancellationToken ct = default);

    Task<AaaCoAResult> ChangeOfAuthorizationAsync(AaaCoARequest request, CancellationToken ct = default);

    Task<bool> TestConnectionAsync(NetworkAccessServer nas, CancellationToken ct = default);
}
