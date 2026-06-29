using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Application.Commands.CreateSlaDefinition;

public sealed record CreateSlaDefinitionCommand(
    string TenantId,
    string Name,
    string? Description,
    TicketPriority Priority,
    int ResponseTimeHours,
    int ResolutionTimeHours,
    int EscalationTimeHours) : IRequest<Result<SlaDefinitionDto>>;
