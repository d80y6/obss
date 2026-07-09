using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddQuoteItem;

public sealed class AddQuoteItemCommandHandler : IRequestHandler<AddQuoteItemCommand, Result>
{
    private readonly IQuoteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddQuoteItemCommandHandler(IQuoteRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddQuoteItemCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
            return Result.Failure(Error.NotFound(nameof(Quote), request.QuoteId));

        var action = Enum.Parse<QuoteItemAction>(request.Action);
        var item = new QuoteItem(Guid.NewGuid(), action, request.Quantity, request.ProductOfferingId, request.ProductOfferingName, request.ProductId);
        quote.AddItem(item);
        await _repository.UpdateAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
