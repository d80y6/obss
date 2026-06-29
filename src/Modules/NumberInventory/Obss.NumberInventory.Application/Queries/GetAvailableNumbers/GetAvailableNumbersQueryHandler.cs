using Mapster;
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Queries.GetAvailableNumbers;

public sealed class GetAvailableNumbersQueryHandler : IRequestHandler<GetAvailableNumbersQuery, Result<IReadOnlyList<TelephoneNumberDto>>>
{
    private readonly ITelephoneNumberRepository _numberRepository;

    public GetAvailableNumbersQueryHandler(ITelephoneNumberRepository numberRepository)
    {
        _numberRepository = numberRepository;
    }

    public async Task<Result<IReadOnlyList<TelephoneNumberDto>>> Handle(GetAvailableNumbersQuery request, CancellationToken cancellationToken)
    {
        var numbers = await _numberRepository.GetAvailableNumbersAsync(
            request.NumberType,
            request.Prefix,
            cancellationToken);

        var dto = numbers.Adapt<IReadOnlyList<TelephoneNumberDto>>();

        return Result.Success(dto);
    }
}
