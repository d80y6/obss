using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningTemplate;

public sealed class CreateProvisioningTemplateCommandHandler : IRequestHandler<CreateProvisioningTemplateCommand, Result<ProvisioningTemplateDto>>
{
    private readonly IProvisioningTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProvisioningTemplateCommandHandler(
        IProvisioningTemplateRepository templateRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProvisioningTemplateDto>> Handle(CreateProvisioningTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = ProvisioningTemplate.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.ServiceType,
            request.Action,
            request.WorkflowDefinitionId);

        await _templateRepository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(template.Adapt<ProvisioningTemplateDto>());
    }
}
