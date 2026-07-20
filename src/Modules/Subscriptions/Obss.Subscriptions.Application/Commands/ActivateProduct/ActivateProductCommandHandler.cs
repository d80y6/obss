using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;

namespace Obss.Subscriptions.Application.Commands.ActivateProduct;

public sealed class ActivateProductCommandHandler : IRequestHandler<ActivateProductCommand, Result>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure(Error.NotFound("Product", request.Id));

        product.Activate();

        await _repository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
