using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.DTOs;

public sealed record SubscriptionDto(
    Guid Id,
    string TenantId,
    Guid CustomerId,
    string CustomerName,
    Guid OrderId,
    Guid OrderItemId,
    Guid ProductId,
    Guid OfferId,
    string OfferName,
    string Status,
    string BillingPeriod,
    string Currency,
    decimal Price,
    int Quantity,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? CancelledAt,
    DateTime? SuspendedAt,
    DateTime? ActivationDate,
    DateTime? RenewalDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Href,
    string? AtType,
    string? AtBaseType,
    string? AtSchemaLocation,
    string? Name,
    string? Description,
    string? ExternalId,
    string? CompletionDate,
    List<SubscriptionAddOnDto> AddOns,
    List<SubscriptionServiceDto> Services,
    List<RelatedPartyDto> RelatedParties);

public sealed record RelatedPartyDto(
    string PartyId,
    string PartyName,
    string Role);

public sealed record SubscriptionAddOnDto(
    Guid Id,
    Guid OfferId,
    string OfferName,
    decimal Price,
    int Quantity,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive);

public sealed record SubscriptionServiceDto(
    Guid Id,
    Guid ServiceId,
    string ServiceType,
    string Status,
    DateTime ProvisionedAt);
