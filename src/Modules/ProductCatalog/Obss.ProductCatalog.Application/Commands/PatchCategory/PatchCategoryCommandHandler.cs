using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.PatchCategory;

public sealed class PatchCategoryCommandHandler : IRequestHandler<PatchCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PatchCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(PatchCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryDto>(Error.NotFound("Category", request.CategoryId));

        category.UpdateDetails(
            request.Name.HasValue ? request.Name.Value : category.Name,
            request.Description.HasValue ? request.Description.Value : category.Description,
            request.SortOrder.HasValue ? request.SortOrder.Value : category.SortOrder);

        if (request.ParentCategoryId.HasValue)
        {
            category.MoveToParent(request.ParentCategoryId.Value);
        }

        await _repository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Adapt<CategoryDto>());
    }
}
