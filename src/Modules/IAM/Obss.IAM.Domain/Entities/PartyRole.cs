using Obss.SharedKernel.Domain.Common;

namespace Obss.IAM.Domain.Entities;

public class PartyRole : AggregateRoot<Guid>
{
    private PartyRole() { }

    private PartyRole(
        Guid id,
        Guid partyId,
        Guid roleId,
        string name,
        string? description,
        DateTime? validFrom,
        DateTime? validUntil)
        : base(id)
    {
        PartyId = partyId;
        RoleId = roleId;
        Name = name;
        Description = description;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        Status = "active";
        CreatedAt = DateTime.UtcNow;
    }

    public Guid PartyId { get; private set; }
    public Guid RoleId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Status { get; private set; } = "active";
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static PartyRole Create(
        Guid partyId,
        Guid roleId,
        string name,
        string? description,
        DateTime? validFrom,
        DateTime? validUntil)
    {
        return new PartyRole(
            Guid.NewGuid(),
            partyId,
            roleId,
            name,
            description,
            validFrom,
            validUntil);
    }

    public void Activate()
    {
        if (Status == "active") return;
        Status = "active";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status == "suspended") return;
        Status = "suspended";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Terminate()
    {
        if (Status == "terminated") return;
        Status = "terminated";
        ValidUntil = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string? name, string? description, DateTime? validFrom, DateTime? validUntil)
    {
        if (name is not null)
            Name = name;
        if (description is not null)
            Description = description;
        if (validFrom.HasValue)
            ValidFrom = validFrom;
        if (validUntil.HasValue)
            ValidUntil = validUntil;
        UpdatedAt = DateTime.UtcNow;
    }
}
