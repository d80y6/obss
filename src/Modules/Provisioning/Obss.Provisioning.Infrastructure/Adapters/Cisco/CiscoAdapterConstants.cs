namespace Obss.Provisioning.Infrastructure.Adapters.Cisco;

public static class CiscoAdapterConstants
{
    public const string AdapterName = "CISCO_ROUTER";
    public const string TechnologyType = "cisco_router";

    public const string InterfacePath = "/data/Cisco-IOS-XE-native:native/interface";
    public const string BgpPath = "/data/Cisco-IOS-XE-bgp:bgp";
    public const string OspfPath = "/data/Cisco-IOS-XE-ospf:ospf";
    public const string StaticRoutePath = "/data/Cisco-IOS-XE-native:native/ip/route";
    public const string SystemPath = "/data/Cisco-IOS-XE-native:native";
    public const string AclPath = "/data/Cisco-IOS-XE-acl:access-lists";
    public const string DeviceStatusPath = "/data/Cisco-IOS-XE-native:native";
    public const string InventoryPath = "/data/Cisco-IOS-XE-device-hardware:device-hardware";
    public const string AlarmsPath = "/data/Cisco-IOS-XE-alarms:alarms";
}
