using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.PatchProductSpecification;

public sealed class PatchProductSpecificationCommandHandler : IRequestHandler<PatchProductSpecificationCommand, Result<ProductSpecificationDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PatchProductSpecificationCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationDto>> Handle(PatchProductSpecificationCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationDto>(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        spec.UpdateDetails(
            request.Name ?? spec.Name,
            request.Description ?? spec.Description,
            request.Brand ?? spec.Brand,
            request.Version ?? spec.Version,
            request.ProductNumber ?? spec.ProductNumber);

        if (request.LifecycleStatus.HasValue)
        {
            switch (request.LifecycleStatus.Value)
            {
                case Domain.Domain.ValueObjects.LifecycleStatus.Active:
                    spec.Activate();
                    break;
                case Domain.Domain.ValueObjects.LifecycleStatus.Retired:
                    spec.Retire();
                    break;
                case Domain.Domain.ValueObjects.LifecycleStatus.Discontinued:
                    spec.Discontinue();
                    break;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(spec.Adapt<ProductSpecificationDto>());
    }
}
