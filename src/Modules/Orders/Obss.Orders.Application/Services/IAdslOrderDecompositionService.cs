using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface IAdslOrderDecompositionService
{
    Task<AdslDecompositionResult> DecomposeAsync(AdslDecompositionRequest request, CancellationToken ct);
}

public sealed record AdslDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string Segment,
    string? DslamId,
    int Vpi,
    int Vci,
    string? PppUsername,
    string? LineProfile);

public sealed record AdslDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<ServiceTask> ServiceTasks,
    IReadOnlyList<ResourceTask> ResourceTasks);
