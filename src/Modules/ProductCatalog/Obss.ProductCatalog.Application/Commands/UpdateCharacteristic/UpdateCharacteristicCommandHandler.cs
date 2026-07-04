using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.UpdateCharacteristic;

public sealed class UpdateCharacteristicCommandHandler : IRequestHandler<UpdateCharacteristicCommand, Result<ProductSpecificationCharacteristicDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCharacteristicCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationCharacteristicDto>> Handle(UpdateCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationCharacteristicDto>(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        var characteristic = spec.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId);
        if (characteristic is null)
            return Result.Failure<ProductSpecificationCharacteristicDto>(Error.NotFound(nameof(ProductSpecificationCharacteristic), request.CharacteristicId));

        characteristic.UpdateDetails(
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(characteristic.Adapt<ProductSpecificationCharacteristicDto>());
    }
}
