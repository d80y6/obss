using Obss.NetworkInventory.Domain.Events;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Domain.Entities;

public class NetworkElement : AggregateRoot<Guid>
{
    private readonly List<NetworkInterface> _interfaces = [];
    private readonly List<NetworkElementIpAddress> _ipAddresses = [];

    private NetworkElement() { }

    private NetworkElement(
        Guid id,
        TenantId tenantId,
        string name,
        string hostname,
        string ipAddress,
        ElementType elementType,
        string vendor,
        string model,
        string? softwareVersion,
        string? serialNumber,
        string? location,
        string? managementIp,
        string? snmpCommunity,
        bool isManaged)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Hostname = hostname;
        IPAddress = ipAddress;
        ElementType = elementType;
        Vendor = vendor;
        Model = model;
        SoftwareVersion = softwareVersion;
        SerialNumber = serialNumber;
        Location = location;
        Status = ElementStatus.Active;
        ManagementIP = managementIp;
        SNMPCommunity = snmpCommunity;
        IsManaged = isManaged;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new NetworkElementAddedDomainEvent(id, tenantId, name, elementType));
    }

    public TenantId TenantId { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Hostname { get; private set; } = string.Empty;
    public string IPAddress { get; private set; } = string.Empty;
    public ElementType ElementType { get; private set; }
    public string Vendor { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string? SoftwareVersion { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? Location { get; private set; }
    public ElementStatus Status { get; private set; }
    public string? ManagementIP { get; private set; }
    public string? SNMPCommunity { get; private set; }
    public bool IsManaged { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<NetworkInterface> Interfaces => _interfaces.AsReadOnly();
    public IReadOnlyCollection<NetworkElementIpAddress> IpAddresses => _ipAddresses.AsReadOnly();

    public static NetworkElement Create(
        TenantId tenantId,
        string name,
        string hostname,
        string ipAddress,
        ElementType elementType,
        string vendor,
        string model,
        string? softwareVersion = null,
        string? serialNumber = null,
        string? location = null,
        string? managementIp = null,
        string? snmpCommunity = null,
        bool isManaged = true)
    {
        return new NetworkElement(
            Guid.NewGuid(),
            tenantId,
            name,
            hostname,
            ipAddress,
            elementType,
            vendor,
            model,
            softwareVersion,
            serialNumber,
            location,
            managementIp,
            snmpCommunity,
            isManaged);
    }

    public void Activate()
    {
        if (Status == ElementStatus.Active)
            return;

        Status = ElementStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMaintenance()
    {
        if (Status == ElementStatus.Maintenance)
            return;

        Status = ElementStatus.Maintenance;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDown(string reason)
    {
        if (Status == ElementStatus.Down)
            return;

        Status = ElementStatus.Down;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NetworkElementDownDomainEvent(Id, TenantId, Name, Hostname, reason));
    }

    public void Decommission()
    {
        if (Status == ElementStatus.Decommissioned)
            return;

        Status = ElementStatus.Decommissioned;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFirmware(string version)
    {
        SoftwareVersion = version;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string hostname, string vendor, string model,
        string? softwareVersion, string? serialNumber, string? location,
        string? managementIp, string? snmpCommunity, bool isManaged)
    {
        Name = name;
        Hostname = hostname;
        Vendor = vendor;
        Model = model;
        SoftwareVersion = softwareVersion;
        SerialNumber = serialNumber;
        Location = location;
        ManagementIP = managementIp;
        SNMPCommunity = snmpCommunity;
        IsManaged = isManaged;
        UpdatedAt = DateTime.UtcNow;
    }

    public NetworkInterface AddInterface(string name, string? description, InterfaceType interfaceType,
        int speed, string? macAddress, int mtu, Guid? connectedToInterfaceId = null)
    {
        var iface = new NetworkInterface(
            Guid.NewGuid(),
            Id,
            name,
            description,
            interfaceType,
            speed,
            macAddress,
            mtu,
            connectedToInterfaceId);

        _interfaces.Add(iface);
        UpdatedAt = DateTime.UtcNow;
        return iface;
    }

    public NetworkElementIpAddress AddIpAddress(Guid? networkInterfaceId, string ipAddress,
        string subnetMask, string? gateway, AddressType addressType, string? assignedTo = null)
    {
        var addr = new NetworkElementIpAddress(
            Guid.NewGuid(),
            Id,
            networkInterfaceId,
            ipAddress,
            subnetMask,
            gateway,
            addressType,
            assignedTo);

        _ipAddresses.Add(addr);
        UpdatedAt = DateTime.UtcNow;
        return addr;
    }
}
