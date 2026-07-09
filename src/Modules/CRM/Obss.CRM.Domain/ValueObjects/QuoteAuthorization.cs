namespace Obss.CRM.Domain.ValueObjects;

public sealed record QuoteAuthorization(
    AuthorizationState State,
    DateTime RequestedDate,
    DateTime? GivenDate,
    string? ApproverName,
    string? ApproverRole);
