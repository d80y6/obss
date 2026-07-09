using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.UpdateQuote;

public sealed class UpdateQuoteCommandHandler : IRequestHandler<UpdateQuoteCommand, Result<QuoteDto>>
{
    private readonly IQuoteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuoteCommandHandler(IQuoteRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<QuoteDto>> Handle(UpdateQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (quote is null)
            return Result.Failure<QuoteDto>(Error.NotFound(nameof(Quote), request.Id));

        quote.UpdateDetails(request.ExternalId, request.Category, request.Description);
        await _repository.UpdateAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(quote.Adapt<QuoteDto>());
    }
}
