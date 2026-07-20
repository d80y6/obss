using System.Text.Json;

namespace Obss.Orders.Application.Services;

public interface IFtthOrderDecompositionService
{
    Task<FtthDecompositionResult> DecomposeAsync(FtthDecompositionRequest request, CancellationToken ct);
}

public sealed record FtthDecompositionRequest(
    Guid OrderId,
    Guid OrderItemId,
    string Segment,
    int DownloadSpeedMbps,
    int UploadSpeedMbps,
    string? OntSerial,
    string? Loid,
    string? PpoeUsername,
    string? InstallationAddress);

public sealed record FtthDecompositionResult(
    Guid CorrelationId,
    IReadOnlyList<ServiceTask> ServiceTasks,
    IReadOnlyList<ResourceTask> ResourceTasks);

public sealed record ServiceTask(
    string TaskType,
    string TaskName,
    string TaskNameAr,
    int StepOrder,
    string? DependsOnTaskType,
    JsonDocument? Configuration);

public sealed record ResourceTask(
    string TaskType,
    string ResourceType,
    int StepOrder,
    string? DependsOnTaskType,
    JsonDocument? Configuration);
