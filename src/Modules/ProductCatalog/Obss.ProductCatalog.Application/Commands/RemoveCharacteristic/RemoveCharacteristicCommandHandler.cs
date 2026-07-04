using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.RemoveCharacteristic;

public sealed class RemoveCharacteristicCommandHandler : IRequestHandler<RemoveCharacteristicCommand, Result>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCharacteristicCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        var characteristic = spec.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId);
        if (characteristic is null)
            return Result.Failure(Error.NotFound(nameof(ProductSpecificationCharacteristic), request.CharacteristicId));

        spec.RemoveCharacteristic(characteristic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
