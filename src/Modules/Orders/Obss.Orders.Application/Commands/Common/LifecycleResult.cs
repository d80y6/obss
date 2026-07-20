namespace Obss.Orders.Application.Commands.Common;

public sealed record LifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId,
    IReadOnlyList<ComponentResult>? ComponentResults);

public sealed record ComponentResult(
    string ComponentName,
    string ComponentNameAr,
    string Status,
    string Message,
    string MessageAr,
    Guid ProvisioningJobId);
