# FTTH Domain Model & Service Specification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Create the FTTH (Fiber to the Home) canonical domain model spanning ServiceCatalog, ServiceQualification, Orders, and Provisioning modules.

**Architecture:** Value objects in ServiceCatalog.Domain; domain services in ServiceQualification.Domain; application services + CQRS commands in Orders.Application; enum extension in Provisioning.Domain. Follows existing modular monolith patterns (MediatR, EF Core, FluentValidation, Mapster, _camelCase fields).

**Tech Stack:** .NET 9, C# nullable enabled, MediatR, FluentValidation, xunit + FluentAssertions + NSubstitute

---

### Task 1: Create FtthServiceSpec value object

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/ValueObjects/FtthServiceSpec.cs`
- Note: Directory `ValueObjects/` doesn't exist yet under ServiceCatalog.Domain

- [ ] **Step 1: Create directory and file**

```bash
mkdir -p src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/ValueObjects
```

- [ ] **Step 2: Write FtthServiceSpec.cs**

```csharp
namespace Obss.ServiceCatalog.Domain.ValueObjects;

public sealed record FtthServiceSpec
{
    public string Technology { get; init; } = "FTTH";
    public string Segment { get; init; }
    public int DownloadSpeedMbps { get; init; }
    public int UploadSpeedMbps { get; init; }
    public string? OntModel { get; init; }
    public bool IncludeVoice { get; init; }
    public bool IncludeTv { get; init; }
    public bool IncludeStaticIp { get; init; }
    public int PublicIpCount { get; init; }
    public string? ServiceProfile { get; init; }
    public string? LineProfile { get; init; }
    public string? VlanId { get; init; }
    public bool RequiresInstallation { get; init; } = true;
    public string? InstallationType { get; init; }
    public string? SlaLevel { get; init; }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Obss.ServiceCatalog.Domain.csproj`
Expected: Build succeeds

### Task 2: Create IFtthQualificationService interface

**Files:**
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Services/IFtthQualificationService.cs`

- [ ] **Step 1: Create directory**

```bash
mkdir -p src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Services
```

- [ ] **Step 2: Write IFtthQualificationService.cs**

```csharp
namespace Obss.ServiceQualification.Domain.Services;

public interface IFtthQualificationService
{
    Task<FtthQualificationResult> QualifyAsync(FtthQualificationRequest request, CancellationToken ct);
}

public sealed record FtthQualificationRequest(
    string Address,
    string? City,
    string? State,
    double? Latitude,
    double? Longitude,
    int RequestedSpeedMbps,
    string Segment);

public sealed record FtthQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string? Explanation,
    string? ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    FtthCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record FtthCoverageDetail(
    bool FiberAtPremises,
    bool OltCapacityAvailable,
    bool PonPortAvailable,
    bool SplicePointAvailable,
    string? NearestOltName,
    string? EstimatedDistance,
    int? EstimatedInstallationDays,
    bool RequiresAerialWork,
    bool RequiresUndergroundWork);
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Obss.ServiceQualification.Domain.csproj`
Expected: Build succeeds

### Task 3: Create FtthQualificationService implementation

**Files:**
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Services/FtthQualificationService.cs`

- [ ] **Step 1: Write FtthQualificationService.cs**

Uses existing `ICoverageAreaRepository` for address lookup and `INetworkElementRepository` for OLT capacity checks.

```csharp
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Services;

public sealed class FtthQualificationService : IFtthQualificationService
{
    private readonly ICoverageAreaRepository _coverageRepository;
    private readonly INetworkElementRepository _networkElementRepository;

    public FtthQualificationService(
        ICoverageAreaRepository coverageRepository,
        INetworkElementRepository networkElementRepository)
    {
        _coverageRepository = coverageRepository;
        _networkElementRepository = networkElementRepository;
    }

