using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class Agreement : AggregateRoot<Guid>
{
    private Agreement() { }

    public Agreement(Guid customerId, string name, AgreementType agreementType)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Name = name;
        AgreementType = agreementType;
        Status = "Active";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AgreementType AgreementType { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public string? Description { get; private set; }
    public DateTime? SignedAt { get; private set; }
    public string? SignedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSigned(DateTime signedAt, string signedBy)
    {
        SignedAt = signedAt;
        SignedBy = signedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidFor(DateTime? validFrom, DateTime? validUntil)
    {
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        UpdatedAt = DateTime.UtcNow;
    }
}
