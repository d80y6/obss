using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;

internal sealed class UpdateServiceCategoryCommandHandler(IServiceCategoryRepository repository) : IRequestHandler<UpdateServiceCategoryCommand>
{
    public async Task Handle(UpdateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service category {request.Id} not found");

        category.UpdateDetails(request.Name, request.Description);
        category.SetValidityPeriod(request.ValidFrom, request.ValidTo);

        await repository.UpdateAsync(category, cancellationToken);
    }
}
