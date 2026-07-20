using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface IOrderDecompositionOrchestrator
{
    Task<UnifiedDecompositionResult> DecomposeAsync(UnifiedDecompositionRequest request, CancellationToken ct);
}

public sealed record UnifiedDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string ServiceType,
    string Segment,
    JsonDocument? ServiceConfiguration);

public sealed record UnifiedDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<DecomposedTask> Tasks);

public sealed record DecomposedTask(
    string TaskType,
    string AdapterType,
    string TaskName,
    string TaskNameAr,
    int StepOrder,
    string? DependsOn,
    JsonDocument? Configuration);
