using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.UpdateTemplate;

public sealed record UpdateTemplateCommand(
    Guid TemplateId,
    string? Name,
    string? Description,
    string? Subject,
    string? Body,
    List<string>? Variables,
    bool? IsActive) : IRequest<Result<NotificationTemplateDto>>;
