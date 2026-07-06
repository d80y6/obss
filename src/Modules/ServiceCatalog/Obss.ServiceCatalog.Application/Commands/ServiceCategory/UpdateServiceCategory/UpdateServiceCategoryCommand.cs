using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;

public sealed record UpdateServiceCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest;
