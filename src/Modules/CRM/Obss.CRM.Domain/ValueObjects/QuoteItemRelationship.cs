namespace Obss.CRM.Domain.ValueObjects;

public sealed record QuoteItemRelationship(
    Guid ItemId,
    Guid RelatedItemId,
    string Type);
