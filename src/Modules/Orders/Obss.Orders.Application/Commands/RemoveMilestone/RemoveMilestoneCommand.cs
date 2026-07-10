using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.RemoveMilestone;

public sealed record RemoveMilestoneCommand(Guid OrderId, Guid MilestoneId) : IRequest<Result>;
