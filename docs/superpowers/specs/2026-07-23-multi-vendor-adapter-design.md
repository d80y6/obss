# Multi-Vendor Router Adapters (Cisco, Juniper, Nokia) — Design Spec

> **Phase 2 of P2-level work.** Builds on the RESTCONF transport (Phase 1) to add vendor-specific provisioning adapters for Cisco IOS-XE, Juniper JunOS, and Nokia SR OS routers/switches.

## Architecture Overview

Each vendor adapter follows the exact same two-tier pattern as the existing Huawei/ZTE adapters in `Adapters/`:

- **Tier 1 — Vendor-specific interface + implementation:** Rich typed operations (ConfigureInterface, ConfigureBgp, GetDeviceStatus, etc.) using RESTCONF transport. Includes real implementation, simulator, config, health check, and constants.
- **Tier 2 — IProvisioningAdapter wrapper:** Bridges `ProvisioningTask` → vendor-specific adapter method, exactly like `HuaweiProvisioningAdapter` / `NetworkProvisioningAdapter`.

All three vendors share the same RESTCONF transport (`IRestconfTransport` from Phase 1) — only the YANG module paths and JSON payload shapes differ per vendor.

## Directory Structure

```
Adapters/
├── Common/                              # Existing shared types (unchanged)
│   ├── AdapterConfigurationBase.cs
│   ├── AdapterConstants.cs              # MODIFIED: add CISCO_ROUTER/JUNIPER_ROUTER/NOKIA_ROUTER
│   ├── AdapterHealthStatus.cs
│   ├── AdapterRegistry.cs               # MODIFIED: register new adapters
│   ├── AdapterResult.cs                 # Existing AdapterResult + AdapterResult<T>
│   ├── IDeviceConnectionManager.cs
│   └── DeviceConnectionManager.cs
├── Cisco/
│   ├── ICiscoRouterAdapter.cs           # ~15 typed operations
│   ├── CiscoRouterAdapter.cs            # Real impl via IRestconfTransport
│   ├── CiscoRouterSimulator.cs          # In-memory simulator
│   ├── CiscoAdapterConfig.cs            # Config extending AdapterConfigurationBase
│   ├── CiscoAdapterConstants.cs         # YANG paths, operation names, defaults
│   ├── CiscoRouterHealthCheck.cs        # IHealthCheck
│   ├── CiscoProvisioningAdapter.cs      # IProvisioningAdapter wrapper
│   └── Models/
│       ├── InterfaceConfig.cs
│       ├── InterfaceStatus.cs
│       ├── BgpConfig.cs
│       ├── OspfConfig.cs
│       ├── StaticRoute.cs
│       ├── SystemConfig.cs
│       ├── AclConfig.cs
│       ├── DeviceStatus.cs
│       ├── DeviceInventory.cs
│       └── AlarmInfo.cs
├── Juniper/
│   ├── IJuniperRouterAdapter.cs         # Same shape, Juniper-specific YANG paths
│   ├── JuniperRouterAdapter.cs
│   ├── JuniperRouterSimulator.cs
│   ├── JuniperAdapterConfig.cs
│   ├── JuniperAdapterConstants.cs
│   ├── JuniperRouterHealthCheck.cs
│   ├── JuniperProvisioningAdapter.cs
│   └── Models/                          # Same model types (YANG payloads differ)
│       ├── InterfaceConfig.cs
│       ├── ...
│       └── AlarmInfo.cs
└── Nokia/
    ├── INokiaRouterAdapter.cs           # Same shape, Nokia-specific YANG paths
    ├── NokiaRouterAdapter.cs
    ├── NokiaRouterSimulator.cs
    ├── NokiaAdapterConfig.cs
    ├── NokiaAdapterConstants.cs
    ├── NokiaRouterHealthCheck.cs
    ├── NokiaProvisioningAdapter.cs
    └── Models/
        ├── InterfaceConfig.cs
        ├── ...
        └── AlarmInfo.cs
```

## Vendor Interface (Tier 1)

Each vendor adapter exposes the same ~15 operations. Shown for Cisco below; Juniper and Nokia are structurally identical with vendor-specific type names and YANG paths.

```csharp
public interface ICiscoRouterAdapter
{
    string AdapterName { get; }

    // Interface Management
    Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config);
    Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName);
    Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync();
    Task<AdapterResult> DeleteInterfaceAsync(string interfaceName);

    // Routing Protocols
    Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config);
    Task<AdapterResult<BgpConfig>> GetBgpConfigAsync();
    Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config);
    Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route);
    Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync();

    // System Configuration
    Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config);
    Task<AdapterResult<SystemConfig>> GetSystemConfigAsync();

    // Security
    Task<AdapterResult<AclConfig>> ConfigureAclAsync(AclConfig config);

    // Device Status
    Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync();
    Task<AdapterResult<DeviceInventory>> GetInventoryAsync();

    // Monitoring
    Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync();
}
```

