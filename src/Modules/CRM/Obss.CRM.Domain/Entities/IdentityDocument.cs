using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class IdentityDocument : Entity<Guid>
{
    private IdentityDocument() { }

    public IdentityDocument(
        Guid id,
        Guid individualId,
        DocumentType documentType,
        string documentNumber,
        string? issuingAuthority,
        string? issuingCountry,
        DateTime? issuedDate,
        DateTime? expiryDate)
        : base(id)
    {
        IndividualId = individualId;
        DocumentType = documentType;
        DocumentNumber = documentNumber;
        IssuingAuthority = issuingAuthority;
        IssuingCountry = issuingCountry;
        IssuedDate = issuedDate;
        ExpiryDate = expiryDate;
        IsVerified = false;
    }

    public Guid IndividualId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public string? IssuingAuthority { get; private set; }
    public string? IssuingCountry { get; private set; }
    public DateTime? IssuedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public bool IsVerified { get; private set; }

    public void MarkVerified() => IsVerified = true;
}
