using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CreateMilestone;

public sealed record CreateMilestoneCommand(
    Guid OrderId,
    string Name,
    string Description,
    DateTime MilestoneDate) : IRequest<Result>;
