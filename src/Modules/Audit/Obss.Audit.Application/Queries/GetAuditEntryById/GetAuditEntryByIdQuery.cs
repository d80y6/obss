using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAuditEntryById;

public sealed record GetAuditEntryByIdQuery(Guid Id) : IRequest<Result<AuditEntryDto>>;
