using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork, ILogger<CreateProductCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.TenantId, request.CustomerId, request.Name, request.Description,
            request.ProductSpecificationId, request.ProductOfferingId);

        await _repository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created Product {Id} for customer {CustomerId}", product.Id, product.CustomerId);

        return Result.Success(product.Adapt<ProductDto>());
    }
}
