namespace Obss.CRM.Application.DTOs;

public sealed record CustomerSegmentAssignmentDto(
    Guid Id,
    Guid CustomerId,
    Guid SegmentId,
    DateTime AssignedAt,
    Guid AssignedBy,
    bool IsAutoAssigned);
