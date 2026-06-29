using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class PONPort : Entity<Guid>
{
    public PONPort(
        Guid id,
        Guid oltId,
        int portNumber,
        PONPortType portType,
        int maxOnt,
        float maxDistance)
        : base(id)
    {
        OLTId = oltId;
        PortNumber = portNumber;
        PortType = portType;
        Status = PONPortStatus.Free;
        MaxONT = maxOnt;
        ConnectedONTCount = 0;
        MaxDistance = maxDistance;
    }

    private PONPort() { }

    public Guid OLTId { get; private set; }
    public int PortNumber { get; private set; }
    public PONPortType PortType { get; private set; }
    public PONPortStatus Status { get; private set; }
    public int MaxONT { get; private set; }
    public int ConnectedONTCount { get; private set; }
    public float MaxDistance { get; private set; }

    public void ConnectONT()
    {
        if (ConnectedONTCount >= MaxONT)
            throw new InvalidOperationException("Maximum number of ONTs already connected to this port.");

        ConnectedONTCount++;
        Status = PONPortStatus.Used;
    }

    public void DisconnectONT()
    {
        if (ConnectedONTCount <= 0)
            return;

        ConnectedONTCount--;
        if (ConnectedONTCount == 0)
            Status = PONPortStatus.Free;
    }

    public void MarkFailed()
    {
        Status = PONPortStatus.Failed;
    }

    public void Repair()
    {
        Status = ConnectedONTCount > 0 ? PONPortStatus.Used : PONPortStatus.Free;
    }
}
