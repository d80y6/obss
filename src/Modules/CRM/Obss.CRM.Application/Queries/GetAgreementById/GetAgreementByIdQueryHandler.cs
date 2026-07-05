using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetAgreementById;

public sealed class GetAgreementByIdQueryHandler : IRequestHandler<GetAgreementByIdQuery, Result<AgreementDto>>
{
    private readonly IRepository<Agreement> _agreementRepository;

    public GetAgreementByIdQueryHandler(IRepository<Agreement> agreementRepository)
    {
        _agreementRepository = agreementRepository;
    }

    public async Task<Result<AgreementDto>> Handle(GetAgreementByIdQuery request, CancellationToken cancellationToken)
    {
        var agreement = await _agreementRepository.GetByIdAsync(request.Id, cancellationToken);
        if (agreement is null)
            return Result.Failure<AgreementDto>(Error.NotFound("Agreement", request.Id));

        return Result.Success(agreement.Adapt<AgreementDto>());
    }
}
