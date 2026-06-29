using Mapster;
using MediatR;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetCollectionCaseById;

public sealed class GetCollectionCaseByIdQueryHandler : IRequestHandler<GetCollectionCaseByIdQuery, Result<CollectionCaseDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;

    public GetCollectionCaseByIdQueryHandler(ICollectionCaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<CollectionCaseDto>> Handle(GetCollectionCaseByIdQuery request, CancellationToken cancellationToken)
    {
        var @case = await _caseRepository.GetByIdWithDetailsAsync(request.CaseId, cancellationToken);
        if (@case is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("CollectionCase", request.CaseId));

        return Result.Success(@case.Adapt<CollectionCaseDto>());
    }
}
