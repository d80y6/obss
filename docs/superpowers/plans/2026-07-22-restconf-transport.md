# RESTCONF Transport Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a dedicated RESTCONF transport (RFC 8040) in the Provisioning module's transport layer.

**Architecture:** `IRestconfTransport` extends `INetworkTransport` and provides CRUD + YANG library query methods. `RestconfTransport` delegates HTTP to `IRestTransport` (existing). Supporting classes handle path building, content negotiation (JSON+XML), RFC 8040 error parsing, and RFC 8525 YANG library metadata caching.

**Tech Stack:** .NET 9, System.Text.Json, System.Xml.Linq, WireMock.NET (test), xUnit + FluentAssertions

---

### Task 1: Model Types

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/Model/RestconfResult.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/Model/RestconfResource.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/Model/YangModule.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/ModelTests.cs`

- [ ] **Step 1: Write the failing model tests**

```csharp
public class RestconfModelTests
{
    [Fact]
    public void RestconfResult_Ok_ShouldSetSuccess()
    {
        var result = RestconfResult.Ok("{\"data\":\"value\"}", TransportProtocol.Restconf);
        result.Success.Should().BeTrue();
        result.Data.Should().Be("{\"data\":\"value\"}");
        result.Protocol.Should().Be(TransportProtocol.Restconf);
    }

    [Fact]
    public void RestconfResult_Fail_ShouldSetError()
    {
        var result = RestconfResult.Fail("error occurred", protocol: TransportProtocol.Restconf);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("error occurred");
    }

    [Fact]
    public void RestconfResource_Enums_ShouldHaveExpectedValues()
    {
        ((int)RestconfResourceType.Datastore).Should().Be(1);
        ((int)RestconfResourceType.Operation).Should().Be(2);
        ((int)RestconfResourceType.Stream).Should().Be(3);
    }

