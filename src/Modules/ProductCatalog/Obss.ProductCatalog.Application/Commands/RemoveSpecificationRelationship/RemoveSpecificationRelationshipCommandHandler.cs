using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Commands.RemoveSpecificationRelationship;

public sealed class RemoveSpecificationRelationshipCommandHandler : IRequestHandler<RemoveSpecificationRelationshipCommand, Result>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSpecificationRelationshipCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveSpecificationRelationshipCommand request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.ProductSpecificationId, cancellationToken);
        if (spec is null)
            return Result.Failure(Error.NotFound(nameof(ProductSpecification), request.ProductSpecificationId));

        var relationship = spec.Relationships.FirstOrDefault(r => r.Id == request.RelationshipId);
        if (relationship is null)
            return Result.Failure(Error.NotFound(nameof(ProductSpecificationRelationship), request.RelationshipId));

        spec.RemoveRelationship(relationship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
