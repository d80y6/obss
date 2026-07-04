using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.CategoryId,
            request.ProductType,
            request.IsShippable,
            request.Taxable,
            request.TaxCategory);

        if (request.Specifications is not null)
        {
            foreach (var spec in request.Specifications)
            {
                product.AddSpecification(new Obss.ProductCatalog.Domain.Domain.ValueObjects.ProductSpecification(spec.Name, spec.Value, spec.IsRequired));


            }
        }

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Adapt<ProductDto>());
    }
}
