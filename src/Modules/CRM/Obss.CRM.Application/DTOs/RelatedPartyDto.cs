namespace Obss.CRM.Application.DTOs;

public sealed record RelatedPartyDto(string Name, string Role, Guid ReferredId, string ReferredType);
