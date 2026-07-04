using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProductSpecification;

public sealed class CreateProductSpecificationCommandHandler : IRequestHandler<CreateProductSpecificationCommand, Result<ProductSpecificationDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductSpecificationCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationDto>> Handle(CreateProductSpecificationCommand request, CancellationToken cancellationToken)
    {
        var spec = Domain.Domain.Entities.ProductSpecification.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.Brand,
            request.Version,
            request.ProductNumber ?? string.Empty);

        if (request.Characteristics is not null)
        {
            foreach (var charItem in request.Characteristics)
            {
                var characteristic = new ProductSpecificationCharacteristic(
                    Guid.NewGuid(),
                    spec.Id,
                    charItem.Name,
                    charItem.Description,
                    charItem.ValueType,
                    charItem.Configurable,
                    charItem.MinValue,
                    charItem.MaxValue,
                    charItem.Regex,
                    charItem.SortOrder,
                    charItem.MaxCardinality,
                    charItem.IsRequired);

                if (charItem.Values is not null)
                {
                    foreach (var valItem in charItem.Values)
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
            }
        }

        if (request.Relationships is not null)
        {
            foreach (var relItem in request.Relationships)
            {
                var relationship = new ProductSpecificationRelationship(
                    Guid.NewGuid(),
                    spec.Id,
                    relItem.TargetSpecificationId,
                    relItem.RelationshipType,
                    relItem.Role,
                    relItem.ValidFrom,
                    relItem.ValidTo);
                spec.AddRelationship(relationship);
            }
        }

        await _repository.AddAsync(spec, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(spec.Adapt<ProductSpecificationDto>());
    }
}
