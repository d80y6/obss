using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrganizationCommandHandler(IRepository<Organization> organizationRepository, IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrganizationDto>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (organization is null)
            return Result.Failure<OrganizationDto>(Error.NotFound("Organization", request.Id));

        if (!Enum.TryParse<CompanyType>(request.CompanyType, true, out var companyType))
            return Result.Failure<OrganizationDto>(Error.Validation($"Invalid company type: {request.CompanyType}"));

        organization.UpdateDetails(
            request.TradingName,
            companyType,
            request.Industry,
            request.RegistrationNumber,
            request.TaxNumber,
            request.CountryOfRegistration);

        await _organizationRepository.UpdateAsync(organization, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(organization.Adapt<OrganizationDto>());
    }
}
