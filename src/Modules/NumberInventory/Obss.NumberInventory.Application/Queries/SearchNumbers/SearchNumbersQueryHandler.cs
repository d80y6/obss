using Mapster;
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Queries.SearchNumbers;

public sealed class SearchNumbersQueryHandler : IRequestHandler<SearchNumbersQuery, Result<IReadOnlyList<TelephoneNumberDto>>>
{
    private readonly ITelephoneNumberRepository _numberRepository;

    public SearchNumbersQueryHandler(ITelephoneNumberRepository numberRepository)
    {
        _numberRepository = numberRepository;
    }

    public async Task<Result<IReadOnlyList<TelephoneNumberDto>>> Handle(SearchNumbersQuery request, CancellationToken cancellationToken)
    {
        var numbers = await _numberRepository.SearchNumbersAsync(
            request.Prefix,
            request.Status,
            request.Type,
            request.Offset,
            request.Limit,
            cancellationToken);

        var dto = numbers.Adapt<IReadOnlyList<TelephoneNumberDto>>();

        return Result.Success(dto);
    }
}
