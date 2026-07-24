# Multi-Vendor Router Adapters Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Cisco, Juniper, and Nokia router provisioning adapters using the existing RESTCONF transport.

**Architecture:** Each vendor adapter follows the existing two-tier pattern (Huawei/ZTE): vendor-specific interface with ~15 typed operations, real implementation via `IRestconfTransport`, in-memory simulator, config, health check, and `IProvisioningAdapter` wrapper. Shared dispatch integration adds router `ProvisioningTaskType` values and vendor-aware `ResolveAdapterName`.

**Tech Stack:** .NET 9, RESTCONF transport (RFC 8040), xUnit + FluentAssertions, WireMock.NET, System.Text.Json

---

### Task 1: Router Dispatch Infrastructure

**Files:**
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Domain/ValueObjects/ProvisioningTaskType.cs`
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Common/AdapterConstants.cs`
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Application/Services/ProvisioningJobCoordinator.cs`

**Test file:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/RouterDispatchTests.cs`

- [ ] **Step 1: Write the failing dispatch tests**

```csharp
using FluentAssertions;
using Obss.Provisioning.Domain.ValueObjects;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters;

public class RouterDispatchTests
{
    [Fact]
    public void AdapterConstants_ShouldDefineRouterTechnologies()
    {
        AdapterConstants.TechnologyCiscoRouter.Should().Be("cisco_router");
        AdapterConstants.TechnologyJuniperRouter.Should().Be("juniper_router");
        AdapterConstants.TechnologyNokiaRouter.Should().Be("nokia_router");
    }

    [Fact]
    public void AdapterConstants_ShouldDefineRouterAdapterNames()
    {
        AdapterConstants.AdapterCiscoRouter.Should().Be("CISCO_ROUTER");
        AdapterConstants.AdapterJuniperRouter.Should().Be("JUNIPER_ROUTER");
        AdapterConstants.AdapterNokiaRouter.Should().Be("NOKIA_ROUTER");
    }

    [Fact]
    public void ProvisioningTaskType_ShouldHaveRouterValues()
    {
        ((int)ProvisioningTaskType.RouterInterfaceConfig).Should().BeGreaterThan(0);
        ((int)ProvisioningTaskType.RouterBgpConfig).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ResolveAdapterName_WithConfigVendor_ShouldReturnCorrectAdapter()
    {
        var config = JsonDocument.Parse("{\"vendor\":\"cisco\"}");
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.RouterInterfaceConfig, null, config);
        name.Should().Be("CISCO_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_WithJuniperConfig_ShouldReturnJuniperAdapter()
    {
        var config = JsonDocument.Parse("{\"vendor\":\"juniper\"}");
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.RouterBgpConfig, null, config);
        name.Should().Be("JUNIPER_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_WithNokiaConfig_ShouldReturnNokiaAdapter()
    {
        var config = JsonDocument.Parse("{\"vendor\":\"nokia\"}");
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.GetRouterStatus, null, config);
        name.Should().Be("NOKIA_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_WithAssignedTo_ShouldResolveVendor()
    {
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.RouterInterfaceConfig, "cisco-router-01", null);
        name.Should().Be("CISCO_ROUTER");
    }

    [Fact]
    public void ResolveAdapterName_NonRouterTask_ShouldUseExistingMapping()
    {
        var name = ProvisioningJobCoordinator.ResolveAdapterName(
            ProvisioningTaskType.FtthOntProvision, null, null);
        name.Should().Be("HUAWEI_BROADBAND");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~RouterDispatch" --no-build`
Expected: FAIL (types/constants not found)

- [ ] **Step 3: Add router task types to ProvisioningTaskType.cs**

```csharp
public enum ProvisioningTaskType
{
    // ... existing 45 values unchanged ...
    RouterInterfaceConfig = 46,
    RouterBgpConfig = 47,
    RouterOspfConfig = 48,
    RouterStaticRouteConfig = 49,
    RouterSystemConfig = 50,
    RouterAclConfig = 51,
    GetRouterStatus = 52,
    GetRouterInventory = 53,
    GetRouterAlarms = 54
}
```

- [ ] **Step 4: Add router constants to AdapterConstants.cs**

Note: `AdapterConstants.cs` is in `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Common/AdapterConstants.cs`. Read the existing file first, then add:

```csharp
// Technology types (add to existing class)
public const string TechnologyCiscoRouter = "cisco_router";
public const string TechnologyJuniperRouter = "juniper_router";
public const string TechnologyNokiaRouter = "nokia_router";

// Adapter names (add to existing class)
public const string AdapterCiscoRouter = "CISCO_ROUTER";
public const string AdapterJuniperRouter = "JUNIPER_ROUTER";
public const string AdapterNokiaRouter = "NOKIA_ROUTER";
```

- [ ] **Step 5: Update ProvisioningJobCoordinator.cs — overload ResolveAdapterName**

Read the existing file first. The existing `ResolveAdapterName` method only takes `ProvisioningTaskType`. We need to:
1. Keep the existing single-parameter overload for backward compatibility
2. Add a new overload that also accepts `string? assignedTo` and `JsonDocument? config`
3. Add a `ResolveVendorAdapter` helper

```csharp
using System.Text.Json; // ADD to top of file

// Existing method stays unchanged for callers that pass taskType only
private static string ResolveAdapterName(ProvisioningTaskType taskType)
    => ResolveAdapterName(taskType, null, null);

// New overload with vendor context
private static string ResolveAdapterName(ProvisioningTaskType taskType, string? assignedTo, JsonDocument? config)
{
    // Check router task types first
    if (IsRouterTask(taskType))
        return ResolveVendorAdapter(assignedTo, config);

    // Forward existing logic for non-router tasks
    return taskType switch
    {
        // ... same switch as before (copy existing body unchanged) ...
    };
}

private static bool IsRouterTask(ProvisioningTaskType taskType)
    => taskType is ProvisioningTaskType.RouterInterfaceConfig
        or ProvisioningTaskType.RouterBgpConfig
        or ProvisioningTaskType.RouterOspfConfig
        or ProvisioningTaskType.RouterStaticRouteConfig
        or ProvisioningTaskType.RouterSystemConfig
        or ProvisioningTaskType.RouterAclConfig
        or ProvisioningTaskType.GetRouterStatus
        or ProvisioningTaskType.GetRouterInventory
        or ProvisioningTaskType.GetRouterAlarms;

private static string ResolveVendorAdapter(string? assignedTo, JsonDocument? config)
{
    // Check config["vendor"] first
    if (config?.RootElement.TryGetProperty("vendor", out var vendorProp) == true)
    {
        var vendor = vendorProp.GetString();
        if (string.Equals(vendor, "juniper", StringComparison.OrdinalIgnoreCase))
            return "JUNIPER_ROUTER";
        if (string.Equals(vendor, "nokia", StringComparison.OrdinalIgnoreCase))
            return "NOKIA_ROUTER";
    }

    // Fallback: check assignedTo for vendor hints
    if (!string.IsNullOrEmpty(assignedTo))
    {
        var lower = assignedTo.ToLowerInvariant();
        if (lower.Contains("juniper")) return "JUNIPER_ROUTER";
        if (lower.Contains("nokia")) return "NOKIA_ROUTER";
    }

    // Default to Cisco
    return "CISCO_ROUTER";
}
```

Also update the call sites in `ExecuteJobAsync` (around lines 110 and 186) to pass the full task context:

```csharp
// Change from:
var adapterName = ResolveAdapterName(task.TaskType);
// To:
var adapterName = ResolveAdapterName(task.TaskType, task.AssignedTo, task.Configuration);
```

- [ ] **Step 6: Run test to verify it passes**

Run: `dotnet build Obss.sln --no-restore | tail -5 && dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~RouterDispatch" --no-build`
Expected: Build succeeded + 7/7 tests PASS

- [ ] **Step 7: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Domain/ValueObjects/ProvisioningTaskType.cs
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Common/AdapterConstants.cs
git add src/Modules/Provisioning/Obss.Provisioning.Application/Services/ProvisioningJobCoordinator.cs
git add tests/Modules/Provisioning.Tests/Adapters/RouterDispatchTests.cs
git commit -m "feat(routers): add router dispatch infrastructure (task types, constants, vendor-aware adapter resolution)"
```

---

### Task 2: Cisco Router — Model Types

**Files (create all):**
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/InterfaceConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/InterfaceStatus.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/BgpConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/OspfConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/StaticRoute.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/SystemConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/AclConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/DeviceStatus.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/DeviceInventory.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/AlarmInfo.cs`

**Test file:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Cisco/Models/CiscoModelTests.cs`

- [ ] **Step 1: Write the failing model tests**

```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco.Models;

public class CiscoModelTests
{
    [Fact]
    public void InterfaceConfig_ShouldPreserveAllProperties()
    {
        var config = new InterfaceConfig(
            "GigabitEthernet0/0/0", "GigabitEthernet",
            "Uplink", "10.0.0.1", 24, true, 1500, "100", new[] { "access" });
        config.Name.Should().Be("GigabitEthernet0/0/0");
        config.Type.Should().Be("GigabitEthernet");
        config.Description.Should().Be("Uplink");
        config.IpAddress.Should().Be("10.0.0.1");
        config.PrefixLength.Should().Be(24);
        config.AdminUp.Should().BeTrue();
        config.Mtu.Should().Be(1500);
        config.VlanId.Should().Be("100");
        config.SwitchportModes.Should().Contain("access");
    }

    [Fact]
    public void BgpConfig_ShouldPreserveProperties()
    {
        var neighbors = new[] { new BgpNeighbor("10.0.0.2", 65002, "ebgp") };
        var networks = new[] { new BgpNetwork("192.168.0.0", 24) };
        var bgp = new BgpConfig(65001, "10.0.0.1", neighbors, networks);
        bgp.AsNumber.Should().Be(65001);
        bgp.RouterId.Should().Be("10.0.0.1");
        bgp.Neighbors.Should().Contain(n => n.RemoteAs == 65002);
        bgp.Networks.Should().Contain(n => n.Prefix == "192.168.0.0");
    }

    [Fact]
    public void DeviceStatus_ShouldPreserveProperties()
    {
        var ifaces = new[] { new InterfaceStatus("GigabitEthernet0/0/0", "up", "up", 1000000) };
        var status = new DeviceStatus("router01", "17.9.1", "ISR4451", "100 days", 45.5, 60.2, ifaces);
        status.Hostname.Should().Be("router01");
        status.CpuUtilization.Should().Be(45.5);
        status.Interfaces.Should().Contain(i => i.OperStatus == "up");
    }

