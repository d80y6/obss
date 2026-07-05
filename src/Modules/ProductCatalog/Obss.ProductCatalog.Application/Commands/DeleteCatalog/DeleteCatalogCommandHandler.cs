using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.DeleteCatalog;

public sealed class DeleteCatalogCommandHandler : IRequestHandler<DeleteCatalogCommand, Result>
{
    private readonly ICatalogRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCatalogCommandHandler(ICatalogRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCatalogCommand request, CancellationToken cancellationToken)
    {
        var catalog = await _repository.GetByIdAsync(request.CatalogId, cancellationToken);
        if (catalog is null)
            return Result.Failure(Error.NotFound("Catalog", request.CatalogId));

        await _repository.DeleteAsync(catalog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
