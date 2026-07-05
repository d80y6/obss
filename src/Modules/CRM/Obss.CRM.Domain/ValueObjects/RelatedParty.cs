using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Domain.ValueObjects;

public sealed record RelatedParty(string Name, string Role, Guid ReferredId, string ReferredType);
