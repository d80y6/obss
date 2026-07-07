using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Domain.Entities;

public class OLT : NetworkElement
{
    private readonly List<PONPort> _ponPorts = [];

    private OLT() { }

    private OLT(
        Guid id,
        TenantId tenantId,
        string name,
        string hostname,
        string ipAddress,
        string vendor,
        string model,
        string? softwareVersion,
        string? location,
        int maxPONPorts,
        int maxONTPerPort,
        int maxBandwidth)
        : base(
            id, tenantId, name, hostname, ipAddress, ElementType.OLT,
            vendor, model, softwareVersion, null, location,
            null, null, false)
    {
        MaxPONPorts = maxPONPorts;
        UsedPONPorts = 0;
        MaxONTPerPort = maxONTPerPort;
        MaxBandwidth = maxBandwidth;
    }

    public int MaxPONPorts { get; private set; }
    public int UsedPONPorts { get; private set; }
    public int MaxONTPerPort { get; private set; }
    public int MaxBandwidth { get; private set; }

    public IReadOnlyCollection<PONPort> PONPorts => _ponPorts.AsReadOnly();

    public static OLT Create(
        TenantId tenantId,
        string name,
        string hostname,
        string ipAddress,
        string vendor,
        string model,
        string? softwareVersion = null,
        string? location = null,
        int maxPONPorts = 16,
        int maxONTPerPort = 64,
        int maxBandwidth = 10)
    {
        return new OLT(
            Guid.NewGuid(),
            tenantId,
            name,
            hostname,
            ipAddress,
            vendor,
            model,
            softwareVersion,
            location,
            maxPONPorts,
            maxONTPerPort,
            maxBandwidth);
    }

    public PONPort AddPONPort(int portNumber, PONPortType portType, int maxOnt, float maxDistance)
    {
        var port = new PONPort(
            Guid.NewGuid(),
            Id,
            portNumber,
            portType,
            maxOnt,
            maxDistance);

        _ponPorts.Add(port);
        return port;
    }

    public void RegisterONT(int portNumber)
    {
        var port = _ponPorts.FirstOrDefault(p => p.PortNumber == portNumber);
        if (port is null)
            throw new InvalidOperationException($"PON port {portNumber} not found.");

        port.ConnectONT();
        UpdatePortUtilization();
    }

    public void RemoveONT(int portNumber)
    {
        var port = _ponPorts.FirstOrDefault(p => p.PortNumber == portNumber);
        if (port is null)
            throw new InvalidOperationException($"PON port {portNumber} not found.");

        port.DisconnectONT();
        UpdatePortUtilization();
    }

    public void UpdatePortUtilization()
    {
        UsedPONPorts = _ponPorts.Count(p => p.Status == PONPortStatus.Used);
    }
}
