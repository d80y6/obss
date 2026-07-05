using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetOrganizationById;

public sealed class GetOrganizationByIdQueryHandler : IRequestHandler<GetOrganizationByIdQuery, Result<OrganizationDto>>
{
    private readonly IRepository<Organization> _organizationRepository;

    public GetOrganizationByIdQueryHandler(IRepository<Organization> organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<OrganizationDto>> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (organization is null)
            return Result.Failure<OrganizationDto>(Error.NotFound("Organization", request.Id));

        return Result.Success(organization.Adapt<OrganizationDto>());
    }
}
