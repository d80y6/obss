using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface ILteOrderDecompositionService
{
    Task<LteDecompositionResult> DecomposeAsync(LteDecompositionRequest request, CancellationToken ct);
}

public sealed record LteDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string Segment,
    string? Iccid,
    string? Apn,
    int DataLimitGb);

public sealed record LteDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<ServiceTask> ServiceTasks,
    IReadOnlyList<ResourceTask> ResourceTasks);
