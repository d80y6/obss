using System.Collections.Concurrent;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class YangLibraryCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public void Update(string deviceId, YangLibraryContent content)
        => _cache[deviceId] = new CacheEntry(content, DateTime.UtcNow);

    public void Invalidate(string deviceId)
        => _cache.TryRemove(deviceId, out _);

    public bool IsModuleSupported(string deviceId, string moduleName)
        => _cache.TryGetValue(deviceId, out var entry)
            && entry.Content.Modules.Any(m => m.Name == moduleName);

    public string? GetCachedContentId(string deviceId)
        => _cache.TryGetValue(deviceId, out var entry) ? entry.Content.ContentId : null;

    private sealed record CacheEntry(YangLibraryContent Content, DateTime UpdatedAt);
}
