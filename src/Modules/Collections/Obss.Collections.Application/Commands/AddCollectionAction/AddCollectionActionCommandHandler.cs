using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.AddCollectionAction;

public sealed class AddCollectionActionCommandHandler : IRequestHandler<AddCollectionActionCommand, Result<CollectionCaseDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddCollectionActionCommandHandler> _logger;

    public AddCollectionActionCommandHandler(
        ICollectionCaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddCollectionActionCommandHandler> logger)
    {
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CollectionCaseDto>> Handle(AddCollectionActionCommand request, CancellationToken cancellationToken)
    {
        var @case = await _caseRepository.GetByIdWithDetailsAsync(request.CollectionCaseId, cancellationToken);
        if (@case is null)
            return Result.Failure<CollectionCaseDto>(Error.NotFound("CollectionCase", request.CollectionCaseId));

        if (!Enum.TryParse<CollectionActionType>(request.ActionType, true, out var actionType))
            return Result.Failure<CollectionCaseDto>(Error.Validation($"Invalid action type: '{request.ActionType}'."));

        var action = CollectionAction.Create(
            @case.Id,
            actionType,
            request.DunningLevel,
            request.Description,
            request.PerformedBy,
            request.NextActionDate);

        @case.AddAction(action);
        await _caseRepository.UpdateAsync(@case, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Collection action {ActionType} added to case {CaseId} at dunning level {Level}.",
            request.ActionType,
            @case.Id,
            request.DunningLevel);

        return Result.Success(@case.Adapt<CollectionCaseDto>());
    }
}
