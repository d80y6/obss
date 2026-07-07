namespace Obss.Subscriptions.Application.DTOs;

public sealed record SubscriptionSummaryDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string OfferName,
    string Status,
    string BillingPeriod,
    decimal Price,
    int Quantity,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? RenewalDate,
    DateTime? ActivationDate,
    string? Name,
    string? Description,
    string? ExternalId,
    string? Href);