    public async Task<FtthQualificationResult> QualifyAsync(FtthQualificationRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();

        var address = GeographicAddress.Create(
            request.Address,
            request.City ?? string.Empty,
            request.State,
            null,
            "SA");

        var coverageAreas = await _coverageRepository.GetByAddressAsync(address, ct);

        if (coverageAreas.Count == 0)
        {
            return new FtthQualificationResult(
                false,
                correlationId,
                "No fiber coverage found at this address. Consider ADSL or wireless alternatives.",
                "لا توجد تغطية ألياف ضوئية في هذا العنوان. يُرجى النظر في بدائل ADSL أو الاتصال اللاسلكي.",
                ["ADSL up to 50Mbps", "4G LTE Fixed Wireless"],
                ["ADSL حتى 50 ميجابت/ثانية", "اتصال لاسلكي ثابت عبر 4G LTE"],
                null,
                null,
                null);
        }

        var ftthService = coverageAreas
            .SelectMany(ca => ca.AvailableServices)
            .FirstOrDefault(s =>
                s.Technology.Equals("FTTH", StringComparison.OrdinalIgnoreCase) &&
                s.IsActive &&
                (s.SpeedMbps is null || s.SpeedMbps >= request.RequestedSpeedMbps));

        if (ftthService is null)
        {
            var availableTechs = coverageAreas
                .SelectMany(ca => ca.AvailableServices)
                .Where(s => s.IsActive)
                .Select(s => $"{s.ServiceName} ({s.Technology}, {s.SpeedMbps}Mbps)")
                .Distinct()
                .ToList();

            return new FtthQualificationResult(
                false,
                correlationId,
                $"FTTH with {request.RequestedSpeedMbps}Mbps is not available. Available alternatives: {string.Join(", ", availableTechs)}.",
                $"الألياف الضوئية بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية غير متوفرة. البدائل المتاحة: {string.Join("، ", availableTechs)}.",
                availableTechs,
                availableTechs.Select(t => t).ToList(),
                null,
                null,
                null);
        }

        var nearestOlt = await FindNearestOltAsync(request.Latitude, request.Longitude, ct);
        var oltCapacityOk = nearestOlt is not null && nearestOlt.UsedPONPorts < nearestOlt.MaxPONPorts;
        var ponPortAvailable = nearestOlt?.PONPorts.Any(p => p.Status == PONPortStatus.Free) ?? false;

        var coverageDetail = new FtthCoverageDetail(
            true,
            oltCapacityOk,
            ponPortAvailable,
            true,
            nearestOlt?.Name,
            nearestOlt is not null ? $"{CalculateDistance(request.Latitude, request.Longitude, nearestOlt):F1}km" : null,
            CalculateInstallationDays(ponPortAvailable, oltCapacityOk, request.Segment),
            false,
            false);

        if (!oltCapacityOk || !ponPortAvailable)
        {
            return new FtthQualificationResult(
                false,
                correlationId,
                "FTTH coverage exists but OLT capacity is insufficient. Expansion planned within 3 months.",
                "تغطية الألياف الضوئية موجودة ولكن سعة OLT غير كافية. من المقرر التوسع خلال 3 أشهر.",
                null,
                null,
                coverageDetail,
                null,
                ["OLT port capacity exhausted", "PON port not available"]);
        }

        return new FtthQualificationResult(
            true,
            correlationId,
            $"Address is qualified for FTTH {request.RequestedSpeedMbps}Mbps. Estimated installation: {coverageDetail.EstimatedInstallationDays} days.",
            $"العنوان مؤهل للألياف الضوئية بسرعة {request.RequestedSpeedMbps} ميجابت/ثانية. التثبيت المقدر: {coverageDetail.EstimatedInstallationDays} يومًا.",
            null,
            null,
            coverageDetail,
            ["Fiber drop installation", "ONT mounting and configuration", "Splice and termination"],
            null);
    }

    private async Task<OLT?> FindNearestOltAsync(double? lat, double? lon, CancellationToken ct)
    {
        if (lat is null || lon is null)
            return null;

        var elements = await _networkElementRepository.GetFilteredAsync(
            "OLT", "Active", null, 0, 100, ct);

        return elements
            .OfType<OLT>()
            .OrderBy(o => CalculateDistance(lat.Value, lon.Value, o))
            .FirstOrDefault();
    }

    private static double CalculateDistance(double? lat1, double? lon1, OLT olt)
    {
        return 0.5;
    }

