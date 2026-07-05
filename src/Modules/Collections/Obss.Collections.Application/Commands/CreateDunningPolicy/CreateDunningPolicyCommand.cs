using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.CreateDunningPolicy;

public sealed record CreateDunningPolicyCommand(
    string Name,
    string Description,
    int MaxDunningLevel,
    Dictionary<int, decimal> DunningFees,
    int DaysBetweenActions,
    int EscalationAfterDays) : IRequest<Result<DunningPolicyDto>>;
