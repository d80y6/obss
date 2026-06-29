namespace Obss.NetworkInventory.Domain.ValueObjects;

public enum ElementType
{
    Router,
    Switch,
    OLT,
    ONT,
    BRAS,
    Firewall,
    Server,
    AccessPoint,
    MediaConverter
}

public enum ElementStatus
{
    Active,
    Maintenance,
    Down,
    Decommissioned
}

public enum InterfaceType
{
    Ethernet,
    GPON,
    XGSPON,
    VDSL,
    ADSL,
    Radio,
    Fiber,
    Management
}

public enum InterfaceStatus
{
    Up,
    Down,
    AdminDown
}

public enum AddressType
{
    Primary,
    Secondary,
    Management,
    Loopback,
    Public
}

public enum SubnetStatus
{
    Active,
    Reserved,
    Exhausted,
    Decommissioned
}

public enum VLANStatus
{
    Active,
    Reserved,
    Decommissioned
}

public enum PONPortType
{
    GPON,
    XGSPON
}

public enum PONPortStatus
{
    Free,
    Used,
    Failed
}

public enum FiberType
{
    SingleMode,
    MultiMode
}

public enum FiberStatus
{
    Active,
    Damaged,
    Decommissioned
}

public enum LinkType
{
    Fiber,
    Copper,
    Radio,
    Microwave,
    Satellite
}

public enum LinkStatus
{
    Active,
    Degraded,
    Down,
    Maintenance
}

public enum CapacityType
{
    Bandwidth,
    Ports,
    IPAddresses,
    VLANS,
    Memory,
    CPU,
    Storage
}
