namespace Obss.Provisioning.Infrastructure.Adapters.Common;

public static class AdapterConstants
{
    public static class TechnologyTypes
    {
        public const string Ftth = "ftth";
        public const string Adsl = "adsl";
        public const string Lte = "lte";
        public const string WiFi = "wifi";
        public const string Telephony = "telephony";
    }

    public static class AdapterNames
    {
        public const string HuaweiBroadband = "HuaweiBroadband";
        public const string ZTEBroadband = "ZTEBroadband";
    }

    public static class Operations
    {
        public const string ActivateFtth = "ActivateFtth";
        public const string ActivateAdsl = "ActivateAdsl";
        public const string Activate4G = "Activate4G";
        public const string ActivateWiFi = "ActivateWiFi";
        public const string Suspend = "Suspend";
        public const string Resume = "Resume";
        public const string ChangeService = "ChangeService";
        public const string Terminate = "Terminate";
        public const string GetDeviceStatus = "GetDeviceStatus";
        public const string GetAlarms = "GetAlarms";
        public const string CollectPerformanceMetrics = "CollectPerformanceMetrics";
        public const string ReconcileInventory = "ReconcileInventory";
    }
}
