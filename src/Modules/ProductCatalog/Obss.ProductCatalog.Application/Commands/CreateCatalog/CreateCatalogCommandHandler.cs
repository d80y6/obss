using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateCatalog;

public sealed class CreateCatalogCommandHandler : IRequestHandler<CreateCatalogCommand, Result<CatalogDto>>
{
    private readonly ICatalogRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCatalogCommandHandler(ICatalogRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CatalogDto>> Handle(CreateCatalogCommand request, CancellationToken cancellationToken)
    {
        var catalog = Catalog.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.CatalogType,
            request.Version,
            request.ValidFrom,
            request.ValidTo);

        await _repository.AddAsync(catalog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(catalog.Adapt<CatalogDto>());
    }
}
