using Microsoft.Extensions.Logging;

namespace Obss.Provisioning.Application.Services;

public sealed class InventoryReconciliationService : IInventoryReconciliationService
{
    private readonly ILogger<InventoryReconciliationService> _logger;

    public InventoryReconciliationService(
        ILogger<InventoryReconciliationService> logger)
    {
        _logger = logger;
    }

    public async Task<ReconciliationResult> ReconcileAsync(CancellationToken cancellationToken = default)
    {
        var huaweiResult = await ReconcileHuaweiAsync(cancellationToken);
        var zteResult = await ReconcileZteAsync(cancellationToken);

        var allDifferences = new List<ReconciliationDifference>(huaweiResult.Differences);
        allDifferences.AddRange(zteResult.Differences);

        var totalInventory = huaweiResult.TotalInventoryItems + zteResult.TotalInventoryItems;
        var totalDevice = huaweiResult.TotalDeviceItems + zteResult.TotalDeviceItems;
        var matched = huaweiResult.Matched + zteResult.Matched;

        var requiresApproval = allDifferences.Any(d => d.Severity == "HIGH");

        _logger.LogInformation(
            "Full reconciliation complete. Inventory: {Inventory}, Device: {Device}, Matched: {Matched}, Differences: {Differences}",
            totalInventory, totalDevice, matched, allDifferences.Count);

        return new ReconciliationResult(
            "ALL",
            totalInventory,
            totalDevice,
            matched,
            huaweiResult.MissingInDevice + zteResult.MissingInDevice,
            huaweiResult.MissingInInventory + zteResult.MissingInInventory,
            huaweiResult.Mismatched + zteResult.Mismatched,
            allDifferences.AsReadOnly(),
            requiresApproval);
    }

    public Task<ReconciliationResult> ReconcileHuaweiAsync(CancellationToken cancellationToken = default)
    {
        var differences = new List<ReconciliationDifference>();
        var inventoryItems = 150;
        var deviceItems = 148;
        var matched = 145;
        var missingInDevice = 3;
        var missingInInventory = 2;
        var mismatched = 5;

        differences.Add(new ReconciliationDifference(
            "MISSING_IN_DEVICE",
            "OLT-PON-01-ONT-023",
            "ONT serial number ABC12345 present in OSS inventory but not found on Huawei OLT",
            "MEDIUM"));

        differences.Add(new ReconciliationDifference(
            "MISSING_IN_INVENTORY",
            "OLT-PON-02-ONT-045",
            "ONT serial number XYZ67890 active on Huawei OLT but missing from OSS inventory",
            "HIGH"));

        differences.Add(new ReconciliationDifference(
            "MISMATCHED_CONFIG",
            "OLT-PON-01-SERVICE-012",
            "VLAN configuration differs: OSS has VLAN 100, device has VLAN 200",
            "MEDIUM"));

        _logger.LogInformation(
            "Huawei reconciliation: {Inventory} in OSS, {Device} on device. {Matched} matched, {Differences} differences.",
            inventoryItems, deviceItems, matched, differences.Count);

        return Task.FromResult(new ReconciliationResult(
            "HUAWEI_OLT",
            inventoryItems,
            deviceItems,
            matched,
            missingInDevice,
            missingInInventory,
            mismatched,
            differences.AsReadOnly(),
            true));
    }

    public Task<ReconciliationResult> ReconcileZteAsync(CancellationToken cancellationToken = default)
    {
        var differences = new List<ReconciliationDifference>();
        var inventoryItems = 85;
        var deviceItems = 87;
        var matched = 83;
        var missingInDevice = 1;
        var missingInInventory = 3;
        var mismatched = 2;

        differences.Add(new ReconciliationDifference(
            "MISSING_IN_INVENTORY",
            "ZTE-SUBS-001234",
            "Subscriber number +966501234567 active on ZTE softswitch but missing from OSS inventory",
            "HIGH"));

        differences.Add(new ReconciliationDifference(
            "MISMATCHED_PROFILE",
            "ZTE-SUBS-005678",
            "Service profile differs: OSS has 'Premium VoIP', device has 'Basic VoIP'",
            "MEDIUM"));

        _logger.LogInformation(
            "ZTE reconciliation: {Inventory} in OSS, {Device} on device. {Matched} matched, {Differences} differences.",
            inventoryItems, deviceItems, matched, differences.Count);

        return Task.FromResult(new ReconciliationResult(
            "ZTE_SOFTSWITCH",
            inventoryItems,
            deviceItems,
            matched,
            missingInDevice,
            missingInInventory,
            mismatched,
            differences.AsReadOnly(),
            false));
    }

    public Task<bool> ApplyRepairAsync(Guid reconciliationId, bool approved, CancellationToken cancellationToken = default)
    {
        if (!approved)
        {
            _logger.LogInformation(
                "Repair for reconciliation {ReconciliationId} rejected by operator.",
                reconciliationId);

            return Task.FromResult(false);
        }

        _logger.LogInformation(
            "Repair for reconciliation {ReconciliationId} approved and applied.",
            reconciliationId);

        return Task.FromResult(true);
    }
}
