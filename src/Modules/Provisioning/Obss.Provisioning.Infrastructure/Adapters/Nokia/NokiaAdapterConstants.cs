namespace Obss.Provisioning.Infrastructure.Adapters.Nokia;

public static class NokiaAdapterConstants
{
    public const string AdapterName = "Nokia";

    // YANG paths (Nokia SR OS / TiMOS)
    public const string PortPath = "/data/nokia-conf:configure/port";
    public const string PortStatusPath = "/data/nokia-state:state/port";
    public const string BgpPath = "/data/nokia-conf:configure/router/0/bgp";
    public const string OspfPath = "/data/nokia-conf:configure/router/0/ospf";
    public const string StaticRoutePath = "/data/nokia-conf:configure/router/0/static-route";
    public const string SystemPath = "/data/nokia-conf:configure/system";
    public const string IpFilterPath = "/data/nokia-conf:configure/ip-filter";
    public const string DeviceStatusPath = "/data/nokia-state:state/system";
    public const string InventoryPath = "/data/nokia-state:state/chassis";
    public const string AlarmsPath = "/data/nokia-state:state/alarm";
}
