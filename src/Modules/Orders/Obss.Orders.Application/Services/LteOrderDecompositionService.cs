using System.Text.Json;

namespace Obss.Orders.Application.Services;

public sealed class LteOrderDecompositionService : ILteOrderDecompositionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<LteDecompositionResult> DecomposeAsync(LteDecompositionRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();
        var serviceTasks = new List<ServiceTask>();
        var resourceTasks = new List<ResourceTask>();

        int stepOrder = 1;

        resourceTasks.Add(new ResourceTask(
            "LTE_SIM_ACTIVATE",
            "SIM_ICCID",
            stepOrder++,
            null,
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                iccid = request.Iccid,
                activationType = "OTA",
                networkType = "4G_LTE"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "LTE_APN_CONFIG",
            "APN configuration",
            "تكوين APN",
            stepOrder++,
            "LTE_SIM_ACTIVATE",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                apn = request.Apn ?? "internet",
                qosClassId = 8,
                allocationRetentionPriority = 2
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "LTE_QOS_POLICY",
            "QoS/policy profile configuration",
            "تكوين ملف QoS / سياسة",
            stepOrder++,
            "LTE_APN_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                dataLimitGb = request.DataLimitGb,
                policyRule = "THROTTLE_AFTER_LIMIT",
                throttledSpeedMbps = 1,
                guaranteedBitrateKbps = 512
            }, JsonOptions))));

        resourceTasks.Add(new ResourceTask(
            "LTE_IP_ALLOCATE",
            "IP_ADDRESS",
            stepOrder++,
            "LTE_QOS_POLICY",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                addressType = "DYNAMIC",
                poolName = "LTE_POOL_1",
                allocationMethod = "RADIUS"
            }, JsonOptions))));

        if (request.Segment == "business")
        {
            resourceTasks.Add(new ResourceTask(
                "LTE_STATIC_IP_ALLOCATE",
                "STATIC_IP",
                stepOrder++,
                "LTE_IP_ALLOCATE",
                JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    ipCount = 1,
                    ipVersion = "IPv4",
                    poolName = "LTE_STATIC_POOL"
                }, JsonOptions))));
        }

        serviceTasks.Add(new ServiceTask(
            "LTE_ACTIVATION_TEST",
            "Activation test",
            "اختبار التفعيل",
            stepOrder,
            request.Segment == "business" ? "LTE_STATIC_IP_ALLOCATE" : "LTE_IP_ALLOCATE",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                testTypes = new[] { "SIM_REGISTRATION", "DATA_SESSION", "THROUGHPUT", "SMS" },
                acceptableSignalStrength = -85,
                minimumThroughputMbps = request.DataLimitGb > 50 ? 50 : 10
            }, JsonOptions))));

        return Task.FromResult(new LteDecompositionResult(
            correlationId,
            serviceTasks,
            resourceTasks));
    }
}
