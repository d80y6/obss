using Obss.SharedKernel.Domain.Common;

namespace Obss.ModuleTemplate.Domain.Events;

public sealed class SampleCreatedDomainEvent : DomainEvent
{
    public SampleCreatedDomainEvent(Guid sampleId, string name)
    {
        SampleId = sampleId;
        Name = name;
    }

    public Guid SampleId { get; }
    public string Name { get; }
}
