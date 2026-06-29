using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.ResolveCollectionCase;

public sealed class ResolveCollectionCaseCommandHandler : IRequestHandler<ResolveCollectionCaseCommand, Result<CollectionCaseDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResolveCollectionCaseCommandHandler> _logger;

    public ResolveCollectionCaseCommandHandler(
        ICollectionCaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ILogger<ResolveCollectionCaseCommandHandler> logger)
    {
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CollectionCaseDto>> Handle(ResolveCollectionCaseCommand request, CancellationToken cancellationToken)
    {
        var @case = await _caseRepository.GetByIdWithDetailsAsync(request.CaseId, cancellationToken);
        if (@case is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("CollectionCase", request.CaseId));

        try
        {
            @case.Resolve();
        }
        catch (Exception ex)
        {
            return Result.Failure<CollectionCaseDto>(Error.Conflict(ex.Message));
        }

        await _caseRepository.UpdateAsync(@case, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Collection case {CaseId} resolved.", request.CaseId);

        return Result.Success(@case.Adapt<CollectionCaseDto>());
    }
}
