using Mapster;
using MediatR;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetCollectionCases;

public sealed class GetCollectionCasesQueryHandler : IRequestHandler<GetCollectionCasesQuery, Result<IReadOnlyList<CollectionCaseDto>>>
{
    private readonly ICollectionCaseRepository _caseRepository;

    public GetCollectionCasesQueryHandler(ICollectionCaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<IReadOnlyList<CollectionCaseDto>>> Handle(GetCollectionCasesQuery request, CancellationToken cancellationToken)
    {
        var cases = await _caseRepository.FindPagedAsync(c =>
            (!request.CustomerId.HasValue || c.CustomerId == request.CustomerId.Value) &&
            (string.IsNullOrEmpty(request.Status) || c.Status == Enum.Parse<CollectionCaseStatus>(request.Status, true)) &&
            (!request.DunningLevel.HasValue || c.CurrentDunningLevel == request.DunningLevel.Value),
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success<IReadOnlyList<CollectionCaseDto>>(cases.Adapt<List<CollectionCaseDto>>());
    }
}
