using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;

internal sealed class CreateServiceCategoryCommandHandler(IServiceCategoryRepository repository) : IRequestHandler<CreateServiceCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Domain.Entities.ServiceCategory.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.ValidFrom,
            request.ValidTo);

        await repository.AddAsync(category, cancellationToken);
        return category.Id;
    }
}