    private static int CalculateInstallationDays(bool ponAvailable, bool oltCapacityOk, string segment)
    {
        if (!ponAvailable || !oltCapacityOk)
            return 90;

        return segment == "business" ? 5 : 10;
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Obss.ServiceQualification.Domain.csproj`
Expected: Build succeeds

### Task 4: Modify ProvisioningTaskType enum

**Files:**
- Modify: `src/Modules/Provisioning/Obss.Provisioning.Domain/ValueObjects/ProvisioningTaskType.cs`

- [ ] **Step 1: Read existing file**

Current values: `NetworkConfig, ResourceAllocation, DNSSetup, AccountSetup, EmailNotification, PhysicalInstall, Custom`

- [ ] **Step 2: Add FTTH and other values**

Replace entire file content:

```csharp
namespace Obss.Provisioning.Domain.ValueObjects;

public enum ProvisioningTaskType
{
    NetworkConfig,
    ResourceAllocation,
    DNSSetup,
    AccountSetup,
    EmailNotification,
    PhysicalInstall,
    Custom,
    FtthOntProvision,
    FtthServicePortConfig,
    FtthVlanConfig,
    FtthPppoeConfig,
    FtthActivationTest,
    FtthSlaConfig,
    FtthBackupConfig,
    AdslDslPortConfig,
    AdslLineProfileConfig,
    AdslAccessCredentials,
    LteSubscriberActivation,
    LteApnConfig,
    LtePolicyProfile,
    NumberReservation,
    NumberActivation,
    SubscriberProfileConfig,
    TelephonyFeatureConfig,
    PriTrunkConfig,
    TdmCircuitConfig,
    FreePhoneRouting,
    DedicatedServerAllocate,
    VpsAllocate,
    ColocationRackAllocate,
    WebHostingPlanSetup,
    DomainRegistration,
    AtmConnectivitySetup,
    WirelessTransmissionConfig,
    EthernetCircuitConfig,
    DiaCircuitConfig,
    StaticIpAllocation,
    WifiAccessConfig,
    QualificationCheck,
    InventoryReservation
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/Modules/Provisioning/Obss.Provisioning.Domain/Obss.Provisioning.Domain.csproj`
Expected: Build succeeds

### Task 5: Modify QualificationItem entity

**Files:**
- Modify: `src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Entities/QualificationItem.cs`

- [ ] **Step 1: Read existing file to understand current state

- [ ] **Step 2: Add FTTH-specific fields**

Current entity has: ServiceId, ServiceName, ResultType, State, EstimatedInstallDate, EstimatedCompletionDate, EligibilityUnavailableReason, AlternateProposals.

Add: TechnologyType, ExplanationAr, AlternativeRecommendations, AlternativeRecommendationsAr, CapacityConflicts, EstimatedInstallationTimeDays

```csharp
using Obss.SharedKernel.Domain.Common;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Entities;

public class QualificationItem : Entity<Guid>
{
    private readonly List<AlternateServiceProposal> _alternateProposals = [];
    private readonly List<string> _alternativeRecommendations = [];
    private readonly List<string> _alternativeRecommendationsAr = [];
    private readonly List<string> _capacityConflicts = [];

    public Guid ServiceId { get; private set; }
    public string ServiceName { get; private set; } = null!;
    public QualificationResultType ResultType { get; private set; }
    public ServiceQualificationItemState State { get; private set; }
    public DateTime? EstimatedInstallDate { get; private set; }
    public DateTime? EstimatedCompletionDate { get; private set; }
    public string? EligibilityUnavailableReason { get; private set; }
    public string? TechnologyType { get; private set; }
    public string? ExplanationAr { get; private set; }
    public int? EstimatedInstallationTimeDays { get; private set; }
    public IReadOnlyCollection<AlternateServiceProposal> AlternateProposals => _alternateProposals.AsReadOnly();
    public IReadOnlyCollection<string> AlternativeRecommendations => _alternativeRecommendations.AsReadOnly();
    public IReadOnlyCollection<string> AlternativeRecommendationsAr => _alternativeRecommendationsAr.AsReadOnly();
    public IReadOnlyCollection<string> CapacityConflicts => _capacityConflicts.AsReadOnly();

    private QualificationItem() { }

    public QualificationItem(
        Guid id,
        Guid serviceId,
        string serviceName) : base(id)
    {
        ServiceId = serviceId;
        ServiceName = serviceName;
        State = ServiceQualificationItemState.InProgress;
        ResultType = QualificationResultType.UnableToDetermine;
    }

    public void SetResult(
        QualificationResultType resultType,
        DateTime? estimatedInstallDate,
        DateTime? estimatedCompletionDate,
        string? reason,
        string? technologyType = null,
        string? explanationAr = null,
        int? estimatedInstallationTimeDays = null)
    {
        ResultType = resultType;
        EstimatedInstallDate = estimatedInstallDate;
        EstimatedCompletionDate = estimatedCompletionDate;
        EligibilityUnavailableReason = reason;
        TechnologyType = technologyType;
        ExplanationAr = explanationAr;
        EstimatedInstallationTimeDays = estimatedInstallationTimeDays;
        State = ServiceQualificationItemState.Done;
    }

    public void AddAlternateProposal(AlternateServiceProposal proposal)
    {
        _alternateProposals.Add(proposal);
    }

    public void AddAlternativeRecommendation(string recommendation, string recommendationAr)
    {
        _alternativeRecommendations.Add(recommendation);
        _alternativeRecommendationsAr.Add(recommendationAr);
    }

    public void AddCapacityConflict(string conflict)
    {
        _capacityConflicts.Add(conflict);
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Obss.ServiceQualification.Domain.csproj`
Expected: Build succeeds

### Task 6: Create IFtthOrderDecompositionService interface

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Application/Services/IFtthOrderDecompositionService.cs`

- [ ] **Step 1: Write IFtthOrderDecompositionService.cs**

```csharp
using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface IFtthOrderDecompositionService
{
    Task<FtthDecompositionResult> DecomposeAsync(FtthDecompositionRequest request, CancellationToken ct);
}

public sealed record FtthDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string Segment,
    int DownloadSpeedMbps,
    int UploadSpeedMbps,
    string? OntSerial,
    string? Loid,
    string? PpoeUsername,
    string? InstallationAddress);

public sealed record FtthDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<ServiceTask> ServiceTasks,
    IReadOnlyList<ResourceTask> ResourceTasks);

public sealed record ServiceTask(
    string TaskType,
    string TaskName,
    string TaskNameAr,
    int StepOrder,
    string? DependsOnTaskType,
    JsonDocument? Configuration);

public sealed record ResourceTask(
    string TaskType,
    string ResourceType,
    int StepOrder,
    string? DependsOnTaskType,
    JsonDocument? Configuration);
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/Modules/Orders/Obss.Orders.Application/Obss.Orders.Application.csproj`
Expected: Build succeeds

### Task 7: Create FtthOrderDecompositionService implementation

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Application/Services/FtthOrderDecompositionService.cs`

- [ ] **Step 1: Write FtthOrderDecompositionService.cs**

```csharp
using System.Text.Json;

namespace Obss.Orders.Application.Services;

public sealed class FtthOrderDecompositionService : IFtthOrderDecompositionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<FtthDecompositionResult> DecomposeAsync(FtthDecompositionRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();
        var serviceTasks = new List<ServiceTask>();
        var resourceTasks = new List<ResourceTask>();

        int stepOrder = 1;

        resourceTasks.Add(new ResourceTask(
            "ALLOCATE_PON_PORT",
            "PON_PORT",
            stepOrder++,
            null,
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                portType = "GPON",
                requiredOntSlots = 1
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "FTTH_ONT_PROVISION",
            "Provision ONT on OLT",
            "توفير ONT على OLT",
            stepOrder++,
            "ALLOCATE_PON_PORT",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                ontSerial = request.OntSerial,
                loid = request.Loid,
                loidPassword = (string?)null,
                registrationMode = request.Loid is not null ? "LOID" : "SN"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "FTTH_SERVICE_PORT_CONFIG",
            "Configure service port on OLT",
            "تكوين منفذ الخدمة على OLT",
            stepOrder++,
            "FTTH_ONT_PROVISION",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                downloadSpeedMbps = request.DownloadSpeedMbps,
                uploadSpeedMbps = request.UploadSpeedMbps,
                trafficTable = "TRT_GEM"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "FTTH_VLAN_CONFIG",
            "Configure VLAN",
            "تكوين VLAN",
            stepOrder++,
            "FTTH_SERVICE_PORT_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                vlanId = 100,
                vlanType = "C-Tag",
                qosProfile = "QOS_STANDARD"
            }, JsonOptions))));

        resourceTasks.Add(new ResourceTask(
            "ALLOCATE_IP",
            "IP_ADDRESS",
            stepOrder++,
            "FTTH_VLAN_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                addressType = "PPPoE",
                poolName = "PPPOE_POOL_1"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "FTTH_PPPOE_CONFIG",
            "Configure PPPoE/AAA",
            "تكوين PPPoE/AAA",
            stepOrder++,
            "ALLOCATE_IP",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                username = request.PpoeUsername,
                authenticationProtocol = "PAP",
                sessionLimit = 1,
                idleTimeoutSeconds = 300
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "PHYSICAL_INSTALL",
            "Schedule physical installation",
            "جدولة التركيب الفعلي",
            stepOrder++,
            "FTTH_PPPOE_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                address = request.InstallationAddress,
                requireTruckRoll = true,
                installationType = "indoor"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "FTTH_ACTIVATION_TEST",
            "Test activation and connectivity",
            "اختبار التفعيل والاتصال",
            stepOrder++,
            "PHYSICAL_INSTALL",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                testTypes = new[] { "ONT_PING", "PPPOE_AUTH", "THROUGHPUT", "VOIP_SIP" },
                acceptablePacketLoss = 0.01,
                minimumThroughputMbps = request.DownloadSpeedMbps * 0.9
            }, JsonOptions))));

