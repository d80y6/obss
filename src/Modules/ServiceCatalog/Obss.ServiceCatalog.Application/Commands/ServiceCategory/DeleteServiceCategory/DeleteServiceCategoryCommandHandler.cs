using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.DeleteServiceCategory;

internal sealed class DeleteServiceCategoryCommandHandler(IServiceCategoryRepository repository) : IRequestHandler<DeleteServiceCategoryCommand>
{
    public async Task Handle(DeleteServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service category {request.Id} not found");

        await repository.DeleteAsync(category, cancellationToken);
    }
}
