using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface IHostingDecompositionService
{
    Task<HostingDecompositionResult> DecomposeAsync(HostingDecompositionRequest request, CancellationToken ct);
}

public sealed record HostingDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string Segment,
    string HostingType,
    string? OsImage,
    int CpuCores,
    int RamGb,
    int StorageGb);

public sealed record HostingDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<ServiceTask> ServiceTasks,
    IReadOnlyList<ResourceTask> ResourceTasks);
