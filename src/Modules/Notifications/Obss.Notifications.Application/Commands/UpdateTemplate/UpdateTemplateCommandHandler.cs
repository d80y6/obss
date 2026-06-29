using Mapster;
using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.UpdateTemplate;

public sealed class UpdateTemplateCommandHandler
    : IRequestHandler<UpdateTemplateCommand, Result<NotificationTemplateDto>>
{
    private readonly IRepository<NotificationTemplate> _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTemplateCommandHandler(
        IRepository<NotificationTemplate> templateRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NotificationTemplateDto>> Handle(
        UpdateTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);

        if (template is null)
            return Result.Failure<NotificationTemplateDto>(
                Error.NotFound(nameof(NotificationTemplate), request.TemplateId));

        if (request.Name is not null)
        {
            var newVersion = template.CreateNewVersion();
            newVersion.Update(request.Name, request.Description, request.Subject, request.Body, request.Variables);
            await _templateRepository.AddAsync(newVersion, cancellationToken);
        }
        else
        {
            template.Update(
                request.Name,
                request.Description,
                request.Subject,
                request.Body,
                request.Variables);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                template.Activate();
            else
                template.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(template.Adapt<NotificationTemplateDto>());
    }
}
