namespace Obss.EventManagement.Application.DTOs;

public sealed record EventDto(
    Guid Id,
    string EventType,
    string Payload,
    DateTime Timestamp);