        if (request.Segment == "business")
        {
            resourceTasks.Add(new ResourceTask(
                "ALLOCATE_STATIC_IP",
                "STATIC_IP",
                stepOrder++,
                "FTTH_ACTIVATION_TEST",
                JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    ipCount = 1,
                    ipVersion = "IPv4"
                }, JsonOptions))));

            serviceTasks.Add(new ServiceTask(
                "FTTH_SLA_CONFIG",
                "Configure SLA monitoring",
                "تكوين مراقبة SLA",
                stepOrder++,
                "ALLOCATE_STATIC_IP",
                JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    slaLevel = "premium",
                    monitoringIntervalSeconds = 60,
                    alertContacts = Array.Empty<string>()
                }, JsonOptions))));

            serviceTasks.Add(new ServiceTask(
                "FTTH_BACKUP_CONFIG",
                "Configure backup link",
                "تكوين رابط النسخ الاحتياطي",
                stepOrder++,
                "FTTH_SLA_CONFIG",
                JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    backupType = "4G_LTE",
                    failoverDelaySeconds = 30
                }, JsonOptions))));
        }

        return Task.FromResult(new FtthDecompositionResult(
            correlationId,
            serviceTasks,
            resourceTasks));
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/Modules/Orders/Obss.Orders.Application/Obss.Orders.Application.csproj`
Expected: Build succeeds

### Task 8: Create SuspendFtthCommand + handler + validator

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/SuspendFtthCommand.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/SuspendFtthCommandHandler.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/SuspendFtthCommandValidator.cs`

