namespace Obss.Provisioning.Infrastructure.Adapters.Juniper;

public static class JuniperAdapterConstants
{
    public const string AdapterName = "Juniper";

    // YANG paths
    public const string InterfacePath = "/data/JunOS:configuration/interfaces/interface";
    public const string InterfaceStatusPath = "/data/JunOS:interface-information";
    public const string BgpPath = "/data/JunOS:configuration/protocols/bgp";
    public const string OspfPath = "/data/JunOS:configuration/protocols/ospf";
    public const string StaticRoutePath = "/data/JunOS:configuration/routing-options/static/route";
    public const string SystemPath = "/data/JunOS:configuration/system";
    public const string FirewallFilterPath = "/data/JunOS:configuration/firewall/filter";
    public const string DeviceStatusPath = "/data/JunOS:system-information";
    public const string InventoryPath = "/data/JunOS:chassis-inventory";
    public const string AlarmsPath = "/data/JunOS:alarm-information";
}
