using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.CreateAuditPolicy;

public sealed record CreateAuditPolicyCommand(
    string EntityType,
    int RetentionDays,
    bool AlertOnFailure = false,
    bool IsActive = true) : IRequest<Result<AuditPolicyDto>>;
