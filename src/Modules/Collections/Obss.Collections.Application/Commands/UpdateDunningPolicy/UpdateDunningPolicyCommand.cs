using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.UpdateDunningPolicy;

public sealed record UpdateDunningPolicyCommand(
    Guid Id,
    string Name,
    string Description,
    int MaxDunningLevel,
    Dictionary<int, decimal> DunningFees,
    int DaysBetweenActions,
    int EscalationAfterDays,
    bool IsActive) : IRequest<Result<DunningPolicyDto>>;
