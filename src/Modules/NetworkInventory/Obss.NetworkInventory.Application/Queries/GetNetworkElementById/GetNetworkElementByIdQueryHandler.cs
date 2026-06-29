using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetNetworkElementById;

public sealed class GetNetworkElementByIdQueryHandler : IRequestHandler<GetNetworkElementByIdQuery, Result<NetworkElementDto>>
{
    private readonly INetworkElementRepository _repository;

    public GetNetworkElementByIdQueryHandler(INetworkElementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<NetworkElementDto>> Handle(GetNetworkElementByIdQuery request, CancellationToken cancellationToken)
    {
        var element = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (element is null)
            return Result.Failure<NetworkElementDto>(Error.NotFound("NetworkElement", request.Id));

        return Result.Success(element.Adapt<NetworkElementDto>());
    }
}
