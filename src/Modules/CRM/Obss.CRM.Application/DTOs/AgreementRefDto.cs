namespace Obss.CRM.Application.DTOs;

public sealed record AgreementRefDto(Guid AgreementId, string Name, string AgreementType, string Role, string? Href);
