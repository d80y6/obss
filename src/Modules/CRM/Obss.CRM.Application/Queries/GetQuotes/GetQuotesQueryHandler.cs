using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetQuotes;

public sealed class GetQuotesQueryHandler : IRequestHandler<GetQuotesQuery, Result<List<QuoteDto>>>
{
    private readonly IQuoteRepository _repository;

    public GetQuotesQueryHandler(IQuoteRepository repository) => _repository = repository;

    public async Task<Result<List<QuoteDto>>> Handle(GetQuotesQuery request, CancellationToken cancellationToken)
    {
        var quotes = await _repository.GetListAsync(cancellationToken);
        return Result.Success(quotes.Adapt<List<QuoteDto>>());
    }
}
