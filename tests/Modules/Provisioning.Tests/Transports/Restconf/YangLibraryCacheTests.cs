using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public class YangLibraryCacheTests
{
    private readonly YangLibraryCache _cache = new();

    [Fact]
    public void IsModuleSupported_EmptyCache_ShouldReturnFalse()
    {
        _cache.IsModuleSupported("device1", "ietf-interfaces").Should().BeFalse();
    }

    [Fact]
    public void Update_ShouldStoreModules()
    {
        var modules = new[] { new YangModule("ietf-interfaces", null, "urn:ietf:params:xml:ns:yang:ietf-interfaces", null, [], []) };
        _cache.Update("device1", new YangLibraryContent("abc123", modules, DateTime.UtcNow));

        _cache.IsModuleSupported("device1", "ietf-interfaces").Should().BeTrue();
        _cache.IsModuleSupported("device1", "ietf-ip").Should().BeFalse();
    }

    [Fact]
    public void Update_ShouldOverwritePreviousCache()
    {
        var oldModules = new[] { new YangModule("ietf-interfaces", null, "urn:", null, [], []) };
        var newModules = new[] { new YangModule("ietf-ip", null, "urn:", null, [], []) };
        _cache.Update("device1", new YangLibraryContent("old", oldModules, DateTime.UtcNow));
        _cache.Update("device1", new YangLibraryContent("new", newModules, DateTime.UtcNow));

        _cache.IsModuleSupported("device1", "ietf-interfaces").Should().BeFalse();
        _cache.IsModuleSupported("device1", "ietf-ip").Should().BeTrue();
    }

    [Fact]
    public void Invalidate_ShouldClearDeviceCache()
    {
        var modules = new[] { new YangModule("ietf-interfaces", null, "urn:", null, [], []) };
        _cache.Update("device1", new YangLibraryContent("abc", modules, DateTime.UtcNow));
        _cache.Invalidate("device1");

        _cache.IsModuleSupported("device1", "ietf-interfaces").Should().BeFalse();
    }

    [Fact]
    public void GetCachedContentId_NoCache_ShouldReturnNull()
    {
        _cache.GetCachedContentId("device1").Should().BeNull();
    }

    [Fact]
    public void GetCachedContentId_ShouldReturnStoredId()
    {
        var modules = new[] { new YangModule("ietf-interfaces", null, "urn:", null, [], []) };
        _cache.Update("device1", new YangLibraryContent("abc123", modules, DateTime.UtcNow));
        _cache.GetCachedContentId("device1").Should().Be("abc123");
    }

    [Fact]
    public void Update_MultipleDevices_ShouldNotInterfere()
    {
        var modA = new[] { new YangModule("mod-a", null, "urn:a", null, [], []) };
        var modB = new[] { new YangModule("mod-b", null, "urn:b", null, [], []) };
        _cache.Update("device1", new YangLibraryContent("id1", modA, DateTime.UtcNow));
        _cache.Update("device2", new YangLibraryContent("id2", modB, DateTime.UtcNow));

        _cache.IsModuleSupported("device1", "mod-a").Should().BeTrue();
        _cache.IsModuleSupported("device1", "mod-b").Should().BeFalse();
        _cache.IsModuleSupported("device2", "mod-b").Should().BeTrue();
        _cache.IsModuleSupported("device2", "mod-a").Should().BeFalse();
    }
}
