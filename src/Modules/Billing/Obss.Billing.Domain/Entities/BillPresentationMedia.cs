using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BillPresentationMedia : Entity<Guid>
{
    private BillPresentationMedia() { }

    public BillPresentationMedia(MediaType type, string? emailAddress, string? paperFormat, string language, bool isPreferred)
    {
        Id = Guid.NewGuid();
        MediaType = type;
        EmailAddress = emailAddress;
        PaperFormat = paperFormat;
        Language = language;
        IsPreferred = isPreferred;
        ValidFrom = DateTime.UtcNow;
    }

    public MediaType MediaType { get; private set; }
    public string? EmailAddress { get; private set; }
    public string? PaperFormat { get; private set; }
    public string Language { get; private set; } = "en";
    public bool IsPreferred { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }

    public void Update(string? emailAddress, string? paperFormat, string? language, bool? isPreferred)
    {
        if (emailAddress is not null) EmailAddress = emailAddress;
        if (paperFormat is not null) PaperFormat = paperFormat;
        if (language is not null) Language = language;
        if (isPreferred.HasValue) IsPreferred = isPreferred.Value;
    }

    public void Deactivate() => ValidUntil = DateTime.UtcNow;
}
