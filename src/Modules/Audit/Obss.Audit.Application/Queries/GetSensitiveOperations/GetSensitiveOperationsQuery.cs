using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetSensitiveOperations;

public sealed record GetSensitiveOperationsQuery(DateTime From, DateTime To) : IRequest<Result<IReadOnlyList<AuditEntryDto>>>;
