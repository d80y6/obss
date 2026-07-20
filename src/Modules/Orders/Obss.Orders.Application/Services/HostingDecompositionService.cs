using System.Text.Json;

namespace Obss.Orders.Application.Services;

public sealed class HostingDecompositionService : IHostingDecompositionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<HostingDecompositionResult> DecomposeAsync(HostingDecompositionRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();
        var serviceTasks = new List<ServiceTask>();
        var resourceTasks = new List<ResourceTask>();

        int stepOrder = 1;

        resourceTasks.Add(new ResourceTask(
            "HST_RACK_POWER_ALLOCATE",
            "RACK_POWER",
            stepOrder++,
            null,
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                hostingType = request.HostingType,
                powerWattage = request.HostingType switch
                {
                    "DedicatedServer" => 500,
                    "VPS" => 0,
                    "Colocation" => 1000,
                    _ => 300
                },
                rackUnits = request.HostingType == "Colocation" ? 2 : 1,
                coolingType = "STANDARD"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "HST_SERVER_PROVISION",
            "Server provisioning",
            "توفير الخادم",
            stepOrder++,
            "HST_RACK_POWER_ALLOCATE",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                hostingType = request.HostingType,
                cpuCores = request.CpuCores,
                ramGb = request.RamGb,
                storageGb = request.StorageGb,
                storageType = "SSD",
                virtualizationType = request.HostingType == "VPS" ? "KVM" : null
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "HST_OS_INSTALLATION",
            "OS installation",
            "تثبيت نظام التشغيل",
            stepOrder++,
            "HST_SERVER_PROVISION",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                osImage = request.OsImage ?? "ubuntu-24.04-lts",
                diskEncryption = request.Segment == "business",
                swapSizeGb = 4,
                timezone = "UTC"
            }, JsonOptions))));

        resourceTasks.Add(new ResourceTask(
            "HST_NETWORK_CONFIG",
            "NETWORK",
            stepOrder++,
            "HST_OS_INSTALLATION",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                publicIpCount = 1,
                privateIpCount = 0,
                firewallProfile = "STANDARD",
                allowedPorts = new[] { 22, 80, 443 }
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "HST_DNS_CONFIG",
            "DNS configuration",
            "تكوين DNS",
            stepOrder++,
            "HST_NETWORK_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                dnsRecords = Array.Empty<object>(),
                reverseDnsEnabled = true,
                dnsProvider = "INTERNAL"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "HST_MONITORING_SETUP",
            "Monitoring setup",
            "إعداد المراقبة",
            stepOrder++,
            "HST_DNS_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                monitoringType = "INFRASTRUCTURE",
                metricsCollectionIntervalSeconds = 30,
                alertThresholdCpu = 90,
                alertThresholdRam = 90,
                alertThresholdDisk = 85,
                backupSchedule = "DAILY"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "HST_ACTIVATION_TEST",
            "Activation test",
            "اختبار التفعيل",
            stepOrder,
            "HST_MONITORING_SETUP",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                testTypes = new[] { "SSH_ACCESS", "PING", "HTTP_SERVICE", "DNS_RESOLUTION" },
                acceptableLatencyMs = 50
            }, JsonOptions))));

        return Task.FromResult(new HostingDecompositionResult(
            correlationId,
            serviceTasks,
            resourceTasks));
    }
}
