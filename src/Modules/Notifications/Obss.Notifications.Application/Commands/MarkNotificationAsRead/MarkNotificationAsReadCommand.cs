using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<Result>;
