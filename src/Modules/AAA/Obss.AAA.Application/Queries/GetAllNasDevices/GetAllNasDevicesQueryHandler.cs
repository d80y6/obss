using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAllNasDevices;

public sealed class GetAllNasDevicesQueryHandler : IRequestHandler<GetAllNasDevicesQuery, Result<IReadOnlyList<NasDto>>>
{
    private readonly INasRepository _nasRepository;

    public GetAllNasDevicesQueryHandler(INasRepository nasRepository)
    {
        _nasRepository = nasRepository;
    }

    public async Task<Result<IReadOnlyList<NasDto>>> Handle(GetAllNasDevicesQuery request, CancellationToken cancellationToken)
    {
        var nasDevices = await _nasRepository.GetAllAsync(cancellationToken);
        return Result.Success(nasDevices.Adapt<IReadOnlyList<NasDto>>());
    }
}
