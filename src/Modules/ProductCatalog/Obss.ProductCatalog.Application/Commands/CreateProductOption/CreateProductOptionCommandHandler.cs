using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.Exceptions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProductOption;

public sealed class CreateProductOptionCommandHandler : IRequestHandler<CreateProductOptionCommand, Result<ProductOptionDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductConfigurationRepository _configRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductOptionCommandHandler(
        IProductRepository productRepository,
        IProductConfigurationRepository configRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _configRepository = configRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductOptionDto>> Handle(CreateProductOptionCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<ProductOptionDto>(Error.NotFound(nameof(Product), request.ProductId));

        var option = ProductOption.Create(
            request.ProductId,
            request.Name,
            request.Description,
            request.OptionType,
            request.IsRequired,
            request.IsMultiSelect,
            request.SortOrder);

        if (request.Values is not null)
        {
            foreach (var valueDto in request.Values)
            {
                var optionValue = OptionValue.Create(
                    option.Id,
                    valueDto.Value,
                    valueDto.DisplayName,
                    valueDto.PriceAdjustment,
                    valueDto.IsDefault);
                option.AddValue(optionValue);
            }
        }

        await _configRepository.AddOptionAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(option.Adapt<ProductOptionDto>());
    }
}
