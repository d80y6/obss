using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;

public sealed record CreateServiceCategoryCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest<Guid>;
