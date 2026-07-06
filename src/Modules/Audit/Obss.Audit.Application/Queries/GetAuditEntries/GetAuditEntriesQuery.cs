using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAuditEntries;

public sealed record GetAuditEntriesQuery(
    string? EntityType,
    string? EntityId,
    string? Action,
    string? PerformedById,
    DateTime? FromDate,
    DateTime? ToDate,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<AuditEntryDto>>>;
