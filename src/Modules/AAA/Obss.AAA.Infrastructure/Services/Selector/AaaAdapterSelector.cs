using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Infrastructure.Services.Selector;

public sealed class AaaAdapterSelector : IAaaAdapterSelector
{
    private readonly IReadOnlyDictionary<AaaProtocolType, IAaaAdapter> _adapters;

    public AaaAdapterSelector(IEnumerable<IAaaAdapter> adapters)
    {
        _adapters = adapters.ToDictionary(a => a.ProtocolType);
    }

    public IReadOnlyDictionary<AaaProtocolType, IAaaAdapter> AllAdapters => _adapters;

    public IAaaAdapter GetAdapter(AaaProtocolType protocolType)
    {
        if (_adapters.TryGetValue(protocolType, out var adapter))
            return adapter;

        throw new InvalidOperationException($"No adapter registered for protocol '{protocolType}'");
    }

    public IAaaAdapter GetAdapter(NetworkAccessServer nas)
    {
        var protocolType = nas.NasType switch
        {
            NasType.BRAS or NasType.BNG => AaaProtocolType.Radius,
            NasType.UAG => AaaProtocolType.Diameter,
            NasType.WLC or NasType.VSAT => AaaProtocolType.Radius,
            _ => AaaProtocolType.Radius,
        };

        return GetAdapter(protocolType);
    }
}
