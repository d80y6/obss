using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.UpdateQuoteItem;

public sealed class UpdateQuoteItemCommandHandler : IRequestHandler<UpdateQuoteItemCommand, Result>
{
    private readonly IQuoteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuoteItemCommandHandler(IQuoteRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateQuoteItemCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
            return Result.Failure(Error.NotFound(nameof(Quote), request.QuoteId));

        var action = Enum.Parse<QuoteItemAction>(request.Action);
        quote.UpdateItem(request.ItemId, action, request.Quantity, request.ProductOfferingId, request.ProductOfferingName, request.ProductId);
        await _repository.UpdateAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
