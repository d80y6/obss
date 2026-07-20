namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public static class ZteAdapterConstants
{
    public const string AdapterName = "ZTE Softswitch Adapter";
    public const string DefaultProtocol = "SIGTRAN";
    public const int DefaultPort = 9060;
    public const int DefaultTimeoutSeconds = 30;
    public const int DefaultMaxRetries = 3;
    public const string DefaultProfileVersion = "1.0.0";
    public const string YemenCountryCode = "+967";
    public const string DefaultVendorProduct = "ZTE Softswitch (unspecified product)";
    public const string CorrelationIdHeader = "X-ZTE-Correlation-Id";

    public const string OperationActivateNumber = "ActivateNumber";
    public const string OperationDeactivateNumber = "DeactivateNumber";
    public const string OperationPortNumber = "PortNumber";
    public const string OperationReserveNumber = "ReserveNumber";
    public const string OperationCreateSubscriber = "CreateSubscriber";
    public const string OperationUpdateSubscriberProfile = "UpdateSubscriberProfile";
    public const string OperationSuspendSubscriber = "SuspendSubscriber";
    public const string OperationResumeSubscriber = "ResumeSubscriber";
    public const string OperationTerminateSubscriber = "TerminateSubscriber";
    public const string OperationSetCallForwarding = "SetCallForwarding";
    public const string OperationSetCallBarring = "SetCallBarring";
    public const string OperationSetCallWaiting = "SetCallWaiting";
    public const string OperationSetCallerId = "SetCallerId";
    public const string OperationSetVoicemail = "SetVoicemail";
    public const string OperationConfigureSupplementaryService = "ConfigureSupplementaryService";
    public const string OperationConfigurePriTrunk = "ConfigurePriTrunk";
    public const string OperationConfigureTdmCircuit = "ConfigureTdmCircuit";
    public const string OperationConfigureFreePhoneNumber = "ConfigureFreePhoneNumber";
    public const string OperationUpdateFreePhoneRouting = "UpdateFreePhoneRouting";
    public const string OperationIngestCallDataRecords = "IngestCallDataRecords";
    public const string OperationAcknowledgeCdrBatch = "AcknowledgeCdrBatch";
    public const string OperationGetSubscriberStatus = "GetSubscriberStatus";
    public const string OperationGetAlarms = "GetAlarms";
    public const string OperationGetTrunkStatus = "GetTrunkStatus";
    public const string OperationReconcileSubscribers = "ReconcileSubscribers";
    public const string OperationTestConnection = "TestConnection";
}
