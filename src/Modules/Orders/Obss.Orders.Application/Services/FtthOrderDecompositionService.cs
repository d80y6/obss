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
                stepOrder,
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
