using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class TopologyMap : Entity<Guid>
{
    private TopologyMap() { }

    private TopologyMap(
        Guid id,
        string name,
        string? description,
        string? configuration)
        : base(id)
    {
        Name = name;
        Description = description;
        Configuration = configuration;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Configuration { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static TopologyMap Create(
        string name,
        string? description,
        string? configuration)
    {
        return new TopologyMap(Guid.NewGuid(), name, description, configuration);
    }

    public void UpdateConfiguration(string configuration)
    {
        Configuration = configuration;
    }
}
