using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetAvailableIPs;

public sealed class GetAvailableIPsQueryHandler : IRequestHandler<GetAvailableIPsQuery, Result<IReadOnlyList<NetworkIpAddressDto>>>
{
    private readonly IRepository<NetworkElementIpAddress> _repository;

    public GetAvailableIPsQueryHandler(IRepository<NetworkElementIpAddress> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<NetworkIpAddressDto>>> Handle(GetAvailableIPsQuery request, CancellationToken cancellationToken)
    {
        var allIps = await _repository.GetAllAsync(cancellationToken);
        var available = allIps
            .Where(ip => ip.IsAvailable && !ip.IsReserved)
            .OrderBy(ip => ip.IPAddress)
            .ToList()
            .Adapt<List<NetworkIpAddressDto>>();

        return Result.Success<IReadOnlyList<NetworkIpAddressDto>>(available);
    }
}
