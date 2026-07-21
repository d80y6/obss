using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetNasById;

public sealed class GetNasByIdQueryHandler : IRequestHandler<GetNasByIdQuery, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;

    public GetNasByIdQueryHandler(INasRepository nasRepository)
    {
        _nasRepository = nasRepository;
    }

    public async Task<Result<NasDto>> Handle(GetNasByIdQuery request, CancellationToken cancellationToken)
    {
        var nas = await _nasRepository.GetByIdAsync(request.Id, cancellationToken);

        if (nas is null)
            return Result.Failure<NasDto>(Error.NotFound("NetworkAccessServer", request.Id));

        return Result.Success(nas.Adapt<NasDto>());
    }
}
