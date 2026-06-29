using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetProvisioningTemplates;

public sealed class GetProvisioningTemplatesQueryHandler : IRequestHandler<GetProvisioningTemplatesQuery, Result<IReadOnlyList<ProvisioningTemplateDto>>>
{
    private readonly IProvisioningTemplateRepository _templateRepository;

    public GetProvisioningTemplatesQueryHandler(IProvisioningTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<Result<IReadOnlyList<ProvisioningTemplateDto>>> Handle(GetProvisioningTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _templateRepository.GetAllAsync(cancellationToken);
        return Result.Success(templates.Adapt<IReadOnlyList<ProvisioningTemplateDto>>());
    }
}
