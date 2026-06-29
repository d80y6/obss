using Mapster;
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Queries.GetNumberById;

public sealed class GetNumberByIdQueryHandler : IRequestHandler<GetNumberByIdQuery, Result<TelephoneNumberDto>>
{
    private readonly ITelephoneNumberRepository _numberRepository;

    public GetNumberByIdQueryHandler(ITelephoneNumberRepository numberRepository)
    {
        _numberRepository = numberRepository;
    }

    public async Task<Result<TelephoneNumberDto>> Handle(GetNumberByIdQuery request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.Id, cancellationToken);

        if (number is null)
            return Result.Failure<TelephoneNumberDto>(Error.NotFound("TelephoneNumber", request.Id));

        return Result.Success(number.Adapt<TelephoneNumberDto>());
    }
}
