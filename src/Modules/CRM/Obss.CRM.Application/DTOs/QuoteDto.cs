namespace Obss.CRM.Application.DTOs;

public sealed record QuoteDto(
    Guid Id,
    string TenantId,
    string? ExternalId,
    string State,
    DateTime QuoteDate,
    string? Category,
    string? Description,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    DateTime? ExpectedQuoteCompletionDate,
    DateTime? EffectiveQuoteCompletionDate,
    DateTime? ExpectedFulfillmentStartDate,
    Guid CustomerId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<QuoteItemDto> Items,
    List<RelatedPartyDto> RelatedParties,
    List<QuotePriceDto> QuotePrices,
    List<QuoteAuthorizationDto> Authorizations,
    List<AccountRefDto> BillingAccountRefs,
    List<AgreementRefDto> AgreementRefs,
    List<NoteDto> Notes);

public sealed record QuoteItemDto(
    Guid Id,
    string Action,
    string State,
    int Quantity,
    Guid? ProductOfferingId,
    string? ProductOfferingName,
    Guid? ProductId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<QuotePriceDto> Prices,
    List<QuoteItemRelationshipDto> ItemRelationships,
    List<NoteDto> Notes);

public sealed record QuotePriceDto(
    string PriceType,
    string Name,
    decimal DutyFreeAmount,
    decimal TaxIncludedAmount,
    decimal TaxRate,
    string Currency,
    string? UnitOfMeasure,
    int? RecurringPeriod,
    string? RecurringPeriodUnit);

public sealed record PriceAlterationDto(
    string Name,
    string? Description,
    string PriceType,
    int? ApplicationDuration,
    int Priority,
    decimal DutyFreeAmount,
    decimal TaxIncludedAmount,
    decimal TaxRate,
    string Currency);

public sealed record QuoteAuthorizationDto(
    string State,
    DateTime RequestedDate,
    DateTime? GivenDate,
    string? ApproverName,
    string? ApproverRole);

public sealed record QuoteItemRelationshipDto(
    Guid ItemId,
    Guid RelatedItemId,
    string Type);

public sealed record NoteDto(
    DateTime Date,
    string Author,
    string Text);
