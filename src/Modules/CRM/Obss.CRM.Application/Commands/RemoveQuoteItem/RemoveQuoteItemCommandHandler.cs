using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.RemoveQuoteItem;

public sealed class RemoveQuoteItemCommandHandler : IRequestHandler<RemoveQuoteItemCommand, Result>
{
    private readonly IQuoteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveQuoteItemCommandHandler(IQuoteRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveQuoteItemCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
            return Result.Failure(Error.NotFound(nameof(Quote), request.QuoteId));

        quote.RemoveItem(request.ItemId);
        await _repository.UpdateAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
