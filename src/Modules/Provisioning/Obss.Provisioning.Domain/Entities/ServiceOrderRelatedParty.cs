using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderRelatedParty : Entity<Guid>
{
    public ServiceOrderRelatedParty(Guid id, string? name, string? role, string? partyId)
        : base(id)
    {
        Name = name;
        Role = role;
        PartyId = partyId;
    }

    private ServiceOrderRelatedParty() { }

    public string? Name { get; private set; }
    public string? Role { get; private set; }
    public string? PartyId { get; private set; }
}
