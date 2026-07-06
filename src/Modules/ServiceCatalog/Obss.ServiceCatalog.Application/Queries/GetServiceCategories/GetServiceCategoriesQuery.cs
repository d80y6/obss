using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategories;

public sealed record GetServiceCategoriesQuery(
    string TenantId,
    Guid? ParentCategoryId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<(List<ServiceCategoryDto> Items, int TotalCount)>;
