using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetSubnetById;

public sealed class GetSubnetByIdQueryHandler : IRequestHandler<GetSubnetByIdQuery, Result<SubnetDto>>
{
    private readonly IRepository<Subnet> _repository;

    public GetSubnetByIdQueryHandler(IRepository<Subnet> repository)
    {
        _repository = repository;
    }

    public async Task<Result<SubnetDto>> Handle(GetSubnetByIdQuery request, CancellationToken cancellationToken)
    {
        var subnet = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (subnet is null)
            return Result.Failure<SubnetDto>(Error.NotFound("Subnet", request.Id));

        return Result.Success(subnet.Adapt<SubnetDto>());
    }
}
