using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class Individual : AggregateRoot<Guid>
{
    private readonly List<IdentityDocument> _documents = [];

    private Individual() { }

    private Individual(
        Guid id,
        string firstName,
        string lastName,
        string? middleName,
        string? salutation,
        string? title,
        DateTime? birthDate,
        string? nationality,
        string? gender)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Salutation = salutation;
        Title = title;
        BirthDate = birthDate;
        Nationality = nationality;
        Gender = gender;
        KycStatus = KycStatus.NotStarted;
        RiskRating = RiskRating.Low;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string? Salutation { get; private set; }
    public string? Title { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? Nationality { get; private set; }
    public string? Gender { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public DateTime? KycVerifiedAt { get; private set; }
    public string? KycVerifiedBy { get; private set; }
    public RiskRating RiskRating { get; private set; }

    public IReadOnlyCollection<IdentityDocument> Documents => _documents.AsReadOnly();

    public static Individual Create(
        string firstName,
        string lastName,
        string? middleName = null,
        string? salutation = null,
        string? title = null,
        DateTime? birthDate = null,
        string? nationality = null,
        string? gender = null)
    {
        return new Individual(
            Guid.NewGuid(), firstName, lastName, middleName,
            salutation, title, birthDate, nationality, gender);
    }

    public void UpdateDetails(
        string firstName, string lastName, string? middleName,
        string? salutation, string? title, DateTime? birthDate,
        string? nationality, string? gender)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Salutation = salutation;
        Title = title;
        BirthDate = birthDate;
        Nationality = nationality;
        Gender = gender;
    }

    public void AddDocument(IdentityDocument document)
    {
        _documents.Add(document);
    }

    public void RemoveDocument(Guid documentId)
    {
        var doc = _documents.FirstOrDefault(d => d.Id == documentId);
        if (doc is not null)
            _documents.Remove(doc);
    }

    public void VerifyKyc(string verifiedBy)
    {
        KycStatus = KycStatus.Verified;
        KycVerifiedAt = DateTime.UtcNow;
        KycVerifiedBy = verifiedBy;
    }

    public void RejectKyc()
    {
        KycStatus = KycStatus.Rejected;
    }

    public void SetRiskRating(RiskRating rating)
    {
        RiskRating = rating;
    }
}
