using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetEntityAuditTrail;

public sealed record GetEntityAuditTrailQuery(
    string EntityType,
    string EntityId) : IRequest<Result<IReadOnlyList<AuditEntryDto>>>;
