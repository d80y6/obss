using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class NetworkElementIpAddress : Entity<Guid>
{
    public NetworkElementIpAddress(
        Guid id,
        Guid networkElementId,
        Guid? networkInterfaceId,
        string ipAddress,
        string subnetMask,
        string? gateway,
        AddressType addressType,
        string? assignedTo)
        : base(id)
    {
        NetworkElementId = networkElementId;
        NetworkInterfaceId = networkInterfaceId;
        IPAddress = ipAddress;
        SubnetMask = subnetMask;
        Gateway = gateway;
        AddressType = addressType;
        IsAvailable = true;
        IsReserved = false;
        AssignedTo = assignedTo;
    }

    private NetworkElementIpAddress() { }

    public Guid NetworkElementId { get; private set; }
    public Guid? NetworkInterfaceId { get; private set; }
    public string IPAddress { get; private set; } = string.Empty;
    public string SubnetMask { get; private set; } = string.Empty;
    public string? Gateway { get; private set; }
    public AddressType AddressType { get; private set; }
    public bool IsAvailable { get; private set; }
    public bool IsReserved { get; private set; }
    public string? AssignedTo { get; private set; }

    public void Assign(string serviceIdentifier)
    {
        IsAvailable = false;
        IsReserved = true;
        AssignedTo = serviceIdentifier;
    }

    public void Release()
    {
        IsAvailable = true;
        IsReserved = false;
        AssignedTo = null;
    }

    public void Reserve()
    {
        IsReserved = true;
        IsAvailable = false;
    }
}
