using System.Text.Json;

namespace Obss.Orders.Application.Services;

public sealed class BusinessConnectivityDecompositionService : IBusinessConnectivityDecompositionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<BusinessConnectivityDecompositionResult> DecomposeAsync(BusinessConnectivityDecompositionRequest request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();
        var serviceTasks = new List<ServiceTask>();
        var resourceTasks = new List<ResourceTask>();

        int stepOrder = 1;

        resourceTasks.Add(new ResourceTask(
            "BIZ_PORT_ALLOCATE",
            "PORT",
            stepOrder++,
            null,
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                connectivityType = request.ConnectivityType,
                bandwidthMbps = request.BandwidthMbps,
                interfaceType = request.ConnectivityType switch
                {
                    "DIA" => "GigabitEthernet",
                    "Ethernet" => "GigabitEthernet",
                    "TDM" => "E1",
                    _ => "GigabitEthernet"
                },
                portCount = 1
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "BIZ_VLAN_CONFIG",
            "VLAN configuration",
            "تكوين VLAN",
            stepOrder++,
            "BIZ_PORT_ALLOCATE",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                vlanId = request.VlanId,
                vlanType = "Q-in-Q",
                qosProfile = "QOS_BUSINESS",
                mtu = 1500
            }, JsonOptions))));

        resourceTasks.Add(new ResourceTask(
            "BIZ_IP_ALLOCATE",
            "IP_ADDRESS",
            stepOrder++,
            "BIZ_VLAN_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                addressType = "STATIC",
                subnetMask = "255.255.255.248",
                gateway = (string?)null,
                peerIp = request.PeerIp
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "BIZ_ROUTING_CONFIG",
            "Routing configuration",
            "تكوين التوجيه",
            stepOrder++,
            "BIZ_IP_ALLOCATE",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                routingProtocol = request.ConnectivityType == "DIA" ? "BGP" : "STATIC",
                bgpAsn = request.ConnectivityType == "DIA" ? 64512 : (int?)null,
                staticRoutes = Array.Empty<string>(),
                adminDistance = 110
            }, JsonOptions))));

        serviceTasks.Add(new ServiceTask(
            "BIZ_QOS_CONFIG",
            "QoS configuration",
            "تكوين QoS",
            stepOrder++,
            "BIZ_ROUTING_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                bandwidthMbps = request.BandwidthMbps,
                committedRateMbps = request.BandwidthMbps,
                burstRateMbps = request.BandwidthMbps * 1.1,
                queuingDiscipline = "CBWFQ"
            }, JsonOptions))));

        if (request.RequiresSla)
        {
            serviceTasks.Add(new ServiceTask(
                "BIZ_SLA_MONITORING",
                "SLA monitoring setup",
                "إعداد مراقبة SLA",
                stepOrder++,
                "BIZ_QOS_CONFIG",
                JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    slaLevel = "premium",
                    monitoringIntervalSeconds = 30,
                    latencyThresholdMs = 10,
                    jitterThresholdMs = 5,
                    packetLossThreshold = 0.001,
                    alertChannels = new[] { "EMAIL", "SNMP_TRAP" }
                }, JsonOptions))));
        }

        serviceTasks.Add(new ServiceTask(
            "BIZ_ACTIVATION_TEST",
            "Activation test",
            "اختبار التفعيل",
            stepOrder,
            request.RequiresSla ? "BIZ_SLA_MONITORING" : "BIZ_QOS_CONFIG",
            JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                testTypes = new[] { "LINK_UP", "BGP_PEERING", "PING", "TRACEROUTE", "THROUGHPUT" },
                acceptableLatencyMs = 10,
                acceptablePacketLoss = 0.001
            }, JsonOptions))));

        return Task.FromResult(new BusinessConnectivityDecompositionResult(
            correlationId,
            serviceTasks,
            resourceTasks));
    }
}
