using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceSpecifications;

internal sealed class GetServiceSpecificationsQueryHandler(IServiceSpecificationRepository repository) : IRequestHandler<GetServiceSpecificationsQuery, (List<ServiceSpecificationDto> Items, int TotalCount)>
{
    public async Task<(List<ServiceSpecificationDto> Items, int TotalCount)> Handle(GetServiceSpecificationsQuery request, CancellationToken cancellationToken)
    {
        var query = await repository.GetAllAsync(cancellationToken);
        var items = query.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
            items = items.Where(s => s.LifecycleStatus.ToString() == request.Status);

        if (!string.IsNullOrEmpty(request.Brand))
            items = items.Where(s => s.Brand == request.Brand);

        var total = await items.CountAsync(cancellationToken);
        var result = await items
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return (result.Adapt<List<ServiceSpecificationDto>>(), total);
    }
}