    [Fact]
    public void AclConfig_ShouldPreserveEntries()
    {
        var entries = new[] { new AclEntry(10, "permit", "10.0.0.0", "0.0.0.255", null, null) };
        var acl = new AclConfig("ACL-ALLOW-MGMT", entries);
        acl.Name.Should().Be("ACL-ALLOW-MGMT");
        acl.Entries.Should().Contain(e => e.Action == "permit");
    }

    [Fact]
    public void StaticRoute_ShouldPreserveProperties()
    {
        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, "GigabitEthernet0/0/0");
        route.Prefix.Should().Be("0.0.0.0/0");
        route.NextHop.Should().Be("10.0.0.1");
        route.AdministrativeDistance.Should().Be(1);
    }

    [Fact]
    public void OspfConfig_ShouldPreserveProperties()
    {
        var areas = new[] { new OspfArea(0, new[] { "GigabitEthernet0/0/0" }) };
        var ospf = new OspfConfig(100, "10.0.0.1", areas);
        ospf.ProcessId.Should().Be(100);
        ospf.Areas.Should().Contain(a => a.AreaId == 0);
    }

    [Fact]
    public void SystemConfig_ShouldPreserveProperties()
    {
        var ntpServers = new[] { "10.0.0.10", "10.0.0.11" };
        var sys = new SystemConfig("router01", "example.com", ntpServers, new[] { "10.0.0.20" }, "obss-admin");
        sys.Hostname.Should().Be("router01");
        sys.NtpServers.Should().Contain("10.0.0.10");
    }

    [Fact]
    public void DeviceInventory_ShouldPreserveProperties()
    {
        var inv = new DeviceInventory("ISR4451-X/K9", "FTX2042AABB", "17.9.1", "2048MB", "16GB",
            new[] { new HardwareComponent("Power Supply 0", "PWR-4451-AC", "OK") });
        inv.Model.Should().Be("ISR4451-X/K9");
        inv.Components.Should().Contain(c => c.Status == "OK");
    }

    [Fact]
    public void AlarmInfo_ShouldPreserveProperties()
    {
        var alarm = new AlarmInfo("2026-07-23T10:00:00Z", "critical", "Interface down", "GigabitEthernet0/0/0");
        alarm.Severity.Should().Be("critical");
        alarm.Description.Should().Be("Interface down");
    }

    [Fact]
    public void BgpNeighbor_ShouldPreserveProperties()
    {
        var neighbor = new BgpNeighbor("10.0.0.2", 65002, "ebgp");
        neighbor.RemoteAddress.Should().Be("10.0.0.2");
        neighbor.RemoteAs.Should().Be(65002);
    }

    [Fact]
    public void InterfaceStatus_ShouldPreserveProperties()
    {
        var status = new InterfaceStatus("GigabitEthernet0/0/0", "up", "up", 1000000);
        status.InterfaceName.Should().Be("GigabitEthernet0/0/0");
        status.AdminStatus.Should().Be("up");
        status.OperStatus.Should().Be("up");
        status.Speed.Should().Be(1000000);
    }

    [Fact]
    public void AclEntry_ShouldPreserveProperties()
    {
        var entry = new AclEntry(10, "deny", "10.0.0.0", "0.0.0.255", "any", "log");
        entry.Sequence.Should().Be(10);
        entry.Action.Should().Be("deny");
        entry.Log.Should().Be("log");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~CiscoModelTests" --no-build`
Expected: FAIL (types not found)

- [ ] **Step 3: Create all 10 Cisco model records**

Create directory `Adapters/Cisco/Models/` and add each file:

`Models/InterfaceConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record InterfaceConfig(
    string Name,
    string Type,
    string? Description,
    string? IpAddress,
    int? PrefixLength,
    bool? AdminUp,
    int? Mtu,
    string? VlanId,
    IReadOnlyList<string>? SwitchportModes);
```

`Models/InterfaceStatus.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record InterfaceStatus(
    string InterfaceName,
    string AdminStatus,
    string OperStatus,
    long Speed);
```

`Models/BgpConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record BgpNeighbor(string RemoteAddress, int RemoteAs, string? PeerGroup);

public sealed record BgpNetwork(string Prefix, int PrefixLength);

public sealed record BgpConfig(
    int AsNumber,
    string? RouterId,
    IReadOnlyList<BgpNeighbor>? Neighbors,
    IReadOnlyList<BgpNetwork>? Networks);
```

`Models/OspfConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record OspfArea(int AreaId, IReadOnlyList<string>? Interfaces);

public sealed record OspfConfig(
    int ProcessId,
    string? RouterId,
    IReadOnlyList<OspfArea>? Areas);
```

`Models/StaticRoute.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record StaticRoute(
    string Prefix,
    string NextHop,
    int? AdministrativeDistance,
    string? Interface);
```

`Models/SystemConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record SystemConfig(
    string? Hostname,
    string? DomainName,
    IReadOnlyList<string>? NtpServers,
    IReadOnlyList<string>? DnsServers,
    string? Username);
```

`Models/AclConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record AclEntry(
    int Sequence,
    string Action,
    string Source,
    string SourceWildcard,
    string? Destination,
    string? Log);

public sealed record AclConfig(
    string Name,
    IReadOnlyList<AclEntry> Entries);
```

`Models/DeviceStatus.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record DeviceStatus(
    string Hostname,
    string SoftwareVersion,
    string Model,
    string Uptime,
    double CpuUtilization,
    double MemoryUtilization,
    IReadOnlyList<InterfaceStatus> Interfaces);
```

`Models/DeviceInventory.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record HardwareComponent(string Name, string PartNumber, string Status);

public sealed record DeviceInventory(
    string Model,
    string SerialNumber,
    string SoftwareVersion,
    string Memory,
    string Storage,
    IReadOnlyList<HardwareComponent> Components);
```

`Models/AlarmInfo.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

public sealed record AlarmInfo(
    string Timestamp,
    string Severity,
    string Description,
    string? Source);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet build Obss.sln --no-restore | tail -5 && dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~CiscoModelTests" --no-build`
Expected: Build succeeded + all tests PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/Models/
git add tests/Modules/Provisioning.Tests/Adapters/Cisco/Models/CiscoModelTests.cs
git commit -m "feat(cisco): add Cisco router adapter model types (10 records)"
```

---

### Task 3: Cisco Router — Interface + Config + Constants + Simulator

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/ICiscoRouterAdapter.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoAdapterConfig.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoAdapterConstants.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoRouterSimulator.cs`

**Test files:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoAdapterConfigTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoRouterSimulatorTests.cs`

- [ ] **Step 1: Write config and simulator tests first**

`CiscoAdapterConfigTests.cs`:
```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public class CiscoAdapterConfigTests
{
    [Fact]
    public void CiscoAdapterConfig_ShouldSetDefaultValues()
    {
        var config = new CiscoAdapterConfig { BaseUri = "https://device/restconf" };
        config.BaseUri.Should().Be("https://device/restconf");
        config.TimeoutSeconds.Should().Be(30);
        config.MaxRetries.Should().Be(3);
    }

    [Fact]
    public void CiscoAdapterConfig_ShouldOverrideDefaults()
    {
        var config = new CiscoAdapterConfig
        {
            BaseUri = "https://device/restconf",
            TimeoutSeconds = 60,
            MaxRetries = 5
        };
        config.TimeoutSeconds.Should().Be(60);
        config.MaxRetries.Should().Be(5);
    }
}
```

`CiscoRouterSimulatorTests.cs`:
```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public class CiscoRouterSimulatorTests
{
    private readonly CiscoRouterSimulator _simulator = new();

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldStoreInterface()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet",
            "Uplink", "10.0.0.1", 24, true, 1500, null, null);
        var result = await _simulator.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("GigabitEthernet0/0/0");
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.Name.Should().Be("GigabitEthernet0/0/0");
        getResult.Data.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task GetInterfaceAsync_NonExistent_ShouldReturnFailure()
    {
        var result = await _simulator.GetInterfaceAsync("GigabitEthernet9/9/9");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInterfaceAsync_ShouldRemoveInterface()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/1", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var deleteResult = await _simulator.DeleteInterfaceAsync("GigabitEthernet0/0/1");
        deleteResult.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("GigabitEthernet0/0/1");
        getResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetInterfaceStatusesAsync_ShouldReturnAllInterfaces()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var result = await _simulator.GetInterfaceStatusesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConfigureBgpAsync_ShouldStoreConfig()
    {
        var bgp = new BgpConfig(65001, "10.0.0.1", null, null);
        var result = await _simulator.ConfigureBgpAsync(bgp);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetBgpConfigAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.AsNumber.Should().Be(65001);
    }

    [Fact]
    public async Task ConfigureSystemAsync_ShouldStoreConfig()
    {
        var sys = new SystemConfig("router01", "example.com", null, null, null);
        var result = await _simulator.ConfigureSystemAsync(sys);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetSystemConfigAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.Hostname.Should().Be("router01");
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var result = await _simulator.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Interfaces.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ConfigureStaticRouteAsync_ShouldStoreRoute()
    {
        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null);
        var result = await _simulator.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();

        var routes = await _simulator.GetStaticRoutesAsync();
        routes.IsSuccess.Should().BeTrue();
        routes.Data.Should().Contain(r => r.Prefix == "0.0.0.0/0");
    }

    [Fact]
    public async Task ConfigureAclAsync_ShouldStoreAcl()
    {
        var entries = new[] { new AclEntry(10, "permit", "10.0.0.0", "0.0.0.255", null, null) };
        var acl = new AclConfig("MGMT-ACL", entries);
        var result = await _simulator.ConfigureAclAsync(acl);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ShouldReturnEmptyList()
    {
        var result = await _simulator.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearState_ShouldResetAllData()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        _simulator.ClearState();

        var result = await _simulator.GetInterfaceAsync("GigabitEthernet0/0/0");
        result.IsSuccess.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run simulator test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~CiscoRouterSimulatorTests|CiscoAdapterConfigTests" --no-build`
Expected: FAIL (types not found)

- [ ] **Step 3: Create ICiscoRouterAdapter interface**

```csharp
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

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

- [ ] **Step 4: Create CiscoAdapterConfig**

```csharp
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Transports.Restconf;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoAdapterConfig : AdapterConfigurationBase
{
    public string BaseUri { get; set; } = string.Empty;
    public bool UseSimulator { get; set; }
    public RestconfTransportConfig? RestconfTransport { get; set; }
}
```

Note: `AdapterConfigurationBase` already provides `ControllerUrl`, `Username`, `Password`, `TimeoutSeconds` (default 30), `MaxRetries` (default 3), `RetryDelayMs`, `CircuitBreakerThreshold`, `CircuitBreakerDurationSeconds`, etc.

- [ ] **Step 5: Create CiscoAdapterConstants**

```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public static class CiscoAdapterConstants
{
    public const string AdapterName = "CISCO_ROUTER";
    public const string TechnologyType = "cisco_router";

    // Default RESTCONF paths for Cisco IOS-XE YANG modules
    public const string InterfacePath = "/data/Cisco-IOS-XE-native:native/interface";
    public const string BgpPath = "/data/Cisco-IOS-XE-bgp:bgp";
    public const string OspfPath = "/data/Cisco-IOS-XE-ospf:ospf";
    public const string StaticRoutePath = "/data/Cisco-IOS-XE-native:native/ip/route";
    public const string SystemPath = "/data/Cisco-IOS-XE-native:native";
    public const string AclPath = "/data/Cisco-IOS-XE-acl:access-lists";
    public const string DeviceStatusPath = "/data/Cisco-IOS-XE-native:native";
    public const string InventoryPath = "/data/Cisco-IOS-XE-device-hardware:device-hardware";
    public const string AlarmsPath = "/data/Cisco-IOS-XE-alarms:alarms";
}
```

- [ ] **Step 6: Create CiscoRouterSimulator**

```csharp
using System.Collections.Concurrent;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoRouterSimulator : ICiscoRouterAdapter
{
    public string AdapterName => CiscoAdapterConstants.AdapterName;

    private readonly ConcurrentDictionary<string, InterfaceConfig> _interfaces = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StaticRoute> _staticRoutes = new(StringComparer.OrdinalIgnoreCase);
    private BgpConfig? _bgpConfig;
    private OspfConfig? _ospfConfig;
    private SystemConfig? _systemConfig;
    private AclConfig? _aclConfig;
    private readonly object _lock = new();

    public double FailureRate { get; set; } = 0.0;

    public void ClearState()
    {
        _interfaces.Clear();
        _staticRoutes.Clear();
        lock (_lock)
        {
            _bgpConfig = null;
            _ospfConfig = null;
            _systemConfig = null;
            _aclConfig = null;
        }
    }

    private bool ShouldFail() => FailureRate > 0 && Random.Shared.NextDouble() < FailureRate;

    public Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<InterfaceConfig>.Failure("Simulated failure"));
        if (string.IsNullOrEmpty(config.Name))
            return Task.FromResult(AdapterResult<InterfaceConfig>.Failure("Interface name is required"));
        _interfaces[config.Name] = config;
        return Task.FromResult(AdapterResult<InterfaceConfig>.Success(config));
    }

    public Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName)
    {
        if (_interfaces.TryGetValue(interfaceName, out var config))
            return Task.FromResult(AdapterResult<InterfaceConfig>.Success(config));
        return Task.FromResult(AdapterResult<InterfaceConfig>.Failure($"Interface {interfaceName} not found"));
    }

    public Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync()
    {
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.Name, i.AdminUp == true ? "up" : "down", "up", 1000000)).ToList();
        return Task.FromResult(AdapterResult<IReadOnlyList<InterfaceStatus>>.Success(statuses));
    }

    public Task<AdapterResult> DeleteInterfaceAsync(string interfaceName)
    {
        if (_interfaces.TryRemove(interfaceName, out _))
            return Task.FromResult(AdapterResult.Success());
        return Task.FromResult(AdapterResult.Failure($"Interface {interfaceName} not found"));
    }

    public Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<BgpConfig>.Failure("Simulated failure"));
        lock (_lock) { _bgpConfig = config; }
        return Task.FromResult(AdapterResult<BgpConfig>.Success(config));
    }

    public Task<AdapterResult<BgpConfig>> GetBgpConfigAsync()
    {
        lock (_lock)
        {
            if (_bgpConfig is not null)
                return Task.FromResult(AdapterResult<BgpConfig>.Success(_bgpConfig));
        }
        return Task.FromResult(AdapterResult<BgpConfig>.Failure("BGP not configured"));
    }

    public Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<OspfConfig>.Failure("Simulated failure"));
        lock (_lock) { _ospfConfig = config; }
        return Task.FromResult(AdapterResult<OspfConfig>.Success(config));
    }

    public Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult.Failure("Simulated failure"));
        if (string.IsNullOrEmpty(route.Prefix))
            return Task.FromResult(AdapterResult.Failure("Route prefix is required"));
        _staticRoutes[route.Prefix] = route;
        return Task.FromResult(AdapterResult.Success());
    }

    public Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync()
    {
        var routes = _staticRoutes.Values.ToList();
        return Task.FromResult(AdapterResult<IReadOnlyList<StaticRoute>>.Success(routes));
    }

    public Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<SystemConfig>.Failure("Simulated failure"));
        lock (_lock) { _systemConfig = config; }
        return Task.FromResult(AdapterResult<SystemConfig>.Success(config));
    }

    public Task<AdapterResult<SystemConfig>> GetSystemConfigAsync()
    {
        lock (_lock)
        {
            if (_systemConfig is not null)
                return Task.FromResult(AdapterResult<SystemConfig>.Success(_systemConfig));
        }
        return Task.FromResult(AdapterResult<SystemConfig>.Success(
            new SystemConfig("sim-router", "example.com", null, null, null)));
    }

    public Task<AdapterResult<AclConfig>> ConfigureAclAsync(AclConfig config)
    {
        if (ShouldFail()) return Task.FromResult(AdapterResult<AclConfig>.Failure("Simulated failure"));
        lock (_lock) { _aclConfig = config; }
        return Task.FromResult(AdapterResult<AclConfig>.Success(config));
    }

    public Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        var statuses = _interfaces.Values.Select(i => new InterfaceStatus(
            i.Name, i.AdminUp == true ? "up" : "down", "up", 1000000)).ToList();
        var status = new DeviceStatus("sim-router", "17.9.1", "ISR4451-X/K9",
            "100 days", 25.0, 50.0, statuses);
        return Task.FromResult(AdapterResult<DeviceStatus>.Success(status));
    }

    public Task<AdapterResult<DeviceInventory>> GetInventoryAsync()
    {
        var components = new[] { new HardwareComponent("Chassis", "ISR4451-X/K9", "OK") };
        var inventory = new DeviceInventory("ISR4451-X/K9", "SIM-SN-001", "17.9.1", "4GB", "16GB", components);
        return Task.FromResult(AdapterResult<DeviceInventory>.Success(inventory));
    }

    public Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync()
    {
        return Task.FromResult(AdapterResult<IReadOnlyList<AlarmInfo>>.Success(Array.Empty<AlarmInfo>()));
    }
}
```

- [ ] **Step 7: Build and run tests**

Run: `dotnet build Obss.sln --no-restore | tail -5`
Expected: Build succeeded

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~CiscoRouterSimulatorTests|CiscoAdapterConfigTests" --no-build`
Expected: All tests PASS