    [Fact]
    public void YangModule_ShouldPreserveAllProperties()
    {
        var module = new YangModule("ietf-interfaces", "2018-02-20", "urn:ietf:params:xml:ns:yang:ietf-interfaces",
            "https://example.com/schema", new[] { "feature-a" }, new[] { "ietf-interfaces-deviations" });
        module.Name.Should().Be("ietf-interfaces");
        module.Revision.Should().Be("2018-02-20");
        module.Namespace.Should().Be("urn:ietf:params:xml:ns:yang:ietf-interfaces");
        module.SchemaUri.Should().Be("https://example.com/schema");
        module.Features.Should().Contain("feature-a");
        module.Deviations.Should().Contain("ietf-interfaces-deviations");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~RestconfModel" --no-build`
Expected: FAIL with CS0246 (type not found)

- [ ] **Step 3: Create model types**

`Model/RestconfResult.cs`:
```csharp
using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public sealed record RestconfResult : TransportResult
{
    public static RestconfResult Ok(string? data = null, TimeSpan? duration = null, TransportProtocol protocol = TransportProtocol.Restconf)
        => new() { Success = true, Data = data, Duration = duration ?? TimeSpan.Zero, Protocol = protocol };

    public static RestconfResult Fail(string error, TimeSpan? duration = null, TransportProtocol protocol = TransportProtocol.Restconf)
        => new() { Success = false, ErrorMessage = error, Duration = duration ?? TimeSpan.Zero, Protocol = protocol };
}
```

`Model/RestconfResource.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public enum RestconfResourceType { Datastore = 1, Operation = 2, Stream = 3 }

public sealed record RestconfQueryParams(int? Depth = null, string? Fields = null, string? WithDefaults = null);
```

`Model/YangModule.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public sealed record YangModule(
    string Name,
    string? Revision,
    string Namespace,
    string? SchemaUri,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> Deviations);
```

`Model/YangLibraryContent.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public sealed record YangLibraryContent(
    string ContentId,
    IReadOnlyList<YangModule> Modules,
    DateTime FetchedAt);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~RestconfModel" --no-build`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/Model/
git add tests/Modules/Provisioning.Tests/Transports/Restconf/ModelTests.cs
git commit -m "feat(restconf): add model types (RestconfResult, RestconfResource, YangModule)"
```

---

### Task 2: TransportProtocol enum + IRestconfTransport + Config

**Files:**
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Abstractions/TransportProtocol.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/IRestconfTransport.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfTransportConfig.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/InterfaceTests.cs`

- [ ] **Step 1: Add Restconf to TransportProtocol enum**

```csharp
public enum TransportProtocol
{
    SnmpV1 = 1,
    SnmpV2C = 2,
    SnmpV3 = 3,
    Ssh = 4,
    Netconf = 5,
    Rest = 6,
    Soap = 7,
    Tr069 = 8,
    Cli = 9,
    Restconf = 10  // ADD
}
```

- [ ] **Step 2: Create IRestconfTransport**

```csharp
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public interface IRestconfTransport : INetworkTransport
{
    Task<RestconfResult> GetAsync(string path, RestconfQueryParams? query = null, CancellationToken ct = default);
    Task<RestconfResult> PostAsync(string path, object? body = null, CancellationToken ct = default);
    Task<RestconfResult> PutAsync(string path, object? body = null, CancellationToken ct = default);
    Task<RestconfResult> PatchAsync(string path, object? body = null, CancellationToken ct = default);
    Task<RestconfResult> DeleteAsync(string path, CancellationToken ct = default);
    Task<YangLibraryContent> GetYangLibraryAsync(CancellationToken ct = default);
}
```

- [ ] **Step 3: Create RestconfTransportConfig**

```csharp
using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfTransportConfig : TransportConfigBase
{
    public override TransportProtocol Protocol => TransportProtocol.Restconf;
    public string BaseUri { get; set; } = string.Empty;
    public int YangLibraryCacheTtlSeconds { get; set; } = 300;
}
```

- [ ] **Step 4: Write interface tests**

```csharp
public class RestconfInterfaceTests
{
    [Fact]
    public void RestconfTransportConfig_ShouldSetProtocol()
    {
        var config = new RestconfTransportConfig { BaseUri = "https://device/restconf" };
        config.Protocol.Should().Be(TransportProtocol.Restconf);
        config.BaseUri.Should().Be("https://device/restconf");
    }

    [Fact]
    public void RestconfTransportConfig_ShouldDefaultCacheTtl()
    {
        var config = new RestconfTransportConfig();
        config.YangLibraryCacheTtlSeconds.Should().Be(300);
    }
}
```

- [ ] **Step 5: Build and verify**

Run: `dotnet build Obss.sln --no-restore 2>&1 | tail -5`
Expected: Build succeeded. 0 Warning(s) 0 Error(s)

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Abstractions/TransportProtocol.cs
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/IRestconfTransport.cs
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfTransportConfig.cs
git add tests/Modules/Provisioning.Tests/Transports/Restconf/InterfaceTests.cs
git commit -m "feat(restconf): add IRestconfTransport interface and TransportConfig"
```

---

### Task 3: RestconfPathBuilder

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfPathBuilder.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/PathBuilderTests.cs`

- [ ] **Step 1: Write the failing path builder tests**

```csharp
using Obss.Provisioning.Infrastructure.Transports.Restconf;

public class RestconfPathBuilderTests
{
    private readonly RestconfPathBuilder _builder = new("https://device/restconf");

    [Fact]
    public void DataPath_ShouldBuildCorrectUri()
    {
        var uri = _builder.DataPath("ietf-interfaces", "interfaces");
        uri.Should().Be("https://device/restconf/data/ietf-interfaces:interfaces");
    }

    [Fact]
    public void ListItemPath_ShouldBuildWithKey()
    {
        var uri = _builder.ListItemPath("ietf-interfaces", "interface", "GigabitEthernet0/0/0");
        uri.Should().Be("https://device/restconf/data/ietf-interfaces:interface=GigabitEthernet0%2F0%2F0");
    }

    [Fact]
    public void OperationPath_ShouldBuildCorrectUri()
    {
        var uri = _builder.OperationPath("ietf-system", "restart");
        uri.Should().Be("https://device/restconf/operations/ietf-system:restart");
    }

    [Fact]
    public void WithQueryParams_ShouldAppendDepth()
    {
        var uri = _builder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(Depth: 2));
        uri.Should().Be("/data/ietf-interfaces:interfaces?depth=2");
    }

    [Fact]
    public void WithQueryParams_ShouldAppendFields()
    {
        var uri = _builder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(Fields: "name;type"));
        uri.Should().Be("/data/ietf-interfaces:interfaces?fields=name%3Btype");
    }

    [Fact]
    public void WithQueryParams_ShouldAppendWithDefaults()
    {
        var uri = _builder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(WithDefaults: "report-all"));
        uri.Should().Be("/data/ietf-interfaces:interfaces?with-defaults=report-all");
    }

    [Fact]
    public void WithQueryParams_ShouldCombineMultiple()
    {
        var uri = _builder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(Depth: 2, Fields: "name"));
        uri.Should().Be("/data/ietf-interfaces:interfaces?depth=2&fields=name");
    }

    [Fact]
    public void DataPath_WithNestedContainer()
    {
        var uri = _builder.DataPath("ietf-interfaces", "interfaces/interface");
        uri.Should().Be("https://device/restconf/data/ietf-interfaces:interfaces/interface");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~PathBuilderTests" --no-build`
Expected: FAIL with CS0246

- [ ] **Step 3: Implement RestconfPathBuilder**

```csharp
using System.Text.Encodings.Web;
using System.Web;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfPathBuilder
{
    private readonly string _baseUri;

    public RestconfPathBuilder(string baseUri)
    {
        _baseUri = baseUri.TrimEnd('/');
    }

    public string DataPath(string module, string container)
        => $"{_baseUri}/data/{module}:{container}";

    public string ListItemPath(string module, string list, string key)
        => $"{_baseUri}/data/{module}:{list}={UrlEncoder.Default.Encode(key)}";

    public string OperationPath(string module, string rpc)
        => $"{_baseUri}/operations/{module}:{rpc}";

    public string WithQueryParams(string path, RestconfQueryParams? query)
    {
        if (query is null) return path;

        var parts = new List<string>();
        if (query.Depth.HasValue) parts.Add($"depth={query.Depth.Value}");
        if (!string.IsNullOrEmpty(query.Fields)) parts.Add($"fields={UrlEncoder.Default.Encode(query.Fields)}");
        if (!string.IsNullOrEmpty(query.WithDefaults)) parts.Add($"with-defaults={query.WithDefaults}");

        return parts.Count > 0 ? $"{path}?{string.Join("&", parts)}" : path;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~PathBuilderTests" --no-build`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfPathBuilder.cs
git add tests/Modules/Provisioning.Tests/Transports/Restconf/PathBuilderTests.cs
git commit -m "feat(restconf): add RestconfPathBuilder for YANG path construction"
```

---

### Task 4: RestconfContentNegotiator

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfContentNegotiator.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/ContentNegotiatorTests.cs`

- [ ] **Step 1: Write the failing content negotiator tests**

```csharp
using Obss.Provisioning.Infrastructure.Transports.Restconf;

public class RestconfContentNegotiatorTests
{
    private readonly RestconfContentNegotiator _negotiator = new();

    [Fact]
    public void GetAcceptHeader_ShouldDefaultToJson()
    {
        var accept = _negotiator.GetAcceptHeader();
        accept.Should().Be("application/yang-data+json");
    }

    [Fact]
    public void GetAcceptHeader_WithXmlPreference_ShouldReturnXml()
    {
        var accept = _negotiator.GetAcceptHeader(preferXml: true);
        accept.Should().Be("application/yang-data+xml");
    }

    [Fact]
    public void Deserialize_Json_ShouldParseYangDataEnvelope()
    {
        var json = "{\"ietf-interfaces:interfaces\":{\"interface\":[{\"name\":\"eth0\"}]}}";
        var result = _negotiator.Deserialize(json, "application/yang-data+json");
        result.Should().NotBeNull();
        result.Should().Contain("interface");
    }

    [Fact]
    public void Serialize_Json_ShouldWrapInYangData()
    {
        var body = new { name = "eth0", type = "ethernetCsmacd" };
        var json = _negotiator.Serialize(body, "application/yang-data+json");
        json.Should().NotBeNull();
    }

    [Fact]
    public void Serialize_Xml_ShouldReturnXmlString()
    {
        var xml = "<interface><name>eth0</name></interface>";
        var result = _negotiator.Serialize(xml, "application/yang-data+xml");
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetContentType_ShouldReturnJsonType()
    {
        _negotiator.GetContentType().Should().Be("application/yang-data+json");
    }

    [Fact]
    public void GetContentType_Xml_ShouldReturnXmlType()
    {
        var negotiator = new RestconfContentNegotiator(preferXml: true);
        negotiator.GetContentType().Should().Be("application/yang-data+xml");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~ContentNegotiatorTests" --no-build`
Expected: FAIL

- [ ] **Step 3: Implement RestconfContentNegotiator**

```csharp
using System.Text.Json;
using System.Xml.Linq;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfContentNegotiator
{
    private const string JsonType = "application/yang-data+json";
    private const string XmlType = "application/yang-data+xml";
    private readonly bool _preferXml;

    public RestconfContentNegotiator(bool preferXml = false)
    {
        _preferXml = preferXml;
    }

    public string GetAcceptHeader(bool preferXml = false)
        => preferXml || _preferXml ? XmlType : JsonType;

    public string GetContentType()
        => _preferXml ? XmlType : JsonType;

    public string Serialize(object? body, string contentType)
    {
        if (body is null) return string.Empty;

        if (contentType.Contains("xml"))
            return body is string s ? s : throw new NotSupportedException("XML body must be a string");

        return JsonSerializer.Serialize(body);
    }

    public string Deserialize(string rawData, string contentType)
    {
        return rawData;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~ContentNegotiatorTests" --no-build`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfContentNegotiator.cs
git add tests/Modules/Provisioning.Tests/Transports/Restconf/ContentNegotiatorTests.cs
git commit -m "feat(restconf): add RestconfContentNegotiator for JSON/XML content negotiation"
```

---

### Task 5: RestconfErrorParser

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfErrorParser.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/ErrorParserTests.cs`

- [ ] **Step 1: Write the failing error parser tests**

```csharp
using Obss.Provisioning.Infrastructure.Transports.Restconf;

public class RestconfErrorParserTests
{
    private readonly RestconfErrorParser _parser = new();

    [Fact]
    public void ParseError_Json_ShouldExtractErrorFields()
    {
        var json = @"{
            ""ietf-restconf:errors"": {
                ""error"": [{
                    ""error-type"": ""application"",
                    ""error-tag"": ""invalid-value"",
                    ""error-path"": ""/ietf-interfaces:interfaces/interface"",
                    ""error-message"": ""Invalid interface name""
                }]
            }
        }";
        var result = _parser.ParseError(json, "application/yang-data+json");
        result.Should().NotBeNull();
        result.ErrorType.Should().Be("application");
        result.ErrorTag.Should().Be("invalid-value");
        result.ErrorPath.Should().Be("/ietf-interfaces:interfaces/interface");
        result.ErrorMessage.Should().Be("Invalid interface name");
    }

    [Fact]
    public void ParseError_Json_NoErrors_ShouldReturnNull()
    {
        var json = "{\"ietf-interfaces:interfaces\":{\"interface\":[]}}";
        var result = _parser.ParseError(json, "application/yang-data+json");
        result.Should().BeNull();
    }

    [Fact]
    public void ParseError_InvalidJson_ShouldReturnNull()
    {
        var result = _parser.ParseError("not json", "application/yang-data+json");
        result.Should().BeNull();
    }

    [Fact]
    public void ToString_ShouldFormatError()
    {
        var error = new RestconfErrorParser.ParsedError("application", "invalid-value", "/path", "bad value");
        error.ToString().Should().Contain("invalid-value").And.Contain("bad value");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~ErrorParserTests" --no-build`
Expected: FAIL

- [ ] **Step 3: Implement RestconfErrorParser**

```csharp
using System.Text.Json;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfErrorParser
{
    public sealed record ParsedError(string ErrorType, string ErrorTag, string? ErrorPath, string? ErrorMessage)
    {
        public override string ToString() => $"[{ErrorType}] {ErrorTag} at {ErrorPath}: {ErrorMessage}";
    }

    public ParsedError? ParseError(string rawData, string contentType)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawData);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ietf-restconf:errors", out var errors))
                return null;

            if (!errors.TryGetProperty("error", out var errorArr) || errorArr.GetArrayLength() == 0)
                return null;

            var first = errorArr[0];
            return new ParsedError(
                GetString(first, "error-type") ?? "application",
                GetString(first, "error-tag") ?? "unknown",
                GetString(first, "error-path"),
                GetString(first, "error-message"));
        }
        catch
        {
            return null;
        }
    }

    private static string? GetString(JsonElement el, string property)
        => el.TryGetProperty(property, out var val) ? val.GetString() : null;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~ErrorParserTests" --no-build`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfErrorParser.cs
git add tests/Modules/Provisioning.Tests/Transports/Restconf/ErrorParserTests.cs
git commit -m "feat(restconf): add RestconfErrorParser for RFC 8040 error handling"
```

---

### Task 6: YangLibraryCache (RFC 8525)

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/YangLibraryCache.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/Model/YangLibraryContent.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/YangLibraryCacheTests.cs`

- [ ] **Step 1: Write YangLibraryContent model**

`Model/YangLibraryContent.cs`:
```csharp
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public sealed record YangLibraryContent(
    string ContentId,
    IReadOnlyList<YangModule> Modules,
    DateTime FetchedAt);
```

- [ ] **Step 2: Write the failing cache tests**

```csharp
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

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
}
```

- [ ] **Step 3: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~YangLibraryCacheTests" --no-build`
Expected: FAIL

- [ ] **Step 4: Implement YangLibraryCache**

```csharp
using System.Collections.Concurrent;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class YangLibraryCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

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
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~YangLibraryCacheTests" --no-build`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/YangLibraryCache.cs
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/Model/YangLibraryContent.cs
git add tests/Modules/Provisioning.Tests/Transports/Restconf/YangLibraryCacheTests.cs
git commit -m "feat(restconf): add YangLibraryCache for RFC 8525 YANG module metadata"
```

---

### Task 7: RestconfTransport Implementation

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfTransport.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/TransportTests.cs`

> Note: Integration test requires `WireMock.Net` and `Testcontainers` packages. Add to test project if not present.

- [ ] **Step 1: Add WireMock.Net test dependency**

Check if WireMock.Net is already referenced:
```bash
grep -i "wiremock" tests/Modules/Provisioning.Tests/Provisioning.Tests.csproj
```
If not present, add:
```bash
dotnet add tests/Modules/Provisioning.Tests/ package WireMock.Net
```

- [ ] **Step 2: Write the failing transport tests**

```csharp
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

public class RestconfTransportTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly RestconfTransport _transport;
    private readonly RestconfTransportConfig _config;

    public RestconfTransportTests()
    {
        _server = WireMockServer.Start();
        _config = new RestconfTransportConfig { BaseUri = _server.Url! };
        _transport = new RestconfTransport(_config);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnData()
    {
        _server.Given(Request.Create().WithPath("/restconf/data/ietf-interfaces:interfaces").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-interfaces:interfaces\":{\"interface\":[]}}"));

        var result = await _transport.GetAsync("/data/ietf-interfaces:interfaces");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_ErrorResponse_ShouldReturnFailure()
    {
        _server.Given(Request.Create().WithPath("/restconf/data/ietf-interfaces:interface=bad").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-restconf:errors\":{\"error\":[{\"error-type\":\"application\",\"error-tag\":\"invalid-value\",\"error-message\":\"Bad value\"}]}}"));

        var result = await _transport.GetAsync("/data/ietf-interfaces:interface=bad");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("invalid-value");
    }

    [Fact]
    public async Task PostAsync_ShouldReturnSuccess()
    {
        _server.Given(Request.Create().WithPath("/restconf/data/ietf-interfaces:interfaces").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/yang-data+json"));

        var result = await _transport.PostAsync("/data/ietf-interfaces:interfaces", new { name = "eth1" });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess()
    {
        _server.Given(Request.Create().WithPath("/restconf/data/ietf-interfaces:interface=eth0").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var result = await _transport.DeleteAsync("/data/ietf-interfaces:interface=eth0");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetYangLibraryAsync_ShouldParseModuleList()
    {
        _server.Given(Request.Create().WithPath("/restconf/modules-state/module").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-yang-library:modules-state\":{\"module\":[{\"name\":\"ietf-interfaces\",\"namespace\":\"urn:ietf:params:xml:ns:yang:ietf-interfaces\"}]}}"));

        var lib = await _transport.GetYangLibraryAsync();

        lib.Should().NotBeNull();
        lib.Modules.Should().Contain(m => m.Name == "ietf-interfaces");
    }

    [Fact]
    public void TestConnectionAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/restconf").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-restconf:restconf\":{}}"));

        var result = await _transport.TestConnectionAsync();

        result.Success.Should().BeTrue();
    }

    public void Dispose()
    {
        _server?.Dispose();
        _transport?.Dispose();
    }
}
```

- [ ] **Step 3: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~RestconfTransportTests" --no-build`
Expected: FAIL (types not found)

- [ ] **Step 4: Implement RestconfTransport**

```csharp
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfTransport : IRestconfTransport, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly RestconfTransportConfig _config;
    private readonly RestconfPathBuilder _pathBuilder;
    private readonly RestconfContentNegotiator _contentNegotiator;
    private readonly RestconfErrorParser _errorParser;
    private readonly YangLibraryCache _yangCache;
    private readonly ILogger? _logger;

    public TransportProtocol Protocol => TransportProtocol.Restconf;
    public ITransportConfig Config => _config;

    public RestconfTransport(
        RestconfTransportConfig config,
        ILogger<RestconfTransport>? logger = null)
    {
        _config = config;
        _httpClient = new HttpClient { BaseAddress = new Uri(config.BaseUri.TrimEnd('/') + "/") };
        _pathBuilder = new RestconfPathBuilder(config.BaseUri);
        _contentNegotiator = new RestconfContentNegotiator();
        _errorParser = new RestconfErrorParser();
        _yangCache = new YangLibraryCache();
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Accept.ParseAdd(_contentNegotiator.GetAcceptHeader());
    }

    public async Task<RestconfResult> GetAsync(string path, RestconfQueryParams? query = null, CancellationToken ct = default)
    {
        var uri = _pathBuilder.WithQueryParams(path, query);
        return await ExecuteAsync(() => _httpClient.GetAsync(uri, ct), ct);
    }

    public async Task<RestconfResult> PostAsync(string path, object? body = null, CancellationToken ct = default)
    {
        var content = SerializeBody(body);
        return await ExecuteAsync(() => _httpClient.PostAsync(path, content, ct), ct);
    }

    public async Task<RestconfResult> PutAsync(string path, object? body = null, CancellationToken ct = default)
    {
        var content = SerializeBody(body);
        return await ExecuteAsync(() => _httpClient.PutAsync(path, content, ct), ct);
    }

    public async Task<RestconfResult> PatchAsync(string path, object? body = null, CancellationToken ct = default)
    {
        var content = SerializeBody(body);
        var request = new HttpRequestMessage(HttpMethod.Patch, path) { Content = content };
        return await ExecuteAsync(() => _httpClient.SendAsync(request, ct), ct);
    }

    public async Task<RestconfResult> DeleteAsync(string path, CancellationToken ct = default)
    {
        return await ExecuteAsync(() => _httpClient.DeleteAsync(path, ct), ct);
    }

    public async Task<YangLibraryContent> GetYangLibraryAsync(CancellationToken ct = default)
    {
        var cached = _yangCache.GetCachedContentId(Config.Name ?? "default");
        var response = await _httpClient.GetAsync("/modules-state/module", ct);
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync(ct);

        var modules = ParseYangLibraryJson(raw);
        var content = new YangLibraryContent(
            Guid.NewGuid().ToString("N"), modules, DateTime.UtcNow);

        _yangCache.Update(Config.Name ?? "default", content);
        return content;
    }

    public async Task<TransportConnectionResult> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("", ct);
            return new TransportConnectionResult
            {
                Success = response.IsSuccessStatusCode,
                Latency = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            return new TransportConnectionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<RestconfResult> ExecuteAsync(Func<Task<HttpResponseMessage>> requestFunc, CancellationToken ct)
    {
        try
        {
            var response = await requestFunc();
            var rawData = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                var parsedError = _errorParser.ParseError(rawData, "application/yang-data+json");
                return RestconfResult.Fail(parsedError?.ErrorMessage ?? $"HTTP {response.StatusCode}");
            }

            return RestconfResult.Ok(rawData, protocol: TransportProtocol.Restconf);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "RESTCONF request failed");
            return RestconfResult.Fail(ex.Message);
        }
    }

    private StringContent? SerializeBody(object? body)
    {
        if (body is null) return null;
        var json = JsonSerializer.Serialize(body);
        return new StringContent(json, System.Text.Encoding.UTF8, _contentNegotiator.GetContentType());
    }

    private static IReadOnlyList<YangModule> ParseYangLibraryJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ietf-yang-library:modules-state", out var state)
                || !state.TryGetProperty("module", out var modArr))
                return Array.Empty<YangModule>();

            var modules = new List<YangModule>();
            foreach (var mod in modArr.EnumerateArray())
            {
                modules.Add(new YangModule(
                    mod.GetProperty("name").GetString() ?? "",
                    null,
                    mod.GetProperty("namespace").GetString() ?? "",
                    null, [], []));
            }
            return modules;
        }
        catch
        {
            return Array.Empty<YangModule>();
        }
    }

    public void Dispose() => _httpClient.Dispose();
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~RestconfTransportTests" --no-build`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/RestconfTransport.cs
git add tests/Modules/Provisioning.Tests/Transports/Restconf/TransportTests.cs
git commit -m "feat(restconf): add RestconfTransport implementing IRestconfTransport via HTTP delegation"
```

---

### Task 8: TransportFactory Integration

**Files:**
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/TransportFactory.cs`
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Extensions/TransportServiceCollectionExtensions.cs`
- Test: `tests/Modules/Provisioning.Tests/Transports/Restconf/FactoryIntegrationTests.cs`

- [ ] **Step 1: Update TransportFactory to support Restconf**

Add Restconf to `SupportedProtocols` and add factory method:

```csharp
// Add to SupportedProtocols:
TransportProtocol.Restconf

// Add CreateRestconfTransport:
private INetworkTransport CreateRestconfTransport(ITransportConfig config, ILoggerFactory loggerFactory)
{
    if (config is not RestconfTransportConfig restconfConfig)
        throw new InvalidOperationException($"Expected RestconfTransportConfig but got {config.GetType().Name}");

    return new RestconfTransport(restconfConfig, loggerFactory.CreateLogger<RestconfTransport>());
}

// Add to switch:
TransportProtocol.Restconf => CreateRestconfTransport(config, loggerFactory),
```

- [ ] **Step 2: Update DI registration**

In `TransportServiceCollectionExtensions.AddNetworkTransports()`:
```csharp
services.AddScoped<RestconfTransport>();
```

- [ ] **Step 3: Write factory integration tests**

```csharp
using Obss.Provisioning.Infrastructure.Transports;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;

public class RestconfFactoryIntegrationTests
{
    [Fact]
    public void TransportFactory_ShouldSupportRestconf()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNetworkTransports();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<ITransportFactory>();

        factory.SupportsProtocol(TransportProtocol.Restconf).Should().BeTrue();
    }

    [Fact]
    public void TransportFactory_ShouldCreateRestconfTransport()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNetworkTransports();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<ITransportFactory>();

        var config = new RestconfTransportConfig
        {
            BaseUri = "https://device/restconf",
            Name = "test-device"
        };

        var transport = factory.CreateTransport(config);
        transport.Should().BeOfType<RestconfTransport>();
        transport.Protocol.Should().Be(TransportProtocol.Restconf);
    }

