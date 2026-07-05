using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.SearchAgreements;

public sealed class SearchAgreementsQueryHandler : IRequestHandler<SearchAgreementsQuery, Result<List<AgreementDto>>>
{
    private readonly IRepository<Agreement> _agreementRepository;

    public SearchAgreementsQueryHandler(IRepository<Agreement> agreementRepository)
    {
        _agreementRepository = agreementRepository;
    }

    public async Task<Result<List<AgreementDto>>> Handle(SearchAgreementsQuery request, CancellationToken cancellationToken)
    {
        var allAgreements = await _agreementRepository.GetAllAsync(cancellationToken);

        var filtered = allAgreements.AsEnumerable();

        if (request.CustomerId.HasValue)
            filtered = filtered.Where(a => a.CustomerId == request.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            filtered = filtered.Where(a => a.Status == request.Status);

        return Result.Success(filtered.Adapt<List<AgreementDto>>());
    }
}
