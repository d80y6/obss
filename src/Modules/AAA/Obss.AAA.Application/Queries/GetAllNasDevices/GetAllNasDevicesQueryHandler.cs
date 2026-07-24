using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAllNasDevices;

public sealed class GetAllNasDevicesQueryHandler : IRequestHandler<GetAllNasDevicesQuery, Result<PaginatedResult<NasDto>>>
{
    private readonly INasRepository _nasRepository;

    public GetAllNasDevicesQueryHandler(INasRepository nasRepository)
    {
        _nasRepository = nasRepository;
    }

    public async Task<Result<PaginatedResult<NasDto>>> Handle(GetAllNasDevicesQuery request, CancellationToken cancellationToken)
    {
        var allNas = await _nasRepository.GetAllAsync(cancellationToken);

        var filtered = string.IsNullOrEmpty(request.NasType)
            ? allNas
            : allNas.Where(n => n.NasType.ToString() == request.NasType).ToList();

        var total = filtered.Count;
        var paged = filtered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result.Success(new PaginatedResult<NasDto>(
            paged.Adapt<IReadOnlyList<NasDto>>(),
            total));
    }
}
