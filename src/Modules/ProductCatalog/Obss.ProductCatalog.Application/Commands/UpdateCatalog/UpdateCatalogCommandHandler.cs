using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateCatalog;

public sealed class UpdateCatalogCommandHandler : IRequestHandler<UpdateCatalogCommand, Result<CatalogDto>>
{
    private readonly ICatalogRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCatalogCommandHandler(ICatalogRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CatalogDto>> Handle(UpdateCatalogCommand request, CancellationToken cancellationToken)
    {
        var catalog = await _repository.GetByIdAsync(request.CatalogId, cancellationToken);
        if (catalog is null)
            return Result.Failure<CatalogDto>(Error.NotFound("Catalog", request.CatalogId));

        catalog.UpdateDetails(
            request.Name,
            request.Description,
            request.CatalogType,
            request.Version,
            request.ValidFrom,
            request.ValidTo);

        await _repository.UpdateAsync(catalog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(catalog.Adapt<CatalogDto>());
    }
}
