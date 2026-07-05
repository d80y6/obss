using Obss.CRM.Domain.ValueObjects;

namespace Obss.CRM.Application.DTOs;

public sealed record NotificationHubDto(
    HubType HubType,
    string Identifier,
    bool IsOptIn,
    DateTime? ValidFrom,
    DateTime? ValidUntil);