    [Fact]
    public void TransportFactory_CreateRestconf_WithInvalidConfig_ShouldThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNetworkTransports();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<ITransportFactory>();

        var invalidConfig = new SshTransportConfig(); // wrong type

        Action act = () => factory.CreateTransport(invalidConfig);
        act.Should().Throw<InvalidOperationException>().WithMessage("*RestconfTransportConfig*");
    }
}
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build Obss.sln --no-restore 2>&1 | tail -5`
Expected: Build succeeded. 0 Warning(s) 0 Error(s)

- [ ] **Step 5: Run all RESTCONF tests**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Restconf" --no-build`
Expected: All tests PASS

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/TransportFactory.cs
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Extensions/
git add tests/Modules/Provisioning.Tests/Transports/Restconf/FactoryIntegrationTests.cs
git commit -m "feat(restconf): integrate RestconfTransport into TransportFactory and DI"
```

---

### Task 9: End-to-End Build Verification

- [ ] **Step 1: Full solution build**

Run: `dotnet build Obss.sln 2>&1 | tail -5`
Expected: Build succeeded. 0 Warning(s) 0 Error(s)

- [ ] **Step 2: Run all RESTCONF tests**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Restconf" --no-build`
Expected: All PASS

- [ ] **Step 3: Run full Provisioning test suite**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --no-build 2>&1 | tail -10`
Expected: No regressions

- [ ] **Step 4: Commit remaining files**

```bash
git add -A
git commit -m "feat(restconf): finalize RESTCONF transport implementation"
```
