using System.Text.Json;

namespace Obss.Orders.Application.Services;

public sealed class AdslOrderDecompositionService : IAdslOrderDecompositionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<AdslDecompositionResult> DecomposeAsync(AdslDecompositionRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();
        var serviceTasks = new List<ServiceTask>();
        var resourceTasks = new List<ResourceTask>();

        int stepOrder = 1;

        resourceTasks.Add(new ResourceTask(
            "ADSL_DSLAM_PORT_ALLOCATE",
            "DSLAM_PORT",
            stepOrder++,
            null,
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                dslamId = request.DslamId,
                portTechnology = "ADSL2+",
                vpi = request.Vpi,
                vci = request.Vci
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "ADSL_LINE_PROFILE_CONFIG",
            "Configure DSL line profile",
            "تكوين ملف خط DSL",
            stepOrder++,
            "ADSL_DSLAM_PORT_ALLOCATE",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                lineProfile = request.LineProfile ?? "ADSL_PROFILE_STANDARD",
                targetSnrMargin = 6.0,
                maxReachableRateKbps = 24000
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "ADSL_ACCESS_CREDENTIALS",
            "Set up access credentials (PPP username/password)",
            "إعداد بيانات اعتماد الوصول (PPP اسم المستخدم/كلمة المرور)",
            stepOrder++,
            "ADSL_LINE_PROFILE_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                username = request.PppUsername,
                authenticationProtocol = "CHAP",
                serviceName = "ADSL_INTERNET",
                ipAllocation = "DYNAMIC"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "ADSL_VLAN_CONFIG",
            "Configure VLAN",
            "تكوين VLAN",
            stepOrder++,
            "ADSL_ACCESS_CREDENTIALS",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                vlanId = 200,
                vlanType = "C-Tag",
                priorityBits = 5,
                qosProfile = "QOS_BEST_EFFORT"
            }, JsonOptions))));

        resourceTasks.Add(new ResourceTask(
            "ALLOCATE_IP_ADSL",
            "IP_ADDRESS",
            stepOrder++,
            "ADSL_VLAN_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                addressType = "PPPoE",
                poolName = "ADSL_PPPOE_POOL"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "ADSL_CPE_CONFIG",
            "Configure CPE",
            "تكوين CPE",
            stepOrder++,
            "ALLOCATE_IP_ADSL",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                cpeManagementProtocol = "TR-069",
                ssid = "ADSL_" + request.PppUsername,
                encryption = "WPA2"
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "ADSL_ACTIVATION_TEST",
            "Activation test",
            "اختبار التفعيل",
            stepOrder,
            "ADSL_CPE_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                testTypes = new[] { "DSL_SYNC", "PPP_AUTH", "INTERNET_PING", "THROUGHPUT" },
                acceptablePacketLoss = 0.02,
                minimumSyncKbps = 2048
            }, JsonOptions))));

        return Task.FromResult(new AdslDecompositionResult(
            correlationId,
            serviceTasks,
            resourceTasks));
    }
}