All methods return `AdapterResult<T>` (already exists in `Adapters.Common`) or `AdapterResult` for void operations.

## Model Types (per vendor, in Models/)

Each vendor's model types capture the YANG data structures for that vendor. Example for Cisco:

```csharp
// Adapters/Cisco/Models/InterfaceConfig.cs
public sealed record InterfaceConfig(
    string Name,
    string Type,                    // "GigabitEthernet", "Loopback", "Vlan", etc.
    string? Description,
    string? IpAddress,
    int? PrefixLength,
    bool? AdminUp,
    int? Mtu,
    string? VlanId,
    IReadOnlyList<string>? SwitchportModes);

// Adapters/Cisco/Models/BgpConfig.cs
public sealed record BgpConfig(
    int AsNumber,
    string? RouterId,
    IReadOnlyList<BgpNeighbor>? Neighbors,
    IReadOnlyList<BgpNetwork>? Networks);

// Adapters/Cisco/Models/DeviceStatus.cs
public sealed record DeviceStatus(
    string Hostname,
    string SoftwareVersion,
    string Model,
    string Uptime,
    double CpuUtilization,
    double MemoryUtilization,
    IReadOnlyList<InterfaceStatus> Interfaces);

// ... (InterfaceStatus, OspfConfig, StaticRoute, SystemConfig, AclConfig, DeviceInventory, AlarmInfo follow same pattern)
```

Each model type includes a `ToYangPayload()` method or a mapping function that transforms the typed record into the vendor-specific JSON structure expected by the YANG module.

## Vendor YANG Module Mapping

```
Vendor      | Interface YANG                         | BGP YANG                          | System YANG
------------|----------------------------------------|-----------------------------------|------------------------
Cisco       | Cisco-IOS-XE-native:native/interface   | Cisco-IOS-XE-bgp:bgp              | Cisco-IOS-XE-native:native
Juniper     | junos-configuration:configuration/interfaces | junos-configuration:configuration/protocols/bgp | junos-configuration:configuration/system
Nokia       | nokia-conf-port:configure port         | nokia-conf-router:configure router/*/bgp | nokia-conf-system:configure system
```

## Real Adapter Implementation Pattern

Each real adapter follows the same structure:

1. Constructor takes vendor-specific config + `IRestconfTransport` (from DI)
2. Each operation builds the RESTCONF path using vendor-specific YANG module prefixes
3. Calls `_transport.GetAsync/PutAsync/PostAsync(path, payload)` 
4. Maps response JSON to typed model record
5. Returns `AdapterResult<T>.Success(...)` or `AdapterResult<T>.Failure(...)`

```csharp
public sealed class CiscoRouterAdapter : ICiscoRouterAdapter
{
    private readonly IRestconfTransport _transport;
    private readonly CiscoAdapterConfig _config;

    public CiscoRouterAdapter(CiscoAdapterConfig config, IRestconfTransport transport)
    {
        _config = config;
        _transport = transport;
    }

    public async Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        // RESTCONF PUT to create/replace the interface resource
        var path = $"/data/Cisco-IOS-XE-native:native/interface/{config.Type}={config.Name}";
        var payload = new { /* YANG-shaped JSON from config */ };
        var result = await _transport.PutAsync(path, payload);
        return result.Success
            ? AdapterResult<InterfaceConfig>.Success(config)
            : AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage);
    }

    public async Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        var path = "/data/Cisco-IOS-XE-native:native";
        var result = await _transport.GetAsync(path);
        if (!result.Success)
            return AdapterResult<DeviceStatus>.Failure(result.ErrorMessage);
        return AdapterResult<DeviceStatus>.Success(ParseDeviceStatus(result.Data));
    }
    // ...
}
```

## Simulator Pattern

Each vendor simulator (`CiscoRouterSimulator`, `JuniperRouterSimulator`, `NokiaRouterSimulator`) follows the `HuaweiBroadbandSimulator` pattern:

- In-memory `ConcurrentDictionary` per resource type
- Configurable failure rate (`FailureRate` double 0.0–1.0 via `Random.Shared`)
- Input validation (required fields, range checks)
- Returns realistic YANG-shaped JSON responses
- Each implements a `ClearState()` method for test isolation

## IProvisioningAdapter Wrappers (Tier 2)

Each vendor gets a provisioning adapter following the `HuaweiProvisioningAdapter` pattern. For example `CiscoProvisioningAdapter : IProvisioningAdapter`:

- `AdapterName` returns `"CISCO_ROUTER"`
- `ExecuteAsync(ProvisioningTask task)` deserializes `task.Configuration` JSON, switches on `task.TaskType` to call the appropriate `ICiscoRouterAdapter` method
- `CompensateAsync(ProvisioningTask task)` provides rollback for applicable operations

