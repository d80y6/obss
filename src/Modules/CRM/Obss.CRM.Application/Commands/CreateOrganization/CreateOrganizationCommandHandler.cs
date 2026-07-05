using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrganizationCommandHandler(IRepository<Organization> organizationRepository, IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrganizationDto>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CompanyType>(request.CompanyType, true, out var companyType))
            return Result.Failure<OrganizationDto>(Error.Validation($"Invalid company type: {request.CompanyType}"));

        var organization = Organization.Create(
            request.TradingName,
            companyType,
            request.Industry,
            request.RegistrationNumber,
            request.TaxNumber,
            request.CountryOfRegistration);

        await _organizationRepository.AddAsync(organization, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(organization.Adapt<OrganizationDto>());
    }
}