- [ ] **Step 8: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/
git add tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoAdapterConfigTests.cs
git add tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoRouterSimulatorTests.cs
git commit -m "feat(cisco): add Cisco router adapter interface, config, constants, and simulator"
```

---

### Task 4: Cisco Router — Real Adapter Implementation

**File:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoRouterAdapter.cs`

**Test file:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoRouterAdapterTests.cs` (WireMock integration)

- [ ] **Step 1: Write the failing adapter tests**

```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public class CiscoRouterAdapterTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly CiscoRouterAdapter _adapter;
    private readonly CiscoAdapterConfig _config;

    public CiscoRouterAdapterTests()
    {
        _server = WireMockServer.Start();
        _config = new CiscoAdapterConfig { BaseUri = _server.Url! };
        var restconfTransport = new RestconfTransport(new RestconfTransportConfig
        {
            BaseUri = _server.Url!,
            Name = "test-cisco"
        });
        _adapter = new CiscoRouterAdapter(_config, restconfTransport);
    }

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/GigabitEthernet=0%2F0%2F0").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet",
            "Uplink", "10.0.0.1", 24, true, 1500, null, null);
        var result = await _adapter.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInterfaceAsync_ShouldReturnInterface()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/GigabitEthernet=0%2F0%2F0").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-native:GigabitEthernet\":{\"name\":\"0/0/0\",\"description\":\"Uplink\"}}"));

        var result = await _adapter.GetInterfaceAsync("GigabitEthernet0/0/0");
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("GigabitEthernet0/0/0");
    }

    [Fact]
    public async Task GetInterfaceAsync_NotFound_ShouldReturnFailure()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/*").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));

        var result = await _adapter.GetInterfaceAsync("GigabitEthernet9/9/9");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetInterfaceStatusesAsync_ShouldReturnList()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-native:native\":{\"interface\":{\"GigabitEthernet\":[{\"name\":\"0/0/0\"}]}}}"));

        var result = await _adapter.GetInterfaceStatusesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/GigabitEthernet=0%2F0%2F0").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var result = await _adapter.DeleteInterfaceAsync("GigabitEthernet0/0/0");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureBgpAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-bgp:bgp").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var bgp = new BgpConfig(65001, "10.0.0.1", null, null);
        var result = await _adapter.ConfigureBgpAsync(bgp);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-native:native\":{\"hostname\":\"router01\",\"version\":\"17.9.1\"}}"));

        var result = await _adapter.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("router01");
    }

    [Fact]
    public async Task ConfigureStaticRouteAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/ip/route").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201));

        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null);
        var result = await _adapter.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ShouldReturnAlarms()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-alarms:alarms").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-alarms:alarms\":{\"alarm\":[{\"severity\":\"critical\",\"alarm-description\":\"Interface down\"}]}}"));

        var result = await _adapter.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain(a => a.Severity == "critical");
    }

    public void Dispose()
    {
        _server?.Dispose();
        _adapter?.Dispose();
    }
}
```

- [ ] **Step 2: Run adapter test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~CiscoRouterAdapterTests" --no-build`
Expected: FAIL (CiscoRouterAdapter not found)

- [ ] **Step 3: Create CiscoRouterAdapter**

```csharp
using System.Text.Json;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoRouterAdapter : ICiscoRouterAdapter, IDisposable
{
    private readonly IRestconfTransport _transport;
    private readonly CiscoAdapterConfig _config;

    public string AdapterName => CiscoAdapterConstants.AdapterName;

    public CiscoRouterAdapter(CiscoAdapterConfig config, IRestconfTransport transport)
    {
        _config = config;
        _transport = transport;
    }

    // Interface Management
    public async Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config)
    {
        var path = $"{CiscoAdapterConstants.InterfacePath}/{config.Type}={config.Name}";
        var payload = CreateInterfacePayload(config);
        var result = await _transport.PutAsync(path, payload);
        return result.Success
            ? AdapterResult<InterfaceConfig>.Success(config)
            : AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage);
    }

    public async Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName)
    {
        // Parse type from name (e.g., "GigabitEthernet0/0/0" -> type "GigabitEthernet", name "0/0/0")
        var (type, name) = ParseInterfaceName(interfaceName);
        var path = $"{CiscoAdapterConstants.InterfacePath}/{type}={name}";
        var result = await _transport.GetAsync(path);

        if (!result.Success)
            return AdapterResult<InterfaceConfig>.Failure(result.ErrorMessage);

        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var iface = ParseInterfaceFromResponse(doc.RootElement, type, interfaceName);
            return iface is not null
                ? AdapterResult<InterfaceConfig>.Success(iface)
                : AdapterResult<InterfaceConfig>.Failure("Failed to parse interface");
        }
        catch
        {
            return AdapterResult<InterfaceConfig>.Failure("Failed to parse interface response");
        }
    }

    public async Task<AdapterResult<IReadOnlyList<InterfaceStatus>>> GetInterfaceStatusesAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.DeviceStatusPath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<InterfaceStatus>>.Failure(result.ErrorMessage);

        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var statuses = ParseInterfaceStatuses(doc.RootElement);
            return AdapterResult<IReadOnlyList<InterfaceStatus>>.Success(statuses);
        }
        catch
        {
            return AdapterResult<IReadOnlyList<InterfaceStatus>>.Failure("Failed to parse interface statuses");
        }
    }

    public async Task<AdapterResult> DeleteInterfaceAsync(string interfaceName)
    {
        var (type, name) = ParseInterfaceName(interfaceName);
        var path = $"{CiscoAdapterConstants.InterfacePath}/{type}={name}";
        var result = await _transport.DeleteAsync(path);
        return result.Success
            ? AdapterResult.Success()
            : AdapterResult.Failure(result.ErrorMessage);
    }

    // Routing Protocols
    public async Task<AdapterResult<BgpConfig>> ConfigureBgpAsync(BgpConfig config)
    {
        var payload = new
        {
            bgp = new
            {
                id = config.AsNumber,
                routerId = config.RouterId,
                neighbor = config.Neighbors?.Select(n => new
                {
                    id = n.RemoteAddress,
                    remoteAs = n.RemoteAs,
                    peerGroup = n.PeerGroup
                }).ToArray(),
                network = config.Networks?.Select(n => new
                {
                    id = $"{n.Prefix}/{n.PrefixLength}",
                    backdoor = (string?)null
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(CiscoAdapterConstants.BgpPath, payload);
        return result.Success
            ? AdapterResult<BgpConfig>.Success(config)
            : AdapterResult<BgpConfig>.Failure(result.ErrorMessage);
    }

    public async Task<AdapterResult<BgpConfig>> GetBgpConfigAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.BgpPath);
        if (!result.Success)
            return AdapterResult<BgpConfig>.Failure(result.ErrorMessage);
        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var bgp = ParseBgpConfig(doc.RootElement);
            return bgp is not null
                ? AdapterResult<BgpConfig>.Success(bgp)
                : AdapterResult<BgpConfig>.Failure("Failed to parse BGP config");
        }
        catch
        {
            return AdapterResult<BgpConfig>.Failure("Failed to parse BGP config");
        }
    }

    public async Task<AdapterResult<OspfConfig>> ConfigureOspfAsync(OspfConfig config)
    {
        var payload = new
        {
            ospf = new
            {
                id = config.ProcessId,
                routerId = config.RouterId,
                area = config.Areas?.Select(a => new
                {
                    id = a.AreaId,
                    interface = a.Interfaces?.ToArray()
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(CiscoAdapterConstants.OspfPath, payload);
        return result.Success
            ? AdapterResult<OspfConfig>.Success(config)
            : AdapterResult<OspfConfig>.Failure(result.ErrorMessage);
    }

    public async Task<AdapterResult> ConfigureStaticRouteAsync(StaticRoute route)
    {
        var payload = new
        {
            route = new
            {
                via = new[]
                {
                    new
                    {
                        prefix = route.Prefix,
                        @"next-hop" = new[]
                        {
                            new
                            {
                                @"forwarding-address" = route.NextHop,
                                metric = route.AdministrativeDistance
                            }
                        }
                    }
                }
            }
        };
        var result = await _transport.PostAsync(CiscoAdapterConstants.StaticRoutePath, payload);
        return result.Success
            ? AdapterResult.Success()
            : AdapterResult.Failure(result.ErrorMessage);
    }

    public async Task<AdapterResult<IReadOnlyList<StaticRoute>>> GetStaticRoutesAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.StaticRoutePath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<StaticRoute>>.Failure(result.ErrorMessage);
        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var routes = ParseStaticRoutes(doc.RootElement);
            return AdapterResult<IReadOnlyList<StaticRoute>>.Success(routes);
        }
        catch
        {
            return AdapterResult<IReadOnlyList<StaticRoute>>.Failure("Failed to parse routes");
        }
    }

    // System Configuration
    public async Task<AdapterResult<SystemConfig>> ConfigureSystemAsync(SystemConfig config)
    {
        var payload = new
        {
            hostname = config.Hostname,
            ip = new
            {
                domain = new
                {
                    name = config.DomainName,
                    nameServer = config.DnsServers?.Select(d => new { address = d }).ToArray()
                }
            },
            ntp = config.NtpServers?.Select(n => new { server = new { name = n } }).ToArray()
        };
        var result = await _transport.PutAsync(CiscoAdapterConstants.SystemPath, payload);
        return result.Success
            ? AdapterResult<SystemConfig>.Success(config)
            : AdapterResult<SystemConfig>.Failure(result.ErrorMessage);
    }

    public async Task<AdapterResult<SystemConfig>> GetSystemConfigAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.SystemPath);
        if (!result.Success)
            return AdapterResult<SystemConfig>.Failure(result.ErrorMessage);
        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var sys = ParseSystemConfig(doc.RootElement);
            return sys is not null
                ? AdapterResult<SystemConfig>.Success(sys)
                : AdapterResult<SystemConfig>.Failure("Failed to parse system config");
        }
        catch
        {
            return AdapterResult<SystemConfig>.Failure("Failed to parse system config");
        }
    }

    // Security
    public async Task<AdapterResult<AclConfig>> ConfigureAclAsync(AclConfig config)
    {
        var payload = new
        {
            acl = new
            {
                name = config.Name,
                accessListEntries = config.Entries.Select(e => new
                {
                    sequence = e.Sequence,
                    action = e.Action,
                    source = new
                    {
                        address = e.Source,
                        wildcard = e.SourceWildcard
                    },
                    destination = e.Destination is not null ? new { address = e.Destination } : null,
                    log = e.Log
                }).ToArray()
            }
        };
        var result = await _transport.PutAsync(CiscoAdapterConstants.AclPath, payload);
        return result.Success
            ? AdapterResult<AclConfig>.Success(config)
            : AdapterResult<AclConfig>.Failure(result.ErrorMessage);
    }

    // Device Status
    public async Task<AdapterResult<DeviceStatus>> GetDeviceStatusAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.DeviceStatusPath);
        if (!result.Success)
            return AdapterResult<DeviceStatus>.Failure(result.ErrorMessage);
        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var status = ParseDeviceStatus(doc.RootElement);
            return status is not null
                ? AdapterResult<DeviceStatus>.Success(status)
                : AdapterResult<DeviceStatus>.Failure("Failed to parse device status");
        }
        catch
        {
            return AdapterResult<DeviceStatus>.Failure("Failed to parse device status");
        }
    }

    public async Task<AdapterResult<DeviceInventory>> GetInventoryAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.InventoryPath);
        if (!result.Success)
            return AdapterResult<DeviceInventory>.Failure(result.ErrorMessage);
        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var inventory = ParseInventory(doc.RootElement);
            return inventory is not null
                ? AdapterResult<DeviceInventory>.Success(inventory)
                : AdapterResult<DeviceInventory>.Failure("Failed to parse inventory");
        }
        catch
        {
            return AdapterResult<DeviceInventory>.Failure("Failed to parse inventory");
        }
    }

    // Monitoring
    public async Task<AdapterResult<IReadOnlyList<AlarmInfo>>> GetActiveAlarmsAsync()
    {
        var result = await _transport.GetAsync(CiscoAdapterConstants.AlarmsPath);
        if (!result.Success)
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Failure(result.ErrorMessage);
        try
        {
            using var doc = JsonDocument.Parse(result.Data);
            var alarms = ParseAlarms(doc.RootElement);
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Success(alarms);
        }
        catch
        {
            return AdapterResult<IReadOnlyList<AlarmInfo>>.Failure("Failed to parse alarms");
        }
    }

    public void Dispose() { /* _transport is owned by DI */ }

    // Helpers
    private static (string type, string name) ParseInterfaceName(string interfaceName)
    {
        // e.g., "GigabitEthernet0/0/0" -> ("GigabitEthernet", "0/0/0")
        // e.g., "Loopback0" -> ("Loopback", "0")
        // e.g., "Vlan100" -> ("Vlan", "100")
        var typeEnd = interfaceName.IndexOfAny("0123456789".ToCharArray());
        return typeEnd > 0
            ? (interfaceName[..typeEnd], interfaceName[typeEnd..])
            : (interfaceName, "");
    }

    private static object CreateInterfacePayload(InterfaceConfig config)
    {
        return new
        {
            name = config.Name,
            description = config.Description,
            ip = config.IpAddress is not null ? new
            {
                address = new
                {
                    primary = new
                    {
                        address = config.IpAddress,
                        mask = config.PrefixLength.HasValue
                            ? string.Join(".", Enumerable.Range(0, 4).Select(
                                i => (config.PrefixLength.Value >= (i + 1) * 8) ? "255"
                                    : (config.PrefixLength.Value > i * 8)
                                        ? (255 << (8 - (config.PrefixLength.Value - i * 8)) & 0xFF).ToString()
                                        : "0"))
                            : null
                    }
                }
            } : null,
            shutdown = config.AdminUp == false,
            mtu = config.Mtu,
            vlan = config.VlanId is not null ? new { id = config.VlanId } : null,
            switchport = config.SwitchportModes?.Any() == true ? new { mode = new { access = (object?)null } } : null
        };
    }

    private static InterfaceConfig? ParseInterfaceFromResponse(JsonElement root, string type, string fullName)
    {
        // Navigate Cisco-specific JSON structure
        if (!root.TryGetProperty($"Cisco-IOS-XE-native:{type}", out var iface))
            return null;
        var name = iface.TryGetProperty("name", out var n) ? n.GetString() : fullName;
        var desc = iface.TryGetProperty("description", out var d) ? d.GetString() : null;
        return new InterfaceConfig(name ?? fullName, type, desc, null, null, null, null, null, null);
    }

    private static List<InterfaceStatus> ParseInterfaceStatuses(JsonElement root)
    {
        var statuses = new List<InterfaceStatus>();
        if (!root.TryGetProperty("Cisco-IOS-XE-native:native", out var native))
            return statuses;
        if (!native.TryGetProperty("interface", out var ifaces))
            return statuses;
        foreach (var ifaceType in ifaces.EnumerateObject())
        {
            foreach (var iface in ifaceType.Value.EnumerateArray())
            {
                var name = iface.TryGetProperty("name", out var n) ? $"{ifaceType.Name}{n.GetString()}" : ifaceType.Name;
                statuses.Add(new InterfaceStatus(name, "up", "up", 1000000));
            }
        }
        return statuses;
    }

    private static BgpConfig? ParseBgpConfig(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-bgp:bgp", out var bgp))
            return null;
        var asNumber = bgp.TryGetProperty("id", out var id) ? id.GetInt32() : 0;
        var routerId = bgp.TryGetProperty("routerId", out var rid) ? rid.GetString() : null;
        return new BgpConfig(asNumber, routerId, null, null);
    }

    private static List<StaticRoute> ParseStaticRoutes(JsonElement root)
    {
        var routes = new List<StaticRoute>();
        if (!root.TryGetProperty("Cisco-IOS-XE-native:route", out var route))
            return routes;
        if (!route.TryGetProperty("via", out var via))
            return routes;
        foreach (var v in via.EnumerateArray())
        {
            var prefix = v.TryGetProperty("prefix", out var p) ? p.GetString() : "";
            var nextHop = "";
            if (v.TryGetProperty("next-hop", out var nh) && nh.GetArrayLength() > 0)
                nextHop = nh[0].TryGetProperty("forwarding-address", out var fa) ? fa.GetString() : "";
            routes.Add(new StaticRoute(prefix ?? "", nextHop ?? "", null, null));
        }
        return routes;
    }

    private static SystemConfig? ParseSystemConfig(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-native:native", out var native))
            return null;
        var hostname = native.TryGetProperty("hostname", out var h) ? h.GetString() : null;
        return new SystemConfig(hostname, null, null, null, null);
    }

    private static DeviceStatus? ParseDeviceStatus(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-native:native", out var native))
            return null;
        var hostname = native.TryGetProperty("hostname", out var h) ? h.GetString() : "unknown";
        var version = native.TryGetProperty("version", out var v) ? v.GetString() : "unknown";
        var interfaces = ParseInterfaceStatuses(root);
        return new DeviceStatus(hostname ?? "unknown", version ?? "unknown", "Cisco",
            "unknown", 0, 0, interfaces);
    }

    private static DeviceInventory? ParseInventory(JsonElement root)
    {
        if (!root.TryGetProperty("Cisco-IOS-XE-device-hardware:device-hardware", out var hw))
            return null;
        var model = hw.TryGetProperty("model", out var m) ? m.GetString() : "unknown";
        var serial = hw.TryGetProperty("serial-number", out var s) ? s.GetString() : "unknown";
        return new DeviceInventory(model ?? "unknown", serial ?? "unknown", "unknown",
            "unknown", "unknown", Array.Empty<HardwareComponent>());
    }

    private static List<AlarmInfo> ParseAlarms(JsonElement root)
    {
        var alarms = new List<AlarmInfo>();
        if (!root.TryGetProperty("Cisco-IOS-XE-alarms:alarms", out var alarmsEl))
            return alarms;
        if (!alarmsEl.TryGetProperty("alarm", out var alarmArr))
            return alarms;
        foreach (var a in alarmArr.EnumerateArray())
        {
            var severity = a.TryGetProperty("severity", out var sev) ? sev.GetString() : "unknown";
            var desc = a.TryGetProperty("alarm-description", out var d) ? d.GetString() : "";
            var source = a.TryGetProperty("alarm-source", out var src) ? src.GetString() : null;
            alarms.Add(new AlarmInfo("", severity ?? "unknown", desc ?? "", source));
        }
        return alarms;
    }
}
```

- [ ] **Step 4: Build and run tests**

Run: `dotnet build Obss.sln --no-restore | tail -5`
Expected: Build succeeded (may have warnings about unused `_config` field — acceptable)

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~CiscoRouterAdapterTests" --no-build`
Expected: All tests PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoRouterAdapter.cs
git add tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoRouterAdapterTests.cs
git commit -m "feat(cisco): add CiscoRouterAdapter real implementation with WireMock tests"
```

---

### Task 5: Cisco Router — Health Check + ProvisioningAdapter + DI Wiring

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoRouterHealthCheck.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoProvisioningAdapter.cs`
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/ModuleRegistration.cs` (or equivalent DI registration)

**Test files:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoRouterHealthCheckTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoProvisioningAdapterTests.cs`

