namespace Obss.CRM.Domain.ValueObjects;

public sealed record AgreementRef(Guid AgreementId, string Name, string AgreementType, string Role, string? Href);