## Dispatch Integration

### AdapterConstants.cs additions

```csharp
public static class AdapterConstants
{
    // Existing technology types...
    public const string TechnologyCiscoRouter = "cisco_router";
    public const string TechnologyJuniperRouter = "juniper_router";
    public const string TechnologyNokiaRouter = "nokia_router";

    // Existing adapter names...
    public const string AdapterCiscoRouter = "CISCO_ROUTER";
    public const string AdapterJuniperRouter = "JUNIPER_ROUTER";
    public const string AdapterNokiaRouter = "NOKIA_ROUTER";
}
```

### ProvisioningTaskType additions

New `ProvisioningTaskType` values (in `ProvisioningTaskType.cs`):
- `RouterInterfaceConfig`, `RouterBgpConfig`, `RouterOspfConfig`, `RouterStaticRouteConfig`, `RouterSystemConfig`, `RouterAclConfig`, `GetRouterStatus`, `GetRouterInventory`, `GetRouterAlarms`

### ProvisioningJobCoordinator dispatch changes

`ResolveAdapterName(ProvisioningTaskType taskType, string? assignedTo, JsonDocument? config)` is overloaded to accept the full task context. For router task types, it reads `config["vendor"]` or falls back to `assignedTo` to determine the vendor:

```csharp
private static string ResolveAdapterName(ProvisioningTaskType taskType, string? assignedTo = null, JsonDocument? config = null)
{
    // ... existing switch arms ...

    // Router tasks — dispatch to vendor-specific adapter
    ProvisioningTaskType.RouterInterfaceConfig or ProvisioningTaskType.RouterBgpConfig
        or ProvisioningTaskType.RouterOspfConfig or ProvisioningTaskType.RouterStaticRouteConfig
        or ProvisioningTaskType.RouterSystemConfig or ProvisioningTaskType.RouterAclConfig
        or ProvisioningTaskType.GetRouterStatus or ProvisioningTaskType.GetRouterInventory
        or ProvisioningTaskType.GetRouterAlarms => ResolveVendorAdapter(assignedTo, config),
}
```

Where `ResolveVendorAdapter` extracts the vendor from `config["vendor"]` (e.g., `"cisco"`, `"juniper"`, `"nokia"`) or `assignedTo` and returns the matching adapter name (`"CISCO_ROUTER"`, `"JUNIPER_ROUTER"`, `"NOKIA_ROUTER"`), defaulting to `"CISCO_ROUTER"`.

### Module Registration

The `ProvisioningModuleRegistration.cs` or equivalent DI wiring:
- Registers all 3 vendor adapter interfaces + real implementations as scoped
- Registers all 3 simulators (for test/simulation mode)
- Registers all 3 `IProvisioningAdapter` wrappers
- Registers all 3 health checks
- Updates `AdapterRegistry` with new technology→adapter mappings

## Testing Strategy

### Unit Tests (per vendor)
- **Model tests** — record equality, serialization, `ToYangPayload()` mapping
- **Simulator tests** — state management, failure rate, validation, `ClearState()`
- **Config tests** — defaults, validation
- **ProvisioningAdapter tests** — task dispatch, error handling

### Integration Tests
- **Real adapter tests** — using WireMock.NET (same pattern as `RestconfTransportTests`): mock RESTCONF responses for each operation, verify adapter methods parse correctly
- **Health check tests** — verify health check probes RESTCONF root + YANG library

### Test files per vendor (3 vendors × ~8 test files = ~24 total)
```
tests/Modules/Provisioning.Tests/Adapters/Cisco/
├── CiscoRouterAdapterTests.cs
├── CiscoRouterSimulatorTests.cs
├── CiscoAdapterConfigTests.cs
├── CiscoProvisioningAdapterTests.cs
├── CiscoRouterHealthCheckTests.cs
└── Models/
    ├── InterfaceConfigTests.cs
    ├── BgpConfigTests.cs
    └── DeviceStatusTests.cs
```
Same structure for `Juniper/` and `Nokia/`.

## Implementation Order

The implementation is large enough that each vendor should be implemented in its own cycle (Cisco first as reference, then Juniper, then Nokia), but within each vendor the pattern is mechanical:

1. **Model types** — all records in `Models/` with tests
2. **Config + Constants** — config record, constants
3. **Interface** — `ICiscoRouterAdapter` (interface only)
4. **Simulator** — in-memory state with tests
5. **Real adapter** — implementation using `IRestconfTransport`, with WireMock integration tests
6. **ProvisioningAdapter wrapper** — bridges ProvisioningTask
7. **Health check** — IHealthCheck
8. **Dispatch integration** — AdapterConstants, ProvisioningTaskType, ResolveAdapterName
9. **DI wiring** — module registration, adapter registry update

Juniper and Nokia are then mechanical copies of Cisco with adjusted YANG paths and model types.
