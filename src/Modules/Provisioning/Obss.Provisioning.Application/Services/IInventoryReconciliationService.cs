namespace Obss.Provisioning.Application.Services;

public sealed record ReconciliationDifference(
    string Type,
    string Identifier,
    string Description,
    string Severity);

public sealed record ReconciliationResult(
    string Source,
    int TotalInventoryItems,
    int TotalDeviceItems,
    int Matched,
    int MissingInDevice,
    int MissingInInventory,
    int Mismatched,
    IReadOnlyList<ReconciliationDifference> Differences,
    bool RequiresOperatorApproval);

public interface IInventoryReconciliationService
{
    Task<ReconciliationResult> ReconcileAsync(CancellationToken cancellationToken = default);
    Task<ReconciliationResult> ReconcileHuaweiAsync(CancellationToken cancellationToken = default);
    Task<ReconciliationResult> ReconcileZteAsync(CancellationToken cancellationToken = default);
    Task<bool> ApplyRepairAsync(Guid reconciliationId, bool approved, CancellationToken cancellationToken = default);
}
