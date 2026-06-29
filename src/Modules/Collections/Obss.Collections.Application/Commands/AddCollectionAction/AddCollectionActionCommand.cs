using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.AddCollectionAction;

public sealed record AddCollectionActionCommand(
    Guid CollectionCaseId,
    string ActionType,
    int DunningLevel,
    string Description,
    string PerformedBy,
    DateTime? NextActionDate) : IRequest<Result<CollectionCaseDto>>;
