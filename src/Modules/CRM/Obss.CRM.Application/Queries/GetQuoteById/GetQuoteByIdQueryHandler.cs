using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Queries.GetQuoteById;

public sealed class GetQuoteByIdQueryHandler : IRequestHandler<GetQuoteByIdQuery, Result<QuoteDto>>
{
    private readonly IQuoteRepository _repository;

    public GetQuoteByIdQueryHandler(IQuoteRepository repository) => _repository = repository;

    public async Task<Result<QuoteDto>> Handle(GetQuoteByIdQuery request, CancellationToken cancellationToken)
    {
        var quote = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (quote is null)
            return Result.Failure<QuoteDto>(Error.NotFound(nameof(Quote), request.Id));

        return Result.Success(quote.Adapt<QuoteDto>());
    }
}
