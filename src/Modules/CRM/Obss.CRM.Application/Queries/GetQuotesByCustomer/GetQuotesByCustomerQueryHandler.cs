using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Queries.GetQuotesByCustomer;

public sealed class GetQuotesByCustomerQueryHandler : IRequestHandler<GetQuotesByCustomerQuery, Result<List<QuoteDto>>>
{
    private readonly IQuoteRepository _repository;

    public GetQuotesByCustomerQueryHandler(IQuoteRepository repository) => _repository = repository;

    public async Task<Result<List<QuoteDto>>> Handle(GetQuotesByCustomerQuery request, CancellationToken cancellationToken)
    {
        var quotes = await _repository.GetByCustomerAsync(request.CustomerId, cancellationToken);
        return Result.Success(quotes.Adapt<List<QuoteDto>>());
    }
}
