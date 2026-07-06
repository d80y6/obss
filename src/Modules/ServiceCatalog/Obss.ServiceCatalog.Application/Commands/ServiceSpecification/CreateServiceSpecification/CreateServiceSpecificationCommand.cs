using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.CreateServiceSpecification;

public sealed record CreateServiceSpecificationCommand(
    string TenantId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    bool IsBundle,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest<Guid>;
