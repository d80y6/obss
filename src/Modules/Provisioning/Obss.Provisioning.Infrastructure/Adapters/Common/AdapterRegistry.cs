using System.Collections.Concurrent;
using Obss.Provisioning.Application.Abstractions;

namespace Obss.Provisioning.Infrastructure.Adapters.Common;

public sealed class AdapterRegistry : IAdapterRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, IProvisioningAdapter>> _adapters = new();

    public void Register(string technologyType, string adapterName, IProvisioningAdapter adapter)
    {
        var techAdapters = _adapters.GetOrAdd(
            technologyType.ToLowerInvariant(),
            _ => new ConcurrentDictionary<string, IProvisioningAdapter>(StringComparer.OrdinalIgnoreCase));

        techAdapters[adapterName] = adapter;
    }

    public IProvisioningAdapter? GetAdapter(string technologyType, string adapterName)
    {
        if (_adapters.TryGetValue(technologyType.ToLowerInvariant(), out var techAdapters))
        {
            return techAdapters.GetValueOrDefault(adapterName);
        }

        return null;
    }

    public IEnumerable<(string Technology, string Name, bool IsHealthy)> GetAllAdapters()
    {
        foreach (var (tech, techAdapters) in _adapters)
        {
            foreach (var (name, _) in techAdapters)
            {
                yield return (tech, name, true);
            }
        }
    }
}
