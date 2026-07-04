using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.AddCharacteristic;

public sealed class AddCharacteristicCommandHandler : IRequestHandler<AddCharacteristicCommand, Result<ProductSpecificationCharacteristicDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCharacteristicCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationCharacteristicDto>> Handle(AddCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationCharacteristicDto>(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        var characteristic = new ProductSpecificationCharacteristic(
            Guid.NewGuid(),
            spec.Id,
            request.Name,
            request.Description,
            request.ValueType,
            request.Configurable,
            request.MinValue,
            request.MaxValue,
            request.Regex,
            request.SortOrder,
            request.MaxCardinality,
            request.IsRequired);

        if (request.Values is not null)
        {
            foreach (var valItem in request.Values)
            {
                var value = new ProductSpecificationCharacteristicValue(
                    Guid.NewGuid(),
                    characteristic.Id,
                    valItem.Value,
                    valItem.UnitOfMeasure,
                    valItem.IsDefault,
                    valItem.ValueFrom,
                    valItem.ValueTo,
                    valItem.RangeInterval,
                    valItem.ValidFrom,
                    valItem.ValidTo);
                characteristic.AddValue(value);
            }
        }

        spec.AddCharacteristic(characteristic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(characteristic.Adapt<ProductSpecificationCharacteristicDto>());
    }
}
