using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.UpdateCharacteristicValue;

public sealed class UpdateCharacteristicValueCommandHandler : IRequestHandler<UpdateCharacteristicValueCommand, Result<ProductSpecificationCharacteristicValueDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCharacteristicValueCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationCharacteristicValueDto>> Handle(UpdateCharacteristicValueCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationCharacteristicValueDto>(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        var characteristic = spec.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId);
        if (characteristic is null)
            return Result.Failure<ProductSpecificationCharacteristicValueDto>(Error.NotFound(nameof(ProductSpecificationCharacteristic), request.CharacteristicId));

        var value = characteristic.Values.FirstOrDefault(v => v.Id == request.ValueId);
        if (value is null)
            return Result.Failure<ProductSpecificationCharacteristicValueDto>(Error.NotFound(nameof(ProductSpecificationCharacteristicValue), request.ValueId));

        value.Update(
            request.Value,
            request.UnitOfMeasure,
            request.IsDefault,
            request.ValueFrom,
            request.ValueTo,
            request.RangeInterval,
            request.ValidFrom,
            request.ValidTo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(value.Adapt<ProductSpecificationCharacteristicValueDto>());
    }
}
