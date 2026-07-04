using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.AddSpecificationRelationship;

public sealed class AddSpecificationRelationshipCommandHandler : IRequestHandler<AddSpecificationRelationshipCommand, Result<ProductSpecificationRelationshipDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddSpecificationRelationshipCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationRelationshipDto>> Handle(AddSpecificationRelationshipCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationRelationshipDto>(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        var relationship = new ProductSpecificationRelationship(
            Guid.NewGuid(),
            spec.Id,
            request.TargetSpecificationId,
            request.RelationshipType,
            request.Role,
            request.ValidFrom,
            request.ValidTo);

        spec.AddRelationship(relationship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(relationship.Adapt<ProductSpecificationRelationshipDto>());
    }
}
