using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.PatchProduct;

public sealed class PatchProductCommandHandler : IRequestHandler<PatchProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PatchProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(PatchProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", request.ProductId));

        product.UpdateDetails(
            request.Name.HasValue ? request.Name.Value : product.Name,
            request.Description.HasValue ? request.Description.Value : product.Description,
            request.CategoryId.HasValue ? request.CategoryId.Value : product.CategoryId,
            request.IsShippable.HasValue ? request.IsShippable.Value : product.IsShippable,
            request.Taxable.HasValue ? request.Taxable.Value : product.Taxable,
            request.TaxCategory.HasValue ? request.TaxCategory.Value : product.TaxCategory);

        await _repository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Adapt<ProductDto>());
    }
}
