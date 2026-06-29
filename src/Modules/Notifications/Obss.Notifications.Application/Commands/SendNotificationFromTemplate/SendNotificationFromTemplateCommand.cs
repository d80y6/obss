using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.SendNotificationFromTemplate;

public sealed record SendNotificationFromTemplateCommand(
    string TenantId,
    Guid TemplateId,
    string Channel,
    string Recipient,
    Dictionary<string, string> Variables,
    string? ReferenceType,
    Guid? ReferenceId) : IRequest<Result<NotificationDto>>;
