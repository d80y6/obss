namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public sealed record ZteOperationProfile
{
    public string ProfileVersion { get; init; } = ZteAdapterConstants.DefaultProfileVersion;
    public string VendorProduct { get; init; } = ZteAdapterConstants.DefaultVendorProduct;
    public IReadOnlySet<string> ConfirmedOperations { get; init; } = new HashSet<string>();
    public IReadOnlySet<string> BlockedOperations { get; init; } = new HashSet<string>();

    public bool IsOperationConfirmed(string operationName) => ConfirmedOperations.Contains(operationName);

    public bool IsOperationBlocked(string operationName) => BlockedOperations.Contains(operationName);

    public static ZteOperationProfile Default { get; } = new()
    {
        ProfileVersion = "1.0.0",
        VendorProduct = ZteAdapterConstants.DefaultVendorProduct,
        ConfirmedOperations = new HashSet<string>
        {
            ZteAdapterConstants.OperationActivateNumber,
            ZteAdapterConstants.OperationDeactivateNumber,
            ZteAdapterConstants.OperationCreateSubscriber,
            ZteAdapterConstants.OperationUpdateSubscriberProfile,
            ZteAdapterConstants.OperationSuspendSubscriber,
            ZteAdapterConstants.OperationResumeSubscriber,
            ZteAdapterConstants.OperationTestConnection,
        },
        BlockedOperations = new HashSet<string>
        {
            ZteAdapterConstants.OperationPortNumber,
            ZteAdapterConstants.OperationReserveNumber,
            ZteAdapterConstants.OperationTerminateSubscriber,
            ZteAdapterConstants.OperationSetCallForwarding,
            ZteAdapterConstants.OperationSetCallBarring,
            ZteAdapterConstants.OperationSetCallWaiting,
            ZteAdapterConstants.OperationSetCallerId,
            ZteAdapterConstants.OperationSetVoicemail,
            ZteAdapterConstants.OperationConfigureSupplementaryService,
            ZteAdapterConstants.OperationConfigurePriTrunk,
            ZteAdapterConstants.OperationConfigureTdmCircuit,
            ZteAdapterConstants.OperationConfigureFreePhoneNumber,
            ZteAdapterConstants.OperationUpdateFreePhoneRouting,
            ZteAdapterConstants.OperationIngestCallDataRecords,
            ZteAdapterConstants.OperationAcknowledgeCdrBatch,
            ZteAdapterConstants.OperationGetSubscriberStatus,
            ZteAdapterConstants.OperationGetAlarms,
            ZteAdapterConstants.OperationGetTrunkStatus,
            ZteAdapterConstants.OperationReconcileSubscribers,
        },
    };
}
