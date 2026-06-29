using Mapster;
using MediatR;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Application.DTOs;
using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.RegisterPartner;

public sealed class RegisterPartnerCommandHandler : IRequestHandler<RegisterPartnerCommand, Result<PartnerDto>>
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public RegisterPartnerCommandHandler(
        IPartnerRepository partnerRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<PartnerDto>> Handle(RegisterPartnerCommand request, CancellationToken cancellationToken)
    {
        var existing = await _partnerRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            return Result.Failure<PartnerDto>(Error.Conflict($"Partner '{request.Name}' already exists."));

        var partner = Partner.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.Name,
            request.ContactName,
            request.ContactEmail,
            request.AllowedIPs,
            request.SlaLevel,
            request.MaxRequestsPerDay);

        await _partnerRepository.AddAsync(partner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(partner.Adapt<PartnerDto>());
    }
}
