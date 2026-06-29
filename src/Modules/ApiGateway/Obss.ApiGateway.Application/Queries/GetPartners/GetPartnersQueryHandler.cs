using Mapster;
using MediatR;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Queries.GetPartners;

public sealed class GetPartnersQueryHandler : IRequestHandler<GetPartnersQuery, Result<IReadOnlyList<PartnerDto>>>
{
    private readonly IPartnerRepository _partnerRepository;

    public GetPartnersQueryHandler(IPartnerRepository partnerRepository)
    {
        _partnerRepository = partnerRepository;
    }

    public async Task<Result<IReadOnlyList<PartnerDto>>> Handle(GetPartnersQuery request, CancellationToken cancellationToken)
    {
        var partners = await _partnerRepository.GetAllAsync(cancellationToken);
        return Result.Success(partners.Adapt<IReadOnlyList<PartnerDto>>());
    }
}
