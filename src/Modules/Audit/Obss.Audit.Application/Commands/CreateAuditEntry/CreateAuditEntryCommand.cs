using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.CreateAuditEntry;

public sealed record CreateAuditEntryCommand(
    string EntityType,
    string EntityId,
    string Action,
    string? Changes,
    string? PerformedById,
    string? PerformedByName,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    bool IsSensitive = false) : IRequest<Result<AuditEntryDto>>;
