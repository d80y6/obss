namespace Obss.CRM.Application.DTOs;

public sealed record PaymentMethodRefDto(Guid PaymentMethodId, string Name, string? Href);