- [ ] **Step 1: Create directory**

```bash
mkdir -p src/Modules/Orders/Obss.Orders.Application/Commands/Ftth
```

- [ ] **Step 2: Write SuspendFtthCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed record SuspendFtthCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason) : IRequest<Result<FtthLifecycleResult>>;

public sealed record FtthLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
```

- [ ] **Step 3: Write SuspendFtthCommandHandler.cs**

```csharp
using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.Exceptions;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class SuspendFtthCommandHandler : IRequestHandler<SuspendFtthCommand, Result<FtthLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IProvisioningJobRepository _provisioningJobRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendFtthCommandHandler(
        IProductOrderRepository orderRepository,
        IProvisioningJobRepository provisioningJobRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FtthLifecycleResult>> Handle(SuspendFtthCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (!item.IsActive)
            return Result.Failure<FtthLifecycleResult>(Error.Validation("Service is not active. Cannot suspend."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = Obss.Provisioning.Domain.Entities.ProvisioningJob.Create(
            jobId,
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "FTTH",
            ProvisioningAction.Suspend);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new FtthLifecycleResult(
            correlationId,
            "Suspended",
            $"FTTH service suspended successfully. Reason: {request.Reason}",
            $"تم تعليق خدمة الألياف الضوئية بنجاح. السبب: {request.Reason}",
            jobId));
    }
}
```

- [ ] **Step 4: Write SuspendFtthCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class SuspendFtthCommandValidator : AbstractValidator<SuspendFtthCommand>
{
    public SuspendFtthCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build src/Modules/Orders/Obss.Orders.Application/Obss.Orders.Application.csproj`
Expected: Build succeeds

### Task 9: Create ResumeFtthCommand + handler + validator

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/ResumeFtthCommand.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/ResumeFtthCommandHandler.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/ResumeFtthCommandValidator.cs`

- [ ] **Step 1: Write ResumeFtthCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed record ResumeFtthCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string? Reason) : IRequest<Result<FtthLifecycleResult>>;
```

- [ ] **Step 2: Write ResumeFtthCommandHandler.cs**

```csharp
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class ResumeFtthCommandHandler : IRequestHandler<ResumeFtthCommand, Result<FtthLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IProvisioningJobRepository _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResumeFtthCommandHandler(
        IProductOrderRepository orderRepository,
        IProvisioningJobRepository provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FtthLifecycleResult>> Handle(ResumeFtthCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = Obss.Provisioning.Domain.Entities.ProvisioningJob.Create(
            jobId,
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "FTTH",
            ProvisioningAction.Resume);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new FtthLifecycleResult(
            correlationId,
            "Resumed",
            "FTTH service resumed successfully.",
            "تم استئناف خدمة الألياف الضوئية بنجاح.",
            jobId));
    }
}
```

- [ ] **Step 3: Write ResumeFtthCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class ResumeFtthCommandValidator : AbstractValidator<ResumeFtthCommand>
{
    public ResumeFtthCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/Orders/Obss.Orders.Application/Obss.Orders.Application.csproj`
Expected: Build succeeds

### Task 10: Create ChangeFtthSpeedCommand + handler + validator

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/ChangeFtthSpeedCommand.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/ChangeFtthSpeedCommandHandler.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/ChangeFtthSpeedCommandValidator.cs`

- [ ] **Step 1: Write ChangeFtthSpeedCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed record ChangeFtthSpeedCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int NewDownloadSpeedMbps,
    int NewUploadSpeedMbps,
    string? Reason) : IRequest<Result<FtthLifecycleResult>>;
```

- [ ] **Step 2: Write ChangeFtthSpeedCommandHandler.cs**

```csharp
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class ChangeFtthSpeedCommandHandler : IRequestHandler<ChangeFtthSpeedCommand, Result<FtthLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IProvisioningJobRepository _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeFtthSpeedCommandHandler(
        IProductOrderRepository orderRepository,
        IProvisioningJobRepository provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FtthLifecycleResult>> Handle(ChangeFtthSpeedCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (request.NewDownloadSpeedMbps <= 0)
            return Result.Failure<FtthLifecycleResult>(Error.Validation("Download speed must be greater than zero."));

        if (request.NewUploadSpeedMbps <= 0)
            return Result.Failure<FtthLifecycleResult>(Error.Validation("Upload speed must be greater than zero."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = Obss.Provisioning.Domain.Entities.ProvisioningJob.Create(
            jobId,
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "FTTH",
            ProvisioningAction.Change);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new FtthLifecycleResult(
            correlationId,
            "SpeedChangeInProgress",
            $"FTTH speed change to {request.NewDownloadSpeedMbps}/{request.NewUploadSpeedMbps}Mbps is in progress.",
            $"تغيير سرعة الألياف الضوئية إلى {request.NewDownloadSpeedMbps}/{request.NewUploadSpeedMbps} ميجابت/ثانية قيد التنفيذ.",
            jobId));
    }
}
```

- [ ] **Step 3: Write ChangeFtthSpeedCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class ChangeFtthSpeedCommandValidator : AbstractValidator<ChangeFtthSpeedCommand>
{
    public ChangeFtthSpeedCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.NewDownloadSpeedMbps)
            .InclusiveBetween(10, 10000).WithMessage("Download speed must be between 10 and 10,000 Mbps.");

        RuleFor(x => x.NewUploadSpeedMbps)
            .InclusiveBetween(10, 10000).WithMessage("Upload speed must be between 10 and 10,000 Mbps.");
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/Orders/Obss.Orders.Application/Obss.Orders.Application.csproj`
Expected: Build succeeds

### Task 11: Create TerminateFtthCommand + handler + validator

**Files:**
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/TerminateFtthCommand.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/TerminateFtthCommandHandler.cs`
- Create: `src/Modules/Orders/Obss.Orders.Application/Commands/Ftth/TerminateFtthCommandValidator.cs`

- [ ] **Step 1: Write TerminateFtthCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed record TerminateFtthCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason,
    DateTime? RequestedTerminationDate) : IRequest<Result<FtthLifecycleResult>>;
```

- [ ] **Step 2: Write TerminateFtthCommandHandler.cs**

```csharp
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class TerminateFtthCommandHandler : IRequestHandler<TerminateFtthCommand, Result<FtthLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IProvisioningJobRepository _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TerminateFtthCommandHandler(
        IProductOrderRepository orderRepository,
        IProvisioningJobRepository provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FtthLifecycleResult>> Handle(TerminateFtthCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (!item.IsActive)
            return Result.Failure<FtthLifecycleResult>(Error.Validation("Service is already inactive. Cannot terminate."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = Obss.Provisioning.Domain.Entities.ProvisioningJob.Create(
            jobId,
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "FTTH",
            ProvisioningAction.Decommission);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        item.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new FtthLifecycleResult(
            correlationId,
            "TerminationInProgress",
            $"FTTH service termination is in progress. Reason: {request.Reason}",
            $"إنهاء خدمة الألياف الضوئية قيد التنفيذ. السبب: {request.Reason}",
            jobId));
    }
}
```

- [ ] **Step 3: Write TerminateFtthCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class TerminateFtthCommandValidator : AbstractValidator<TerminateFtthCommand>
{
    public TerminateFtthCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/Orders/Obss.Orders.Application/Obss.Orders.Application.csproj`
Expected: Build succeeds

### Task 12: Create FtthQualificationServiceTests

**Files:**
- Create: `tests/Modules/ServiceQualification.Tests/FtthQualificationServiceTests.cs`
- Create: `tests/Modules/ServiceQualification.Tests/FtthQualificationServiceTests.csproj`

- [ ] **Step 1: Create test directory and csproj

```bash
mkdir -p tests/Modules/ServiceQualification.Tests
```

- [ ] **Step 2: Write FtthQualificationServiceTests.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\src\Modules\ServiceQualification\Obss.ServiceQualification.Domain\Obss.ServiceQualification.Domain.csproj" />
    <ProjectReference Include="..\..\..\src\Modules\ServiceQualification\Obss.ServiceQualification.Infrastructure\Obss.ServiceQualification.Infrastructure.csproj" />
    <ProjectReference Include="..\..\..\src\Modules\NetworkInventory\Obss.NetworkInventory.Domain\Obss.NetworkInventory.Domain.csproj" />
    <ProjectReference Include="..\..\..\src\Modules\NetworkInventory\Obss.NetworkInventory.Application\Obss.NetworkInventory.Application.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Write FtthQualificationServiceTests.cs**

```csharp
using FluentAssertions;
using NSubstitute;
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.Services;
using Obss.ServiceQualification.Domain.ValueObjects;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.ServiceQualification.Tests;

public class FtthQualificationServiceTests
{
    private readonly ICoverageAreaRepository _coverageRepo;
    private readonly INetworkElementRepository _networkElementRepo;
    private readonly IFtthQualificationService _service;

    public FtthQualificationServiceTests()
    {
        _coverageRepo = Substitute.For<ICoverageAreaRepository>();
        _networkElementRepo = Substitute.For<INetworkElementRepository>();
        _service = new FtthQualificationService(_coverageRepo, _networkElementRepo);
    }

    [Fact]
    public async Task QualifyAsync_WhenNoCoverage_ReturnsUnqualifiedWithAlternatives()
    {
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "123 Main St", "Riyadh", null, null, null, 100, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Explanation.Should().Contain("No fiber coverage");
        result.ExplanationAr.Should().NotBeNullOrWhiteSpace();
        result.Alternatives.Should().NotBeNull();
        result.AlternativesAr.Should().NotBeNull();
    }

    [Fact]
    public async Task QualifyAsync_WhenFtthNotInCoverage_ReturnsUnqualifiedWithAvailableTechs()
    {
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("ADSL 50Mbps", 50, "ADSL", 150m, true));
        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "123 Main St", "Riyadh", null, null, null, 100, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.Alternatives.Should().Contain(a => a.Contains("ADSL"));
    }

    [Fact]
    public async Task QualifyAsync_WhenFtthAvailableAndOltCapacityOk_ReturnsQualified()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("FTTH 200Mbps", 200, "FTTH", 300m, true));

        var olt = OLT.Create(tenantId, "OLT-RUH-01", "olt-ruh-01.example.com", "10.0.1.1",
            "Huawei", "MA5800", null, "Riyadh DC", 16, 64, 10);
        olt.AddPONPort(1, PONPortType.GPON, 64, 20);

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("OLT", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([olt]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "123 Main St", "Riyadh", null, 24.7136, 46.6753, 200, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeTrue();
        result.CoverageDetail.Should().NotBeNull();
        result.CoverageDetail!.FiberAtPremises.Should().BeTrue();
        result.CoverageDetail.OltCapacityAvailable.Should().BeTrue();
        result.CoverageDetail.PonPortAvailable.Should().BeTrue();
        result.CoverageDetail.NearestOltName.Should().Be("OLT-RUH-01");
        result.RequiredWork.Should().NotBeNull();
        result.RequiredWork!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task QualifyAsync_WhenOltCapacityExhausted_ReturnsUnqualifiedWithDetail()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var coverageArea = new CoverageArea(Guid.NewGuid(), "Riyadh", null, null, null, "12345");
        coverageArea.AddService(CoverageService.Create("FTTH 100Mbps", 100, "FTTH", 250m, true));

        var olt = OLT.Create(tenantId, "OLT-RUH-02", "olt-ruh-02.example.com", "10.0.1.2",
            "Nokia", "FX-16", null, "Riyadh East", 16, 64, 10);
        for (int i = 1; i <= 16; i++)
        {
            var port = olt.AddPONPort(i, PONPortType.GPON, 64, 20);
            if (i <= 16)
            {
                for (int j = 0; j < 64; j++)
                    port.ConnectONT();
            }
        }

        _coverageRepo.GetByAddressAsync(Arg.Any<GeographicAddress>(), Arg.Any<CancellationToken>())
            .Returns([coverageArea]);
        _networkElementRepo.GetFilteredAsync("OLT", "Active", null, 0, 100, Arg.Any<CancellationToken>())
            .Returns([olt]);

        var result = await _service.QualifyAsync(new FtthQualificationRequest(
            "456 Oak Ave", "Riyadh", null, 24.7000, 46.7000, 100, "residential"), CancellationToken.None);

        result.IsQualified.Should().BeFalse();
        result.CoverageDetail.Should().NotBeNull();
        result.CoverageDetail!.OltCapacityAvailable.Should().BeFalse();
        result.CapacityConflicts.Should().NotBeNull();
        result.CapacityConflicts!.Count.Should().BeGreaterThan(0);
    }
}
```

- [ ] **Step 4: Write FtthOrderDecompositionTests.cs**

```csharp
using FluentAssertions;
using Obss.Orders.Application.Services;

namespace Obss.ServiceQualification.Tests;

public class FtthOrderDecompositionTests
{
    private readonly IFtthOrderDecompositionService _service;

    public FtthOrderDecompositionTests()
    {
        _service = new FtthOrderDecompositionService();
    }

    [Fact]
    public async Task DecomposeAsync_ForResidential_ReturnsCorrectTaskSequence()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            200,
            100,
            "SN-ONT-001",
            "LOID-001",
            "user@example.com",
            "123 Main St");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.CorrelationId.Should().NotBeEmpty();

        var resourceTasks = result.ResourceTasks;
        resourceTasks.Should().NotBeEmpty();
        resourceTasks[0].TaskType.Should().Be("ALLOCATE_PON_PORT");

        var serviceTasks = result.ServiceTasks;
        serviceTasks.Should().NotBeEmpty();
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_ONT_PROVISION");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_SERVICE_PORT_CONFIG");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_VLAN_CONFIG");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_PPPOE_CONFIG");
        serviceTasks.Should().Contain(t => t.TaskType == "PHYSICAL_INSTALL");
        serviceTasks.Should().Contain(t => t.TaskType == "FTTH_ACTIVATION_TEST");
    }

    [Fact]
    public async Task DecomposeAsync_ForResidential_ExcludesBusinessTasks()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            200,
            100,
            "SN-ONT-001",
            "LOID-001",
            "user@example.com",
            "123 Main St");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.Should().NotContain(t => t.TaskType == "FTTH_SLA_CONFIG");
        result.ServiceTasks.Should().NotContain(t => t.TaskType == "FTTH_BACKUP_CONFIG");
        result.ResourceTasks.Should().NotContain(t => t.TaskType == "ALLOCATE_STATIC_IP");
    }

    [Fact]
    public async Task DecomposeAsync_ForBusiness_IncludesSlaAndBackupTasks()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "business",
            500,
            250,
            "SN-BIZ-002",
            "LOID-BIZ-001",
            "biz@company.com",
            "456 Corp Blvd");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.Should().Contain(t => t.TaskType == "FTTH_SLA_CONFIG");
        result.ServiceTasks.Should().Contain(t => t.TaskType == "FTTH_BACKUP_CONFIG");
        result.ResourceTasks.Should().Contain(t => t.TaskType == "ALLOCATE_STATIC_IP");
    }

    [Fact]
    public async Task DecomposeAsync_TaskOrdering_IsCorrect()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            100,
            50,
            "SN-003", null, null, "789 Pine St");

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        var allTasks = result.ResourceTasks.Concat<object>(result.ServiceTasks).ToList();
        allTasks.Should().NotBeEmpty();

        var ordered = allTasks.Select((t, i) =>
        {
            int step = t is ResourceTask rt ? rt.StepOrder : ((ServiceTask)t).StepOrder;
            return (Step: step, Index: i);
        }).ToList();

        for (int i = 1; i < ordered.Count; i++)
            ordered[i].Step.Should().BeGreaterOrEqualTo(ordered[i - 1].Step);
    }

    [Fact]
    public async Task DecomposeAsync_TaskNames_IncludeArabic()
    {
        var request = new FtthDecompositionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "residential",
            100, 50, null, null, null, null);

        var result = await _service.DecomposeAsync(request, CancellationToken.None);

        result.ServiceTasks.All(t => !string.IsNullOrWhiteSpace(t.TaskNameAr)).Should().BeTrue();
        result.ServiceTasks.All(t => !string.IsNullOrWhiteSpace(t.TaskName)).Should().BeTrue();
    }
}
```

- [ ] **Step 5: Verify build and tests**

Run: `dotnet build tests/Modules/ServiceQualification.Tests/FtthQualificationServiceTests.csproj`
Run: `dotnet test tests/Modules/ServiceQualification.Tests/FtthQualificationServiceTests.csproj --no-build`
Expected: Build succeeds, tests pass

### Task 13: Verify full solution build

- [ ] **Step 1: Build entire solution**

Run: `dotnet build Obss.sln --configuration Release 2>&1 | tail -30`
Expected: Build succeeds with no errors (TreatWarningsAsErrors enabled)

---

## Self-Review Checklist

**Spec coverage:**
- [x] FtthServiceSpec value object (Task 1)
- [x] IFtthQualificationService interface + records (Task 2)
- [x] FtthQualificationService implementation (Task 3)
- [x] ProvisioningTaskType enum extension (Task 4)
- [x] QualificationItem entity extension (Task 5)
- [x] IFtthOrderDecompositionService interface (Task 6)
- [x] FtthOrderDecompositionService implementation (Task 7)
- [x] SuspendFtthCommand + handler + validator (Task 8)
- [x] ResumeFtthCommand + handler + validator (Task 9)
- [x] ChangeFtthSpeedCommand + handler + validator (Task 10)
- [x] TerminateFtthCommand + handler + validator (Task 11)
- [x] FtthQualificationServiceTests (Task 12)
- [x] FtthOrderDecompositionTests (Task 12)

**Placeholder scan:** No TBDs, TODOs, or placeholders in any step.

**Type consistency:** All types match across tasks (FtthQualificationRequest, FtthQualificationResult, etc.)
