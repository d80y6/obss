using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface ITelephonyOrderDecompositionService
{
    Task<TelephonyDecompositionResult> DecomposeAsync(TelephonyDecompositionRequest request, CancellationToken ct);
}

public sealed record TelephonyDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string Segment,
    string? PhoneNumber,
    bool CallForwarding,
    bool CallWaiting,
    bool CallerId,
    bool ThreeWayCalling);

public sealed record TelephonyDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<ServiceTask> ServiceTasks,
    IReadOnlyList<ResourceTask> ResourceTasks);
