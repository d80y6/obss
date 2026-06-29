using Obss.SharedKernel.Domain.Common;

namespace Obss.ModuleTemplate.Domain.Entities;

public class SampleEntity : Entity<Guid>
{
    private SampleEntity() { }

    public SampleEntity(Guid id, Guid aggregateId, string code, string name)
        : base(id)
    {
        AggregateId = aggregateId;
        Code = code;
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid AggregateId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public void UpdateName(string name)
    {
        Name = name;
    }
}
