using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.UpdateProductSpecification;

public sealed class UpdateProductSpecificationCommandHandler : IRequestHandler<UpdateProductSpecificationCommand, Result<ProductSpecificationDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductSpecificationCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationDto>> Handle(UpdateProductSpecificationCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationDto>(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        spec.UpdateDetails(
            request.Name,
            request.Description,
            request.Brand,
            request.Version,
            request.ProductNumber ?? string.Empty);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(spec.Adapt<ProductSpecificationDto>());
    }
}
