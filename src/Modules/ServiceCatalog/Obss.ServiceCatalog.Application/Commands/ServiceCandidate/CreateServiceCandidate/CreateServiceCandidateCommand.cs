using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.CreateServiceCandidate;

public sealed record CreateServiceCandidateCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid? ServiceSpecificationId,
    Guid? BaseCandidateId,
    string? FeatureSpecification,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    List<Guid>? CategoryIds
) : IRequest<Guid>;
