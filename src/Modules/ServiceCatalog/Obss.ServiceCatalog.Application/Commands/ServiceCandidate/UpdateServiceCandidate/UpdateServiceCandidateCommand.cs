using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.UpdateServiceCandidate;

public sealed record UpdateServiceCandidateCommand(
    Guid Id,
    string Name,
    string? Description,
    string? FeatureSpecification
) : IRequest;
