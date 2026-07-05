namespace Obss.CRM.Application.DTOs;

public sealed record CreditProfileDto(
    Guid Id,
    Guid CustomerId,
    int Score,
    string ScoreType,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? RiskRating);
