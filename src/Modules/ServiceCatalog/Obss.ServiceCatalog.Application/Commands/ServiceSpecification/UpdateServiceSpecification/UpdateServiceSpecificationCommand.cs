using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateServiceSpecification;

public sealed record UpdateServiceSpecificationCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest;
