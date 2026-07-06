using Mapster;
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategoryById;

internal sealed class GetServiceCategoryByIdQueryHandler(IServiceCategoryRepository repository) : IRequestHandler<GetServiceCategoryByIdQuery, ServiceCategoryDto?>
{
    public async Task<ServiceCategoryDto?> Handle(GetServiceCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        return category?.Adapt<ServiceCategoryDto>();
    }
}
