using Obss.CRM.Domain.ValueObjects;

namespace Obss.CRM.Application.DTOs;

public sealed record ContactMediumDto(
    ContactMediumType MediumType,
    bool IsPreferred,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    List<ContactCharValue> Characteristics);