- [ ] **Step 1: Write health check and provisioning adapter tests**

`CiscoRouterHealthCheckTests.cs`:
```csharp
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public class CiscoRouterHealthCheckTests
{
    [Fact]
    public void HealthCheckName_ShouldMatchConvention()
    {
        var check = new CiscoRouterHealthCheck(null!);
        check.GetType().Name.Should().Be("CiscoRouterHealthCheck");
    }
}
```

`CiscoProvisioningAdapterTests.cs`:
```csharp
using FluentAssertions;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public class CiscoProvisioningAdapterTests
{
    [Fact]
    public void AdapterName_ShouldReturnCiscoRouter()
    {
        var adapter = new CiscoProvisioningAdapter(null!);
        adapter.AdapterName.Should().Be("CISCO_ROUTER");
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~CiscoRouterHealthCheckTests|CiscoProvisioningAdapterTests" --no-build`
Expected: FAIL (types not found)

- [ ] **Step 3: Create CiscoRouterHealthCheck**

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoRouterHealthCheck : IHealthCheck
{
    private readonly ICiscoRouterAdapter _adapter;

    public CiscoRouterHealthCheck(ICiscoRouterAdapter adapter)
    {
        _adapter = adapter;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _adapter.GetDeviceStatusAsync();
            if (result.IsSuccess)
                return HealthCheckResult.Healthy("Cisco router reachable");
            return HealthCheckResult.Degraded($"Cisco router responded with: {result.ErrorMessage}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cisco router unreachable", ex);
        }
    }
}
```

- [ ] **Step 4: Create CiscoProvisioningAdapter**

```csharp
using System.Text.Json;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public sealed class CiscoProvisioningAdapter : IProvisioningAdapter
{
    private readonly ICiscoRouterAdapter _adapter;

    public string AdapterName => CiscoAdapterConstants.AdapterName;

    public CiscoProvisioningAdapter(ICiscoRouterAdapter adapter)
    {
        _adapter = adapter;
    }

    public async Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        if (task.Configuration is null)
            return ProvisioningResult.Failure("No configuration provided");

        try
        {
            var config = task.Configuration.RootElement;
            var operation = config.TryGetProperty("operation", out var op) ? op.GetString() : null;

            return operation switch
            {
                "configureInterface" => await ExecuteConfigureInterface(config),
                "getInterface" => await ExecuteGetInterface(config),
                "deleteInterface" => await ExecuteDeleteInterface(config),
                "configureBgp" => await ExecuteConfigureBgp(config),
                "getBgpConfig" => await ExecuteGetBgpConfig(),
                "configureOspf" => await ExecuteConfigureOspf(config),
                "configureStaticRoute" => await ExecuteConfigureStaticRoute(config),
                "configureSystem" => await ExecuteConfigureSystem(config),
                "getDeviceStatus" => await ExecuteGetDeviceStatus(),
                "getInventory" => await ExecuteGetInventory(),
                "getAlarms" => await ExecuteGetAlarms(),
                _ => ProvisioningResult.Failure($"Unknown operation: {operation}")
            };
        }
        catch (Exception ex)
        {
            return ProvisioningResult.Failure($"Cisco adapter error: {ex.Message}");
        }
    }

    public async Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        // Rollback: delete what we created
        if (task.Configuration?.RootElement.TryGetProperty("operation", out var op) == true)
        {
            var operation = op.GetString();
            if (operation == "configureInterface")
            {
                var name = task.Configuration.RootElement.TryGetProperty("interfaceName", out var n) ? n.GetString() : null;
                if (name is not null)
                    await _adapter.DeleteInterfaceAsync(name);
            }
        }
        return ProvisioningResult.Success();
    }

    private async Task<ProvisioningResult> ExecuteConfigureInterface(JsonElement config)
    {
        var iface = DeserializeInterface(config);
        var result = await _adapter.ConfigureInterfaceAsync(iface);
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteGetInterface(JsonElement config)
    {
        var name = config.TryGetProperty("interfaceName", out var n) ? n.GetString() : "";
        var result = await _adapter.GetInterfaceAsync(name ?? "");
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteDeleteInterface(JsonElement config)
    {
        var name = config.TryGetProperty("interfaceName", out var n) ? n.GetString() : "";
        var result = await _adapter.DeleteInterfaceAsync(name ?? "");
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteConfigureBgp(JsonElement config)
    {
        var asn = config.TryGetProperty("asNumber", out var a) ? a.GetInt32() : 0;
        var routerId = config.TryGetProperty("routerId", out var r) ? r.GetString() : null;
        var bgp = new BgpConfig(asn, routerId, null, null);
        var result = await _adapter.ConfigureBgpAsync(bgp);
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteGetBgpConfig()
    {
        var result = await _adapter.GetBgpConfigAsync();
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteConfigureOspf(JsonElement config)
    {
        var processId = config.TryGetProperty("processId", out var p) ? p.GetInt32() : 0;
        var routerId = config.TryGetProperty("routerId", out var r) ? r.GetString() : null;
        var ospf = new OspfConfig(processId, routerId, null);
        var result = await _adapter.ConfigureOspfAsync(ospf);
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteConfigureStaticRoute(JsonElement config)
    {
        var prefix = config.TryGetProperty("prefix", out var p) ? p.GetString() : "";
        var nextHop = config.TryGetProperty("nextHop", out var n) ? n.GetString() : "";
        var route = new StaticRoute(prefix ?? "", nextHop ?? "", null, null);
        var result = await _adapter.ConfigureStaticRouteAsync(route);
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteConfigureSystem(JsonElement config)
    {
        var hostname = config.TryGetProperty("hostname", out var h) ? h.GetString() : null;
        var sys = new SystemConfig(hostname, null, null, null, null);
        var result = await _adapter.ConfigureSystemAsync(sys);
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteGetDeviceStatus()
    {
        var result = await _adapter.GetDeviceStatusAsync();
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteGetInventory()
    {
        var result = await _adapter.GetInventoryAsync();
        return ToProvisioningResult(result);
    }

    private async Task<ProvisioningResult> ExecuteGetAlarms()
    {
        var result = await _adapter.GetActiveAlarmsAsync();
        return ToProvisioningResult(result);
    }

    private static InterfaceConfig DeserializeInterface(JsonElement config)
    {
        return new InterfaceConfig(
            config.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            config.TryGetProperty("type", out var t) ? t.GetString() ?? "GigabitEthernet" : "GigabitEthernet",
            config.TryGetProperty("description", out var d) ? d.GetString() : null,
            config.TryGetProperty("ipAddress", out var ip) ? ip.GetString() : null,
            config.TryGetProperty("prefixLength", out var pl) ? pl.GetInt32() : null,
            config.TryGetProperty("adminUp", out var au) ? au.GetBoolean() : null,
            config.TryGetProperty("mtu", out var m) ? m.GetInt32() : null,
            config.TryGetProperty("vlanId", out var vi) ? vi.GetString() : null,
            null);
    }

    private static ProvisioningResult ToProvisioningResult<T>(AdapterResult<T> result)
    {
        return result.IsSuccess
            ? ProvisioningResult.Success(result.Data)
            : ProvisioningResult.Failure(result.ErrorMessage);
    }

    private static ProvisioningResult ToProvisioningResult(AdapterResult result)
    {
        return result.IsSuccess
            ? ProvisioningResult.Success()
            : ProvisioningResult.Failure(result.ErrorMessage);
    }
}
```

- [ ] **Step 5: Register Cisco adapter in DI**

Modify `src/Modules/Provisioning/Obss.Provisioning.Api/Extensions/ProvisioningModuleRegistration.cs`.

Add using statements at the top:
```csharp
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Microsoft.Extensions.Diagnostics.HealthChecks;
```

Add `RegisterCiscoAdapter` call inside `AddProvisioningModule` (after the existing `RegisterHuaweiAdapter` call):
```csharp
RegisterCiscoAdapter(services, configuration);
```

Add the registration method:
```csharp
private static void RegisterCiscoAdapter(IServiceCollection services, IConfiguration? configuration)
{
    var config = new CiscoAdapterConfig();
    if (configuration is not null)
    {
        var section = configuration.GetSection("Provisioning:Cisco");
        if (section.Exists())
        {
            section.Bind(config);
            config.RestconfTransport = section.GetSection("RestconfTransport").Exists()
                ? section.GetSection("RestconfTransport").Get<RestconfTransportConfig>()
                : null;
        }
    }

    services.AddSingleton(config);

    if (config.UseSimulator)
    {
        services.AddSingleton<ICiscoRouterAdapter, CiscoRouterSimulator>();
    }
    else
    {
        services.AddSingleton<ICiscoRouterAdapter>(sp =>
        {
            var transportConfig = config.RestconfTransport ?? new RestconfTransportConfig
            {
                BaseUri = config.BaseUri,
                Name = "cisco-router"
            };
            var transport = new RestconfTransport(transportConfig);
            return new CiscoRouterAdapter(config, transport);
        });
    }

    services.AddScoped<IProvisioningAdapter, CiscoProvisioningAdapter>();
    services.AddSingleton<IHealthCheck, CiscoRouterHealthCheck>();
}
```

Note: `CiscoAdapterConfig` will need a `UseSimulator` bool property and a `RestconfTransport` property. Read the existing `HuaweiAdapterConfig` for reference on the pattern.

- [ ] **Step 6: Build and run all Cisco tests**

Run: `dotnet build Obss.sln --no-restore | tail -5`
Expected: Build succeeded

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Cisco" --no-build`
Expected: All Cisco tests PASS

- [ ] **Step 7: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoRouterHealthCheck.cs
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Cisco/CiscoProvisioningAdapter.cs
git add tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoRouterHealthCheckTests.cs
git add tests/Modules/Provisioning.Tests/Adapters/Cisco/CiscoProvisioningAdapterTests.cs
# Also add the DI registration file changes
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/...
git commit -m "feat(cisco): add Cisco health check, provisioning adapter, and DI wiring"
```

---

### Task 6: Juniper Router — Model Types

**Files (create all):**
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/InterfaceConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/InterfaceStatus.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/BgpConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/OspfConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/StaticRoute.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/SystemConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/AclConfig.cs` (Juniper: `FirewallFilter`)
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/DeviceStatus.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/DeviceInventory.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/AlarmInfo.cs`

**Test file:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Juniper/Models/JuniperModelTests.cs`

These are the same shape as Cisco but with Juniper-specific model structure. Juniper's YANG model differs:
- Interfaces are under `junos-configuration:configuration/interfaces` with `unit` and `family` sub-structures
- Firewall filters replace ACLs
- Device properties use Juniper-specific naming

- [ ] **Step 1: Write the failing Juniper model tests**

```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Juniper.Models;

public class JuniperModelTests
{
    [Fact]
    public void InterfaceConfig_ShouldPreserveProperties()
    {
        var unit = new InterfaceUnit(0, new[] { "inet" }, "10.0.0.1", 24);
        var config = new InterfaceConfig("ge-0/0/0", "GE", "Uplink", true, 1500, new[] { unit });
        config.Name.Should().Be("ge-0/0/0");
        config.Units.Should().Contain(u => u.UnitId == 0);
    }

    [Fact]
    public void BgpConfig_ShouldPreserveProperties()
    {
        var group = new BgpGroup("EBGP-PEERS", new[] { new BgpNeighborJ("10.0.0.2", 65002) });
        var bgp = new BgpConfigJ(65001, "10.0.0.1", new[] { group });
        bgp.AsNumber.Should().Be(65001);
        bgp.Groups.Should().Contain(g => g.Name == "EBGP-PEERS");
    }

    [Fact]
    public void FirewallFilter_ShouldPreserveProperties()
    {
        var term = new FirewallTerm("ALLOW-MGMT", new[] { "10.0.0.0/24" }, "accept", "log");
        var filter = new FirewallFilter("MGMT-FILTER", new[] { term });
        filter.Name.Should().Be("MGMT-FILTER");
        filter.Terms.Should().Contain(t => t.Action == "accept");
    }

    [Fact]
    public void DeviceStatus_ShouldPreserveProperties()
    {
        var status = new DeviceStatusJ("router01", "22.4R2", "MX480", "200 days", 35.0, 45.0);
        status.Hostname.Should().Be("router01");
        status.Model.Should().Be("MX480");
    }

    [Fact]
    public void StaticRouteJ_ShouldPreserveProperties()
    {
        var route = new StaticRouteJ("0.0.0.0/0", "10.0.0.1", 1, "ge-0/0/0");
        route.Prefix.Should().Be("0.0.0.0/0");
        route.NextHop.Should().Be("10.0.0.1");
    }

    [Fact]
    public void AlarmInfo_ShouldPreserveProperties()
    {
        var alarm = new AlarmInfoJ("2026-07-23T10:00:00Z", "yellow", "Link down", "ge-0/0/0");
        alarm.Severity.Should().Be("yellow");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~JuniperModelTests" --no-build`
Expected: FAIL (types not found)

- [ ] **Step 3: Create all Juniper model records**

Create directory `Adapters/Juniper/Models/` and add each file. Juniper model types differ from Cisco's — they follow JunOS YANG structure (interfaces with units/family, firewall filters instead of ACLs, etc.).

`Models/InterfaceConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record InterfaceUnit(
    int UnitId,
    IReadOnlyList<string>? Family,
    string? IpAddress,
    int? PrefixLength);

public sealed record InterfaceConfig(
    string Name,
    string Type,
    string? Description,
    bool? AdminUp,
    int? Mtu,
    IReadOnlyList<InterfaceUnit>? Units);
```

`Models/InterfaceStatus.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record InterfaceStatusJ(
    string InterfaceName,
    string AdminStatus,
    string OperStatus,
    long Speed);
```

`Models/BgpConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record BgpNeighborJ(string RemoteAddress, int RemoteAs);

public sealed record BgpGroup(string Name, IReadOnlyList<BgpNeighborJ>? Neighbors);

public sealed record BgpConfigJ(
    int AsNumber,
    string? RouterId,
    IReadOnlyList<BgpGroup>? Groups);
```

`Models/OspfConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record OspfAreaJ(int AreaId, IReadOnlyList<string>? Interfaces);

public sealed record OspfConfigJ(
    int ProcessId,
    string? RouterId,
    IReadOnlyList<OspfAreaJ>? Areas);
```

`Models/StaticRoute.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record StaticRouteJ(
    string Prefix,
    string NextHop,
    int? AdministrativeDistance,
    string? Interface);
```

`Models/SystemConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record SystemConfigJ(
    string? Hostname,
    string? DomainName,
    IReadOnlyList<string>? NtpServers,
    IReadOnlyList<string>? DnsServers);
```

`Models/FirewallFilter.cs` (replaces ACL for Juniper):
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record FirewallTerm(
    string Name,
    IReadOnlyList<string>? SourceAddresses,
    string Action,
    string? Log);

public sealed record FirewallFilter(
    string Name,
    IReadOnlyList<FirewallTerm> Terms);
```

`Models/DeviceStatus.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record DeviceStatusJ(
    string Hostname,
    string SoftwareVersion,
    string Model,
    string Uptime,
    double CpuUtilization,
    double MemoryUtilization);
```

`Models/DeviceInventory.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record HardwareComponentJ(string Name, string PartNumber, string Status);

public sealed record DeviceInventoryJ(
    string Model,
    string SerialNumber,
    string SoftwareVersion,
    string Memory,
    string Storage,
    IReadOnlyList<HardwareComponentJ> Components);
```

`Models/AlarmInfo.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

public sealed record AlarmInfoJ(
    string Timestamp,
    string Severity,
    string Description,
    string? Source);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet build Obss.sln --no-restore | tail -5 && dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~JuniperModelTests" --no-build`
Expected: Build succeeded + all tests PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/Models/
git add tests/Modules/Provisioning.Tests/Adapters/Juniper/Models/JuniperModelTests.cs
git commit -m "feat(juniper): add Juniper router adapter model types (10 records)"
```

---

### Task 7: Juniper Router — Interface, Config, Constants, Simulator

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/IJuniperRouterAdapter.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/JuniperAdapterConfig.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/JuniperAdapterConstants.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/JuniperRouterSimulator.cs`

**Test files:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Juniper/JuniperAdapterConfigTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Juniper/JuniperRouterSimulatorTests.cs`

Same structure as Cisco (Tasks 3-4) but with `IJuniperRouterAdapter : ICiscoRouterAdapter` equivalent interface, Juniper-specific YANG paths in `JuniperAdapterConstants`, and `JuniperRouterSimulator` using Juniper model types.

- [ ] **Step 1: Write config and simulator tests**

`JuniperAdapterConfigTests.cs`:
```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Juniper;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Juniper;

public class JuniperAdapterConfigTests
{
    [Fact]
    public void JuniperAdapterConfig_ShouldSetDefaultValues()
    {
        var config = new JuniperAdapterConfig { BaseUri = "https://device/restconf" };
        config.BaseUri.Should().Be("https://device/restconf");
        config.TimeoutSeconds.Should().Be(30);
    }
}
```

`JuniperRouterSimulatorTests.cs`:
```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Juniper;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Juniper;

public class JuniperRouterSimulatorTests
{
    private readonly JuniperRouterSimulator _simulator = new();

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldStoreInterface()
    {
        var units = new[] { new InterfaceUnit(0, new[] { "inet" }, "10.0.0.1", 24) };
        var config = new InterfaceConfig("ge-0/0/0", "GE", "Uplink", true, 1500, units);
        var result = await _simulator.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("ge-0/0/0");
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.Name.Should().Be("ge-0/0/0");
    }

    [Fact]
    public async Task ClearState_ShouldReset()
    {
        var config = new InterfaceConfig("ge-0/0/0", "GE", null, true, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        _simulator.ClearState();

        var result = await _simulator.GetInterfaceAsync("ge-0/0/0");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        var result = await _simulator.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("sim-juniper");
    }

    [Fact]
    public async Task ConfigureFirewallFilterAsync_ShouldStore()
    {
        var terms = new[] { new FirewallTerm("ALLOW", new[] { "10.0.0.0/24" }, "accept", null) };
        var filter = new FirewallFilter("MGMT", terms);
        var result = await _simulator.ConfigureFirewallFilterAsync(filter);
        result.IsSuccess.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~JuniperRouterSimulatorTests|JuniperAdapterConfigTests" --no-build`
Expected: FAIL

- [ ] **Step 3: Create IJuniperRouterAdapter**

Same shape as `ICiscoRouterAdapter` but uses Juniper model types:
```csharp
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Juniper;

public interface IJuniperRouterAdapter
{
    string AdapterName { get; }

    Task<AdapterResult<InterfaceConfig>> ConfigureInterfaceAsync(InterfaceConfig config);
    Task<AdapterResult<InterfaceConfig>> GetInterfaceAsync(string interfaceName);
    Task<AdapterResult<IReadOnlyList<InterfaceStatusJ>>> GetInterfaceStatusesAsync();
    Task<AdapterResult> DeleteInterfaceAsync(string interfaceName);

    Task<AdapterResult<BgpConfigJ>> ConfigureBgpAsync(BgpConfigJ config);
    Task<AdapterResult<BgpConfigJ>> GetBgpConfigAsync();
    Task<AdapterResult<OspfConfigJ>> ConfigureOspfAsync(OspfConfigJ config);
    Task<AdapterResult> ConfigureStaticRouteAsync(StaticRouteJ route);
    Task<AdapterResult<IReadOnlyList<StaticRouteJ>>> GetStaticRoutesAsync();

    Task<AdapterResult<SystemConfigJ>> ConfigureSystemAsync(SystemConfigJ config);
    Task<AdapterResult<SystemConfigJ>> GetSystemConfigAsync();

    Task<AdapterResult<FirewallFilter>> ConfigureFirewallFilterAsync(FirewallFilter filter);

    Task<AdapterResult<DeviceStatusJ>> GetDeviceStatusAsync();
    Task<AdapterResult<DeviceInventoryJ>> GetInventoryAsync();

    Task<AdapterResult<IReadOnlyList<AlarmInfoJ>>> GetActiveAlarmsAsync();
}
```

- [ ] **Step 4: Create JuniperAdapterConfig + Constants**

`JuniperAdapterConfig.cs` — same structure as `CiscoAdapterConfig`.

`JuniperAdapterConstants.cs` — with Juniper-specific YANG paths:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Juniper;

public static class JuniperAdapterConstants
{
    public const string AdapterName = "JUNIPER_ROUTER";
    public const string TechnologyType = "juniper_router";
    public const string InterfacePath = "/data/junos-configuration:configuration/interfaces";
    public const string BgpPath = "/data/junos-configuration:configuration/protocols/bgp";
    public const string OspfPath = "/data/junos-configuration:configuration/protocols/ospf";
    public const string StaticRoutePath = "/data/junos-configuration:configuration/routing-options/static";
    public const string SystemPath = "/data/junos-configuration:configuration/system";
    public const string FirewallPath = "/data/junos-configuration:configuration/firewall";
    public const string DeviceStatusPath = "/data/junos-system:system-information";
    public const string InventoryPath = "/data/junos-hardware:hardware-inventory";
    public const string AlarmsPath = "/data/junos-alarms:alarms";
}
```

- [ ] **Step 5: Create JuniperRouterSimulator**

Same structure as `CiscoRouterSimulator` but using Juniper model types (`InterfaceConfig` with `Units`, `FirewallFilter`, `BgpConfigJ`, etc.). Follow the exact same `ConcurrentDictionary` pattern.

- [ ] **Step 6: Build and run tests**

Run: `dotnet build Obss.sln --no-restore | tail -5`
Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~JuniperRouterSimulatorTests|JuniperAdapterConfigTests" --no-build`
Expected: All pass

- [ ] **Step 7: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/
git add tests/Modules/Provisioning.Tests/Adapters/Juniper/
git commit -m "feat(juniper): add Juniper router adapter interface, config, constants, and simulator"
```

---

### Task 8: Juniper Router — Real Adapter + Health Check + ProvisioningAdapter + DI

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/JuniperRouterAdapter.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/JuniperRouterHealthCheck.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/JuniperProvisioningAdapter.cs`

**Test files:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Juniper/JuniperRouterAdapterTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Juniper/JuniperRouterHealthCheckTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Juniper/JuniperProvisioningAdapterTests.cs`

Also modify the module registration file to wire up Juniper DI.

- [ ] **Step 1: Write adapter tests (WireMock)**

`JuniperRouterAdapterTests.cs` — same pattern as Cisco but using Juniper paths:

```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Juniper;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Juniper;

public class JuniperRouterAdapterTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly JuniperRouterAdapter _adapter;

    public JuniperRouterAdapterTests()
    {
        _server = WireMockServer.Start();
        var config = new JuniperAdapterConfig { BaseUri = _server.Url! };
        var transport = new RestconfTransport(new RestconfTransportConfig { BaseUri = _server.Url!, Name = "test-juniper" });
        _adapter = new JuniperRouterAdapter(config, transport);
    }

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/junos-configuration:configuration/interfaces/interface=ge-0%2F0%2F0").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var units = new[] { new InterfaceUnit(0, new[] { "inet" }, "10.0.0.1", 24) };
        var config = new InterfaceConfig("ge-0/0/0", "GE", "Uplink", true, 1500, units);
        var result = await _adapter.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInterfaceAsync_ShouldReturnInterface()
    {
        _server.Given(Request.Create().WithPath("/data/junos-configuration:configuration/interfaces/interface=ge-0%2F0%2F0").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"interface\":{\"name\":\"ge-0/0/0\",\"description\":\"Uplink\"}}"));

        var result = await _adapter.GetInterfaceAsync("ge-0/0/0");
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("ge-0/0/0");
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        _server.Given(Request.Create().WithPath("/data/junos-system:system-information").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"junos-system:system-information\":{\"hostname\":\"router01\"}}"));

        var result = await _adapter.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("router01");
    }

    [Fact]
    public async Task ConfigureFirewallFilterAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/junos-configuration:configuration/firewall").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var terms = new[] { new FirewallTerm("ALLOW-MGMT", new[] { "10.0.0.0/24" }, "accept", null) };
        var filter = new FirewallFilter("MGMT-FILTER", terms);
        var result = await _adapter.ConfigureFirewallFilterAsync(filter);
        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        _server?.Dispose();
        _adapter?.Dispose();
    }
}
```

`JuniperRouterHealthCheckTests.cs` and `JuniperProvisioningAdapterTests.cs` — same basic structure as Cisco equivalents.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~JuniperRouterAdapterTests|JuniperRouterHealthCheckTests|JuniperProvisioningAdapterTests" --no-build`
Expected: FAIL

- [ ] **Step 3: Create JuniperRouterAdapter**

Same pattern as `CiscoRouterAdapter` but:
- Uses `IJuniperRouterAdapter` interface
- Uses `JuniperAdapterConstants` for YANG paths
- Uses Juniper model types
- Implements `ConfigureFirewallFilterAsync` instead of `ConfigureAclAsync`
- Juniper payload shapes follow JunOS JSON structure (e.g., `interface { name; unit { family inet { address 10.0.0.1/24 } } }`)

- [ ] **Step 4: Create JuniperRouterHealthCheck + JuniperProvisioningAdapter**

Same patterns as Cisco equivalents, adapted for `IJuniperRouterAdapter`.

- [ ] **Step 5: Add Juniper DI wiring to module registration**

Same pattern as Cisco: register `IJuniperRouterAdapter` → `JuniperRouterAdapter`, `IProvisioningAdapter` → `JuniperProvisioningAdapter`, `IHealthCheck` → `JuniperRouterHealthCheck`, register in `AdapterRegistry` under `"JUNIPER_ROUTER"`.

- [ ] **Step 6: Build and run all Juniper tests**

Run: `dotnet build Obss.sln --no-restore | tail -5`
Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Juniper" --no-build`
Expected: All Juniper tests PASS

- [ ] **Step 7: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Juniper/
git add tests/Modules/Provisioning.Tests/Adapters/Juniper/
git commit -m "feat(juniper): add Juniper real adapter, health check, provisioning adapter, and DI wiring"
```

---

### Task 9: Nokia Router — Model Types

**Files (create all):**
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/InterfaceConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/InterfaceStatus.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/BgpConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/OspfConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/StaticRoute.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/SystemConfig.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/AclConfig.cs` (Nokia: `IpFilter`)
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/DeviceStatus.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/DeviceInventory.cs`
- `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/AlarmInfo.cs`

**Test file:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Nokia/Models/NokiaModelTests.cs`

Nokia SR OS YANG differs from both Cisco and Juniper:
- Ports under `nokia-conf-port:configure port`
- Routers under `nokia-conf-router:configure router`
- System under `nokia-conf-system:configure system`
- Filters (ACLs) under `nokia-conf-filter:configure filter`

- [ ] **Step 1: Write Nokia model tests**

```csharp
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Nokia.Models;

public class NokiaModelTests
{
    [Fact]
    public void InterfaceConfig_ShouldPreserveProperties()
    {
        var config = new InterfaceConfigN("1/1/1", "port", "Uplink", true, 1500, 100);
        config.Name.Should().Be("1/1/1");
        config.PortType.Should().Be("port");
        config.Mtu.Should().Be(1500);
    }

    [Fact]
    public void BgpConfigN_ShouldPreserveProperties()
    {
        var neighbor = new BgpNeighborN("10.0.0.2", 65002, "external");
        var bgp = new BgpConfigN(65001, "10.0.0.1", new[] { neighbor });
        bgp.AsNumber.Should().Be(65001);
        bgp.Neighbors.Should().Contain(n => n.PeerAs == 65002);
    }

    [Fact]
    public void IpFilter_ShouldPreserveProperties()
    {
        var entry = new IpFilterEntry(10, "drop", "10.0.0.0/24", null);
        var filter = new IpFilter("BLOCK-MGMT", new[] { entry });
        filter.Name.Should().Be("BLOCK-MGMT");
        filter.Entries.Should().Contain(e => e.Action == "drop");
    }

    [Fact]
    public void DeviceStatusN_ShouldPreserveProperties()
    {
        var status = new DeviceStatusN("router01", "20.10.R1", "7750 SR-12", "50 days", 30.0, 55.0);
        status.Hostname.Should().Be("router01");
        status.Model.Should().Be("7750 SR-12");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~NokiaModelTests" --no-build`
Expected: FAIL

- [ ] **Step 3: Create all Nokia model records**

Create directory `Adapters/Nokia/Models/` with Nokia-specific record types:

`Models/InterfaceConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record InterfaceConfigN(
    string Name,
    string PortType,
    string? Description,
    bool? AdminUp,
    int? Mtu,
    int? LagId);
```

`Models/InterfaceStatus.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record InterfaceStatusN(
    string InterfaceName,
    string AdminStatus,
    string OperStatus,
    long Speed);
```

`Models/BgpConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record BgpNeighborN(string PeerAddress, int PeerAs, string? PeerGroup);

public sealed record BgpConfigN(
    int AsNumber,
    string? RouterId,
    IReadOnlyList<BgpNeighborN>? Neighbors);
```

`Models/OspfConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record OspfAreaN(int AreaId, IReadOnlyList<string>? Interfaces);

public sealed record OspfConfigN(
    int ProcessId,
    string? RouterId,
    IReadOnlyList<OspfAreaN>? Areas);
```

`Models/StaticRoute.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record StaticRouteN(
    string Prefix,
    string NextHop,
    int? Metric,
    string? Interface);
```

`Models/SystemConfig.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record SystemConfigN(
    string? Hostname,
    string? DomainName,
    IReadOnlyList<string>? NtpServers,
    IReadOnlyList<string>? DnsServers);
```

`Models/AclConfig.cs` (Nokia: IP filters):
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record IpFilterEntry(
    int Sequence,
    string Action,
    string SourcePrefix,
    string? DestinationPrefix);

public sealed record IpFilter(
    string Name,
    IReadOnlyList<IpFilterEntry> Entries);
```

`Models/DeviceStatus.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record DeviceStatusN(
    string Hostname,
    string SoftwareVersion,
    string Model,
    string Uptime,
    double CpuUtilization,
    double MemoryUtilization);
```

`Models/DeviceInventory.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record HardwareComponentN(string Name, string PartNumber, string Status);

public sealed record DeviceInventoryN(
    string Model,
    string SerialNumber,
    string SoftwareVersion,
    string Memory,
    string Storage,
    IReadOnlyList<HardwareComponentN> Components);
```

`Models/AlarmInfo.cs`:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

public sealed record AlarmInfoN(
    string Timestamp,
    string Severity,
    string Description,
    string? Source);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet build Obss.sln --no-restore | tail -5 && dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~NokiaModelTests" --no-build`
Expected: Build succeeded + tests PASS

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/Models/
git add tests/Modules/Provisioning.Tests/Adapters/Nokia/Models/NokiaModelTests.cs
git commit -m "feat(nokia): add Nokia router adapter model types (10 records)"
```

---

### Task 10: Nokia Router — Interface, Config, Constants, Simulator

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/INokiaRouterAdapter.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/NokiaAdapterConfig.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/NokiaAdapterConstants.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/NokiaRouterSimulator.cs`

**Test files:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Nokia/NokiaAdapterConfigTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Nokia/NokiaRouterSimulatorTests.cs`

Same structure as Cisco/Juniper. `INokiaRouterAdapter` uses Nokia model types and includes `ConfigureIpFilterAsync` instead of `ConfigureAclAsync`/`ConfigureFirewallFilterAsync`.

- [ ] **Step 1: Write config and simulator tests**

Follow the same pattern as Cisco (`CiscoAdapterConfigTests.cs` / `CiscoRouterSimulatorTests.cs`) with Nokia model types.

- [ ] **Step 2: Run tests to verify they fail**

- [ ] **Step 3: Create INokiaRouterAdapter**

```csharp
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;

namespace Obss.Provisioning.Infrastructure.Adapters.Nokia;

public interface INokiaRouterAdapter
{
    string AdapterName { get; }

    Task<AdapterResult<InterfaceConfigN>> ConfigureInterfaceAsync(InterfaceConfigN config);
    Task<AdapterResult<InterfaceConfigN>> GetInterfaceAsync(string interfaceName);
    Task<AdapterResult<IReadOnlyList<InterfaceStatusN>>> GetInterfaceStatusesAsync();
    Task<AdapterResult> DeleteInterfaceAsync(string interfaceName);

    Task<AdapterResult<BgpConfigN>> ConfigureBgpAsync(BgpConfigN config);
    Task<AdapterResult<BgpConfigN>> GetBgpConfigAsync();
    Task<AdapterResult<OspfConfigN>> ConfigureOspfAsync(OspfConfigN config);
    Task<AdapterResult> ConfigureStaticRouteAsync(StaticRouteN route);
    Task<AdapterResult<IReadOnlyList<StaticRouteN>>> GetStaticRoutesAsync();

    Task<AdapterResult<SystemConfigN>> ConfigureSystemAsync(SystemConfigN config);
    Task<AdapterResult<SystemConfigN>> GetSystemConfigAsync();

    Task<AdapterResult<IpFilter>> ConfigureIpFilterAsync(IpFilter filter);

    Task<AdapterResult<DeviceStatusN>> GetDeviceStatusAsync();
    Task<AdapterResult<DeviceInventoryN>> GetInventoryAsync();

    Task<AdapterResult<IReadOnlyList<AlarmInfoN>>> GetActiveAlarmsAsync();
}
```

- [ ] **Step 4: Create NokiaAdapterConfig + NokiaAdapterConstants**

`NokiaAdapterConfig.cs` — same as `CiscoAdapterConfig`.

`NokiaAdapterConstants.cs` with Nokia SR OS YANG paths:
```csharp
namespace Obss.Provisioning.Infrastructure.Adapters.Nokia;

public static class NokiaAdapterConstants
{
    public const string AdapterName = "NOKIA_ROUTER";
    public const string TechnologyType = "nokia_router";
    public const string InterfacePath = "/data/nokia-conf-port:configure port";
    public const string BgpPath = "/data/nokia-conf-router:configure router/*/bgp";
    public const string OspfPath = "/data/nokia-conf-router:configure router/*/ospf";
    public const string StaticRoutePath = "/data/nokia-conf-router:configure router/*/static-route";
    public const string SystemPath = "/data/nokia-conf-system:configure system";
    public const string FilterPath = "/data/nokia-conf-filter:configure filter/ip-filter";
    public const string DeviceStatusPath = "/data/nokia-state-system:state/system";
    public const string InventoryPath = "/data/nokia-state-hardware:state/hardware";
    public const string AlarmsPath = "/data/nokia-state-alarms:state/alarms";
}
```

- [ ] **Step 5: Create NokiaRouterSimulator**

Same pattern as `CiscoRouterSimulator` but using Nokia model types and `ConfigureIpFilterAsync` method.

- [ ] **Step 6: Build and run tests**

- [ ] **Step 7: Commit**

---

### Task 11: Nokia Router — Real Adapter + Health Check + ProvisioningAdapter + DI

**Files:**
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/NokiaRouterAdapter.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/NokiaRouterHealthCheck.cs`
- Create: `src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Adapters/Nokia/NokiaProvisioningAdapter.cs`

**Test files:**
- Create: `tests/Modules/Provisioning.Tests/Adapters/Nokia/NokiaRouterAdapterTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Nokia/NokiaRouterHealthCheckTests.cs`
- Create: `tests/Modules/Provisioning.Tests/Adapters/Nokia/NokiaProvisioningAdapterTests.cs`

- [ ] **Step 1: Write WireMock adapter tests for Nokia**

Follow the same pattern as `CiscoRouterAdapterTests.cs` but using Nokia paths (`/data/nokia-conf-port:configure port/...`) and Nokia model types.

- [ ] **Step 2: Run tests to verify they fail**

- [ ] **Step 3: Create NokiaRouterAdapter**

Same pattern as `CiscoRouterAdapter` but:
- Uses `INokiaRouterAdapter`
- Uses `NokiaAdapterConstants` for YANG paths
- Nokia payload shapes follow SR OS JSON structure
- Implements `ConfigureIpFilterAsync` instead of `ConfigureAclAsync`

- [ ] **Step 4: Create NokiaRouterHealthCheck + NokiaProvisioningAdapter**

Same pattern as Cisco equivalents.

- [ ] **Step 5: Add Nokia DI wiring to module registration**

Same pattern: register `INokiaRouterAdapter` → `NokiaRouterAdapter`, `IProvisioningAdapter` → `NokiaProvisioningAdapter`, `IHealthCheck` → `NokiaRouterHealthCheck`, register in `AdapterRegistry` under `"NOKIA_ROUTER"`.

- [ ] **Step 6: Build and run all Nokia tests**

- [ ] **Step 7: Commit**

---

### Task 12: End-to-End Build Verification

- [ ] **Step 1: Full solution build**

Run: `dotnet build Obss.sln 2>&1 | tail -5`
Expected: Build succeeded. 0 Warning(s) 0 Error(s)

- [ ] **Step 2: Run all RESTCONF tests (Phase 1 regression check)**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Restconf" --no-build`
Expected: All 51+ PASS

- [ ] **Step 3: Run all Cisco tests**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Cisco" --no-build`
Expected: All PASS

- [ ] **Step 4: Run all Juniper tests**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Juniper" --no-build`
Expected: All PASS

- [ ] **Step 5: Run all Nokia tests**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --filter "FullyQualifiedName~Nokia" --no-build`
Expected: All PASS

- [ ] **Step 6: Run full Provisioning test suite**

Run: `dotnet test tests/Modules/Provisioning.Tests/ --no-build 2>&1 | tail -10`
Expected: No regressions against pre-existing 7 Docker-dependent failures

- [ ] **Step 7: Final commit any remaining changes**

```bash
git add -A
git commit -m "feat(routers): finalize multi-vendor router adapters (Cisco, Juniper, Nokia)"
```
