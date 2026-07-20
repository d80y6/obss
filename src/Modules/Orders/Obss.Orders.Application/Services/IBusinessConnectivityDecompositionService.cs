using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface IBusinessConnectivityDecompositionService
{
    Task<BusinessConnectivityDecompositionResult> DecomposeAsync(BusinessConnectivityDecompositionRequest request, CancellationToken ct);
}

public sealed record BusinessConnectivityDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string Segment,
    string ConnectivityType,
    int BandwidthMbps,
    int VlanId,
    string? PeerIp,
    bool RequiresSla);

public sealed record BusinessConnectivityDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<ServiceTask> ServiceTasks,
    IReadOnlyList<ResourceTask> ResourceTasks);
