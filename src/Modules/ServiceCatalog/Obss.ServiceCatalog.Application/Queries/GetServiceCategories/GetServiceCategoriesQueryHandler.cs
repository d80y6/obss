using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategories;

internal sealed class GetServiceCategoriesQueryHandler(IServiceCategoryRepository repository) : IRequestHandler<GetServiceCategoriesQuery, (List<ServiceCategoryDto> Items, int TotalCount)>
{
    public async Task<(List<ServiceCategoryDto> Items, int TotalCount)> Handle(GetServiceCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = await repository.GetAllAsync(cancellationToken);
        var items = query.AsQueryable();

        if (request.ParentCategoryId.HasValue)
            items = items.Where(c => c.ParentCategoryId == request.ParentCategoryId.Value);
        else
            items = items.Where(c => c.ParentCategoryId == null);

        if (!string.IsNullOrEmpty(request.Status))
            items = items.Where(c => c.LifecycleStatus.ToString() == request.Status);

        var total = await items.CountAsync(cancellationToken);
        var result = await items
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return (result.Adapt<List<ServiceCategoryDto>>(), total);
    }
}
