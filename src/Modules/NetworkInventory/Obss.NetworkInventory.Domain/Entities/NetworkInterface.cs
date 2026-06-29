using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class NetworkInterface : Entity<Guid>
{
    public NetworkInterface(
        Guid id,
        Guid networkElementId,
        string name,
        string? description,
        InterfaceType interfaceType,
        int speed,
        string? macAddress,
        int mtu,
        Guid? connectedToInterfaceId)
        : base(id)
    {
        NetworkElementId = networkElementId;
        Name = name;
        Description = description;
        InterfaceType = interfaceType;
        Speed = speed;
        Status = InterfaceStatus.Up;
        MacAddress = macAddress;
        MTU = mtu;
        ConnectedToInterfaceId = connectedToInterfaceId;
        CreatedAt = DateTime.UtcNow;
    }

    private NetworkInterface() { }

    public Guid NetworkElementId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public InterfaceType InterfaceType { get; private set; }
    public int Speed { get; private set; }
    public InterfaceStatus Status { get; private set; }
    public string? MacAddress { get; private set; }
    public int MTU { get; private set; }
    public Guid? ConnectedToInterfaceId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void SetUp()
    {
        Status = InterfaceStatus.Up;
    }

    public void SetDown()
    {
        Status = InterfaceStatus.Down;
    }

    public void SetAdminDown()
    {
        Status = InterfaceStatus.AdminDown;
    }

    public void ConnectTo(Guid targetInterfaceId)
    {
        ConnectedToInterfaceId = targetInterfaceId;
    }

    public void Disconnect()
    {
        ConnectedToInterfaceId = null;
    }
}
