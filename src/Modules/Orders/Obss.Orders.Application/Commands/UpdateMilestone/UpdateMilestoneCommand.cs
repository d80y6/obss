using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.UpdateMilestone;

public sealed record UpdateMilestoneCommand(
    Guid OrderId,
    Guid MilestoneId,
    string Status,
    DateTime? MilestoneDate) : IRequest<Result>;
