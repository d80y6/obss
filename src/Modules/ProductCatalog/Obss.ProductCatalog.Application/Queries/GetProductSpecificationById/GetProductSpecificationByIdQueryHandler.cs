using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecificationById;

public sealed class GetProductSpecificationByIdQueryHandler : IRequestHandler<GetProductSpecificationByIdQuery, Result<ProductSpecificationDto>>
{
    private readonly IProductSpecificationRepository _repository;

    public GetProductSpecificationByIdQueryHandler(IProductSpecificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductSpecificationDto>> Handle(GetProductSpecificationByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationDto>(Error.NotFound(nameof(ProductSpecification), request.Id));

        return Result.Success(spec.Adapt<ProductSpecificationDto>());
    }
}
