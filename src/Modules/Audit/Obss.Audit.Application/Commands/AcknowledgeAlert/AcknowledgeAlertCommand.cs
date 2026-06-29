using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.AcknowledgeAlert;

public sealed record AcknowledgeAlertCommand(Guid Id) : IRequest<Result<AuditAlertDto>>;
