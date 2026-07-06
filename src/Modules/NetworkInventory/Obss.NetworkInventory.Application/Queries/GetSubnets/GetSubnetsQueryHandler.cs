using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetSubnets;

public sealed class GetSubnetsQueryHandler : IRequestHandler<GetSubnetsQuery, Result<IReadOnlyList<SubnetDto>>>
{
    private readonly IRepository<Subnet> _repository;

    public GetSubnetsQueryHandler(IRepository<Subnet> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<SubnetDto>>> Handle(GetSubnetsQuery request, CancellationToken cancellationToken)
    {
        var allSubnets = await _repository.GetAllAsync(cancellationToken);
        var filtered = allSubnets.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<Domain.ValueObjects.SubnetStatus>(request.Status, out var status))
        {
            filtered = filtered.Where(s => s.Status == status);
        }

        if (request.VLANId.HasValue)
        {
            filtered = filtered.Where(s => s.VLANId == request.VLANId.Value);
        }

        var result = filtered
            .OrderBy(s => s.Name)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToList()
            .Adapt<List<SubnetDto>>();

        return Result.Success<IReadOnlyList<SubnetDto>>(result);
    }
}
