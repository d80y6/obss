using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAlertRules;

public sealed record GetAlertRulesQuery : IRequest<Result<IReadOnlyList<AuditAlertRuleDto>>>;
