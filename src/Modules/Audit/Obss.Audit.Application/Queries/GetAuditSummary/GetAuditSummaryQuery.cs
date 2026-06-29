using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAuditSummary;

public sealed record GetAuditSummaryQuery : IRequest<Result<AuditSummaryDto>>;
