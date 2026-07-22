# RESTCONF Transport for OBSS Provisioning

## Overview

Add a dedicated RESTCONF transport (RFC 8040) to the Provisioning module's transport layer, enabling
YANG-based device management for Cisco, Juniper, Nokia, and other RESTCONF-capable network elements.

## Architecture

```
IRestTransport (existing, HTTP)
    ↑ delegates
IRestconfTransport (new)
    ↑ implements
RestconfTransport
```

The RESTCONF transport lives in `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Transports/Restconf/`
following the existing pattern (SNMP, SSH, NETCONF, REST).

## Component Tree

```
Transports/
├── Restconf/
│   ├── IRestconfTransport.cs
│   ├── RestconfTransport.cs
│   ├── RestconfTransportConfig.cs
│   ├── RestconfPathBuilder.cs
│   ├── RestconfContentNegotiator.cs
│   ├── RestconfErrorParser.cs
│   ├── YangLibraryCache.cs
│   └── Model/
│       ├── RestconfResource.cs
│       ├── RestconfResult.cs
│       ├── YangModule.cs
│       └── YangLibraryContent.cs
├── Common/
│   ├── ITransportFactory.cs         (updated)
│   └── TransportServiceCollectionExtensions.cs  (updated)
```

## Interfaces

### IRestconfTransport

```csharp
public interface IRestconfTransport
{
    Task<RestconfResult<T>> GetAsync<T>(string path, RestconfQueryParams? query = null);
    Task<RestconfResult<T>> PostAsync<T>(string path, object body);
    Task<RestconfResult<T>> PutAsync<T>(string path, object body);
    Task<RestconfResult<T>> PatchAsync<T>(string path, object body);
    Task<RestconfResult> DeleteAsync(string path);
    Task<YangLibraryContent> GetYangLibraryAsync();
    string BaseUri { get; }
}
```

### RestconfResult

```csharp
public sealed record RestconfResult(bool IsSuccess, object? Data, Error? Error);
public sealed record RestconfResult<T>(bool IsSuccess, T? Data, Error? Error);
```

### RestconfTransportConfig (extends TransportConfigBase)

- `BaseUri` — device RESTCONF root URL (e.g. `https://10.0.0.1/restconf`)
- Content type preference: JSON or XML
- YANG library cache TTL (default: 300s)
- Connection timeout, TLS settings (inherited from TransportConfigBase)

## Data Flow

### GET request (e.g., interfaces list)

1. Adapter calls `transport.GetAsync<Interface[]>("/data/ietf-interfaces:interfaces/interface", new { depth = 2 })`
2. `RestconfTransport` checks YANG library cache for `ietf-interfaces` module
3. `RestconfContentNegotiator` sets `Accept: application/yang-data+json`
4. Delegates to `IRestTransport.GetAsync(uri, headers)`
5. On success: strips YANG-data envelope, deserializes to `Interface[]`
6. On error: `RestconfErrorParser` extracts RFC 8040 error fields
7. Returns `RestconfResult`

### Content Negotiation

- `Accept` header: `application/yang-data+json` (primary) or `application/yang-data+xml`
- Response `Content-Type` determines deserialization path
- JSON: `System.Text.Json`
- XML: `System.Xml.Linq` / `XmlSerializer`

## YANG Library Cache (RFC 8525)

```csharp
public sealed class YangLibraryCache
{
    public Task<YangLibraryContent> GetOrFetchAsync(string deviceId, IRestconfTransport transport);
    public void Invalidate(string deviceId);
    public bool IsModuleSupported(string deviceId, string moduleName);
}

public sealed record YangLibraryContent(
    string ContentId,
    IReadOnlyList<YangModule> Modules,
    DateTime FetchedAt);

public sealed record YangModule(
    string Name,
    string? Revision,
    string Namespace,
    string? SchemaUri,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> Deviations);
```

**Flow:**
1. First access: GET `{+restconf}/modules-state/module` → parse module list → cache
2. Subsequent: compare cached `content-id` vs device's current `content-id` (lightweight check)
3. If changed: re-fetch full module list
4. Module validation: before any request, warn if target module not in device's module set (non-blocking)

## Error Handling (RFC 8040 §7.1)

Device error response body:
```json
{
  "ietf-restconf:errors": {
    "error": [{
      "error-type": "application",
      "error-tag": "invalid-value",
      "error-path": "/ietf-interfaces:interfaces/interface",
      "error-message": "Invalid interface name"
    }]
  }
}
```

`RestconfErrorParser` extracts these and maps to `RestconfResult.Failure` with structured error.

## Transport Factory Integration

- `ITransportFactory.CreateRestconfAsync(string name)` — new method
- Config section: `"Transports:Restconf:{name}"` → `RestconfTransportConfig`
- Shares `IHttpClientFactory` pool with `RestTransport`
- Registered in `TransportServiceCollectionExtensions.AddNetworkTransports()`

## File List

| File | Lines | Purpose |
|------|-------|---------|
| `Model/RestconfResult.cs` | ~30 | Result types |
| `Model/RestconfResource.cs` | ~20 | Resource/query param types |
| `Model/YangModule.cs` | ~15 | Module metadata record |
| `Model/YangLibraryContent.cs` | ~15 | Library content record |
| `IRestconfTransport.cs` | ~20 | Public interface |
| `RestconfTransportConfig.cs` | ~25 | Configuration |
| `RestconfTransport.cs` | ~180 | Core implementation |
| `RestconfPathBuilder.cs` | ~80 | URI construction |
| `RestconfContentNegotiator.cs` | ~60 | Content type handling |
| `RestconfErrorParser.cs` | ~60 | RFC 8040 error parsing |
| `YangLibraryCache.cs` | ~100 | RFC 8525 metadata cache |
| `ITransportFactory.cs` | ~10 | Updated (1 new method) |
| `TransportServiceCollectionExtensions.cs` | ~10 | Updated DI registration |
| **Total** | **~625** | |

## Testing

| Test Type | Scope |
|-----------|-------|
| Unit: PathBuilder | Path construction, query params, edge cases |
| Unit: ErrorParser | Error envelope parsing, all error types |
| Unit: ContentNegotiator | Content type selection, serialization |
| Unit: YangLibraryCache | Cache hit/miss, invalidation, content-id check |
| Integration | WireMock.NET mock RESTCONF server: full CRUD cycle, error responses, YANG library query |
