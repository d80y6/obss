using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid NotificationId) : IRequest<Result<NotificationDto>>;
